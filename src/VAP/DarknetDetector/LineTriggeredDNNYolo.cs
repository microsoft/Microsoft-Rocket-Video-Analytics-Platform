// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using DNNDetector.Model;
using DNNDetector.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenCvSharp;
using Utils.Config;
using Wrapper.Yolo;
using Wrapper.Yolo.Model;


namespace DarknetDetector
{
    public class LineTriggeredDNNDarknet
    {
        static string YOLOCONFIG = "YoloV3TinyCoco"; // "cheap" yolo config folder name
        FrameDNNDarknet frameDNNYolo;
        FrameBuffer frameBufferLtDNNYolo;

        public LineTriggeredDNNDarknet(double rFactor)
        {
            frameBufferLtDNNYolo = new FrameBuffer(DNNConfig.FRAME_SEARCH_RANGE);

            frameDNNYolo = new FrameDNNDarknet(YOLOCONFIG, DNNMode.LT, rFactor);

            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderBGSLine);
            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderLtDNN);
        }

        public LineTriggeredDNNDarknet(List<Tuple<string, int[]>> lines)
        {
            frameBufferLtDNNYolo = new FrameBuffer(DNNConfig.FRAME_SEARCH_RANGE);

            frameDNNYolo = new FrameDNNDarknet(YOLOCONFIG, DNNMode.LT, lines);

            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderBGSLine);
            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderLtDNN);
        }

        public List<Item> Run(Mat frame, int frameIndex, Dictionary<string, bool> occupancy, List<Tuple<string, int[]>> lines, HashSet<string> category)
        {
            // buffer frame
            frameBufferLtDNNYolo.Buffer(frame);

            foreach (string lane in occupancy.Keys)
            {
                if (occupancy[lane]) //object detected by BGS
                {
                    if (frameIndex >= DNNConfig.FRAME_SEARCH_RANGE)
                    {
                        // call yolo for crosscheck
                        int lineID = Array.IndexOf(occupancy.Keys.ToArray(), lane);
                        int subLineID = lines[lineID].Item2.Length;
                        frameDNNYolo.SetTrackingPoint(new System.Drawing.Point((int)((lines[lineID].Item2[subLineID - 4] + lines[lineID].Item2[subLineID - 2]) / 2),
                                                            (int)((lines[lineID].Item2[subLineID - 3] + lines[lineID].Item2[subLineID - 1]) / 2))); //only needs to check the last line in each row
                        Mat[] frameBufferArray = frameBufferLtDNNYolo.ToArray();
                        int frameIndexYolo = frameIndex - 1;
                        DateTime start = DateTime.Now;
                        List<YoloTrackingItem> analyzedTrackingItems = null;

                        while (frameIndex - frameIndexYolo < DNNConfig.FRAME_SEARCH_RANGE)
                        {
                            Console.WriteLine("** Calling Cheap on " + (DNNConfig.FRAME_SEARCH_RANGE - (frameIndex - frameIndexYolo)));
                            Mat frameYolo = frameBufferArray[DNNConfig.FRAME_SEARCH_RANGE - (frameIndex - frameIndexYolo)];
                            byte[] imgByte = Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frameYolo));

                            analyzedTrackingItems = frameDNNYolo.Detect(imgByte, category, lineID, System.Drawing.Brushes.Pink, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE, frameIndexYolo);

                            // object detected by cheap YOLO
                            if (analyzedTrackingItems != null)
                            {
                                List<Item> ltDNNItem = new List<Item>();
                                foreach (YoloTrackingItem yoloTrackingItem in analyzedTrackingItems)
                                {
                                    Item item = Item(yoloTrackingItem);
                                    item.RawImageData = imgByte;
                                    item.TriggerLine = lane;
                                    item.TriggerLineID = lineID;
                                    item.Model = "Cheap";
                                    ltDNNItem.Add(item);

                                    // output cheap YOLO results
                                    string blobName_Cheap = $@"frame-{frameIndex}-Cheap-{yoloTrackingItem.Confidence}.jpg";
                                    string fileName_Cheap = @OutputFolder.OutputFolderLtDNN + blobName_Cheap;
                                    File.WriteAllBytes(fileName_Cheap, yoloTrackingItem.TaggedImageData);
                                    File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_Cheap, yoloTrackingItem.TaggedImageData);
                                }
                                return ltDNNItem;
                            }
                            frameIndexYolo--;
                        }
                    }
                }
            }

            return null;
        }

        Item Item(YoloTrackingItem yoloTrackingItem)
        {
            Item item = new Item(yoloTrackingItem.X, yoloTrackingItem.Y, yoloTrackingItem.Width, yoloTrackingItem.Height,
                yoloTrackingItem.ObjId, yoloTrackingItem.Type, yoloTrackingItem.Confidence, 0, "");

            item.TrackId = yoloTrackingItem.TrackId;
            item.Index = yoloTrackingItem.Index;
            item.TaggedImageData = yoloTrackingItem.TaggedImageData;
            item.CroppedImageData = yoloTrackingItem.CroppedImageData;

            return item;
        }
    }
}
