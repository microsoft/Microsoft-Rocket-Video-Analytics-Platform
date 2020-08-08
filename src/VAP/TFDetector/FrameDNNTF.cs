// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using DNNDetector.Config;
using DNNDetector.Model;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Utils.Config;
using Wrapper.TF;
using Wrapper.TF.Common;

namespace TFDetector
{
    public class FrameDNNTF
    {
        private static int _imageWidth, _imageHeight, _index;
        private static List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> _lines;
        private static HashSet<string> _category;

        TFWrapper tfWrapper = new TFWrapper();
        byte[] imageByteArray;
        Brush bboxColor = Brushes.Green;

        public FrameDNNTF(List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines)
        {
            _lines = lines;
        }

        public List<Item> Run(Mat frameTF, int frameIndex, HashSet<string> category, Brush bboxColor, double min_score_for_linebbox_overlap, bool saveImg = true)
        {
            _imageWidth = frameTF.Width;
            _imageHeight = frameTF.Height;
            _category = category;
            imageByteArray = Utils.Utils.ImageToByteJpeg(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frameTF));

            float[,,] boxes;
            float[,] scores, classes;
            (boxes, scores, classes) = tfWrapper.Run(imageByteArray);
            
            List<Item> preValidItems = ValidateItems(boxes, scores, classes, DNNConfig.MIN_SCORE_FOR_TFOBJECT_OUTPUT);
            List<Item> validObjects = new List<Item>();

            //run overlap ratio-based validation
            if (_lines != null)
            {
                for (int lineID = 0; lineID < _lines.Count; lineID++)
                {
                    var overlapItems = preValidItems.Select(o => new { Overlap = Utils.Utils.checkLineBboxOverlapRatio(_lines[lineID].coordinates, o.X, o.Y, o.Width, o.Height), Bbox_x = o.X + o.Width, Bbox_y = o.Y + o.Height, Distance = this.Distance(_lines[lineID].coordinates, o.Center()), Item = o })
                        .Where(o => o.Bbox_x <= _imageWidth && o.Bbox_y <= _imageHeight && o.Overlap >= min_score_for_linebbox_overlap).OrderBy(o => o.Distance);
                    foreach (var item in overlapItems)
                    {
                        item.Item.TaggedImageData = Utils.Utils.DrawImage(imageByteArray, item.Item.X, item.Item.Y, item.Item.Width, item.Item.Height, bboxColor);
                        item.Item.CroppedImageData = Utils.Utils.CropImage(imageByteArray, item.Item.X, item.Item.Y, item.Item.Width, item.Item.Height);
                        item.Item.Index = _index;
                        validObjects.Add(item.Item);
                        _index++;
                    }
                }
            }

            // output tf results
            if (saveImg)
            {
                foreach (Item it in validObjects)
                {
                    string blobName_TF = $@"frame-{frameIndex}-TF-{it.Confidence}.jpg";
                    string fileName_TF = @OutputFolder.OutputFolderFrameDNNTF + blobName_TF;
                    File.WriteAllBytes(fileName_TF, it.TaggedImageData);
                    File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_TF, it.TaggedImageData);

                    using (Image image = Image.FromStream(new MemoryStream(it.TaggedImageData)))
                    {
                        image.Save(@OutputFolder.OutputFolderFrameDNNTF + $"frame-{frameIndex}-TF-{it.Confidence}.jpg", ImageFormat.Jpeg);
                        image.Save(@OutputFolder.OutputFolderAll + $"frame-{frameIndex}-TF-{it.Confidence}.jpg", ImageFormat.Jpeg);
                    }
                }
            }

            return (validObjects.Count == 0 ? null : validObjects);
        }

        List<Item> ValidateItems(float[,,] boxes, float[,] scores, float[,] classes, double minScore)
        {
            List<Item> frameDNNItem = new List<Item>();

            var x = boxes.GetLength(0);
            var y = boxes.GetLength(1);
            var z = boxes.GetLength(2);

            float ymin = 0, xmin = 0, ymax = 0, xmax = 0;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (scores[i, j] < minScore) continue;

                    int value = Convert.ToInt32(classes[i, j]);
                    CatalogItem catalogItem = TFWrapper._catalog.FirstOrDefault(item => item.Id == value);
                    if (_category.Count > 0 && !_category.Contains(catalogItem.DisplayName)) continue;

                    for (int k = 0; k < z; k++)
                    {
                        var box = boxes[i, j, k];
                        switch (k)
                        {
                            case 0:
                                ymin = box;
                                break;
                            case 1:
                                xmin = box;
                                break;
                            case 2:
                                ymax = box;
                                break;
                            case 3:
                                xmax = box;
                                break;
                        }
                    }

                    int bbox_x = (int)(xmin * _imageWidth), bbox_y = (int)(ymin * _imageHeight), 
                        bbox_w = (int)((xmax - xmin) * _imageWidth), bbox_h = (int)((ymax - ymin) * _imageHeight);
                    
                    //check line overlap
                    for (int lineID = 0; lineID < _lines.Count; lineID++)
                    {
                        float ratio = Utils.Utils.checkLineBboxOverlapRatio(_lines[lineID].coordinates, bbox_x, bbox_y, bbox_w, bbox_h);
                        if (ratio >= DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_SMALL)
                        {
                            Item it = new Item(bbox_x, bbox_y, bbox_w, bbox_h, catalogItem.Id, catalogItem.DisplayName, scores[i, j], lineID, _lines[lineID].key);
                            it.TaggedImageData = Utils.Utils.DrawImage(imageByteArray, bbox_x, bbox_y, bbox_w, bbox_h, bboxColor);
                            it.CroppedImageData = Utils.Utils.CropImage(imageByteArray, bbox_x, bbox_y, bbox_w, bbox_h);
                            frameDNNItem.Add(it);
                            break;
                        }
                    }
                }
            }

            return frameDNNItem;
        }

        private double Distance((System.Drawing.Point p1, System.Drawing.Point p2) line, System.Drawing.Point bboxCenter)
        {
            System.Drawing.Point p1 = new System.Drawing.Point((int)((line.p1.X + line.p2.X) / 2), (int)((line.p1.Y + line.p2.Y) / 2));
            return Math.Sqrt(this.Pow2(bboxCenter.X - p1.X) + Pow2(bboxCenter.Y - p1.Y));
        }

        private double Pow2(double x)
        {
            return x * x;
        }
    }
}
