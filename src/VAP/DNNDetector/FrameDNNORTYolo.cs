// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using DNNDetector.Model;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Utils.Config;
using Wrapper.ORT;

namespace DNNDetector
{
    public class FrameDNNOnnxYolo
    {
        private static int _imageWidth, _imageHeight, _index;
        private static List<Tuple<string, int[]>> _lines;
        private static Dictionary<string, int> _category;

        ORTWrapper onnxWrapper;
        byte[] imageByteArray;

        public FrameDNNOnnxYolo(List<Tuple<string, int[]>> lines, string modelName, DNNMode mode)
        {
            _lines = lines;
            onnxWrapper = new ORTWrapper($@"..\..\..\..\..\..\modelOnnx\{modelName}ort.onnx", mode);
        }

        public List<Item> Run(Mat frameOnnx, int frameIndex, Dictionary<string, int> category, Brush bboxColor, int lineID, double min_score_for_linebbox_overlap, bool savePictures = false)
        {
            _imageWidth = frameOnnx.Width;
            _imageHeight = frameOnnx.Height;
            _category = category;
            imageByteArray = Utils.Utils.ImageToByteJpeg(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frameOnnx)); // Todo: feed in bmp

            List<ORTItem> boundingBoxes = onnxWrapper.UseApi(
                    OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frameOnnx),
                    _imageHeight,
                    _imageWidth);

            List<Item> preValidItems = new List<Item>();
            foreach (ORTItem bbox in boundingBoxes)
            {
                preValidItems.Add(new Item(bbox));
            }
            List<Item> validObjects = new List<Item>();

            //run _category and overlap ratio-based validation
            if (_lines != null)
            {
                var overlapItems = preValidItems.Select(o => new { Overlap = Utils.Utils.checkLineBboxOverlapRatio(_lines[lineID].Item2, o.X, o.Y, o.Width, o.Height), Bbox_x = o.X + o.Width, Bbox_y = o.Y + o.Height, Distance = this.Distance(_lines[lineID].Item2, o.Center()), Item = o })
                    .Where(o => o.Bbox_x <= _imageWidth && o.Bbox_y <= _imageHeight && o.Overlap >= min_score_for_linebbox_overlap && _category.ContainsKey(o.Item.ObjName)).OrderBy(o => o.Distance);
                foreach (var item in overlapItems)
                {
                    item.Item.TaggedImageData = Utils.Utils.DrawImage(imageByteArray, item.Item.X, item.Item.Y, item.Item.Width, item.Item.Height, bboxColor);
                    item.Item.CroppedImageData = Utils.Utils.CropImage(imageByteArray, item.Item.X, item.Item.Y, item.Item.Width, item.Item.Height);
                    item.Item.Index = _index;
                    item.Item.TriggerLine = _lines[lineID].Item1;
                    item.Item.TriggerLineID = lineID;
                    item.Item.Model = "FrameDNN";
                    validObjects.Add(item.Item);
                    _index++;
                }
            }
            else if (min_score_for_linebbox_overlap == 0.0) //frameDNN object detection
            {
                var overlapItems = preValidItems.Select(o => new { Bbox_x = o.X + o.Width, Bbox_y = o.Y + o.Height, Item = o })
                    .Where(o => o.Bbox_x <= _imageWidth && o.Bbox_y <= _imageHeight && _category.ContainsKey(o.Item.ObjName));
                foreach (var item in overlapItems)
                {
                    item.Item.TaggedImageData = Utils.Utils.DrawImage(imageByteArray, item.Item.X, item.Item.Y, item.Item.Width, item.Item.Height, bboxColor);
                    item.Item.CroppedImageData = Utils.Utils.CropImage(imageByteArray, item.Item.X, item.Item.Y, item.Item.Width, item.Item.Height);
                    item.Item.Index = _index;
                    item.Item.TriggerLine = "";
                    item.Item.TriggerLineID = -1;
                    item.Item.Model = "FrameDNN";
                    validObjects.Add(item.Item);
                    _index++;
                }
            }

            //output onnxyolo results
            if (savePictures)
            {
                foreach (Item it in validObjects)
                {
                    using (Image image = Image.FromStream(new MemoryStream(it.TaggedImageData)))
                    {

                        image.Save(@OutputFolder.OutputFolderFrameDNNONNX + $"frame-{frameIndex}-ONNX-{it.Confidence}.jpg", ImageFormat.Jpeg);
                        image.Save(@OutputFolder.OutputFolderAll + $"frame-{frameIndex}-ONNX-{it.Confidence}.jpg", ImageFormat.Jpeg);
                    }
                }
                //byte[] imgBboxes = DrawAllBb(frameIndex, Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frameOnnx)),
                //        validObjects, Brushes.Pink);
            }

            return (validObjects.Count == 0 ? null : validObjects);
        }

        private double Distance(int[] line, System.Drawing.Point bboxCenter)
        {
            System.Drawing.Point p1 = new System.Drawing.Point((int)((line[0] + line[2]) / 2), (int)((line[1] + line[3]) / 2));
            return Math.Sqrt(this.Pow2(bboxCenter.X - p1.X) + Pow2(bboxCenter.Y - p1.Y));
        }

        private double Pow2(double x)
        {
            return x * x;
        }

        public static byte[] DrawAllBb(int frameIndex, byte[] imgByte, List<Item> items, Brush bboxColor)
        {
            byte[] canvas = new byte[imgByte.Length];
            canvas = imgByte;
            foreach (var item in items)
            {
                canvas = Utils.Utils.DrawImage(canvas, item.X, item.Y, item.Width, item.Height, bboxColor);
            }
            string frameIndexString = frameIndex.ToString("000000.##");
            File.WriteAllBytes(@OutputFolder.OutputFolderFrameDNNONNX + $@"frame{frameIndexString}-Raw.jpg", canvas);

            return canvas;
        }
    }
}
