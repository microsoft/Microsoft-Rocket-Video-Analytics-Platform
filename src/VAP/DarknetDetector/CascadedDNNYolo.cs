// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using DNNDetector.Config;
using DNNDetector.Model;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using Utils.Config;
using Wrapper.Yolo;
using Wrapper.Yolo.Model;

namespace DarknetDetector
{
    public class CascadedDNNDarknet
    {
        static string YOLOCONFIG = "YoloV3Coco"; // "cheap" yolo config folder name
        FrameDNNDarknet frameDNNYolo;
        FrameBuffer frameBufferCcDNN;

        public CascadedDNNDarknet(double rFactor)
        {
            frameBufferCcDNN = new FrameBuffer(DNNConfig.FRAME_SEARCH_RANGE);

            frameDNNYolo = new FrameDNNDarknet(YOLOCONFIG, DNNMode.CC, rFactor);
        }

        public CascadedDNNDarknet(List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines)
        {
            frameBufferCcDNN = new FrameBuffer(DNNConfig.FRAME_SEARCH_RANGE);

            frameDNNYolo = new FrameDNNDarknet(YOLOCONFIG, DNNMode.CC, lines);
        }

        public List<Item> Run(Mat frame, int frameIndex, List<Item> ltDNNItemList, List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines, HashSet<string> category)
        {
            if (ltDNNItemList == null)
            {
                return null;
            }

            List<Item> ccDNNItem = new List<Item>();

            foreach (Item ltDNNItem in ltDNNItemList)
            {
                if (ltDNNItem.Confidence >= DNNConfig.CONFIDENCE_THRESHOLD)
                {
                    ccDNNItem.Add(ltDNNItem);
                    continue;
                }
                else
                {
                    List<YoloTrackingItem> analyzedTrackingItems = null;
                    frameDNNYolo.SetTrackingPoint(new System.Drawing.Point((int)((lines[ltDNNItem.TriggerLineID].coordinates.p1.X + lines[ltDNNItem.TriggerLineID].coordinates.p2.X) / 2),
                                                                (int)((lines[ltDNNItem.TriggerLineID].coordinates.p1.Y + lines[ltDNNItem.TriggerLineID].coordinates.p2.Y) / 2))); //only needs to check the last line in each row
                    byte[] imgByte = ltDNNItem.RawImageData;

                    Console.WriteLine("** Calling Heavy");
                    analyzedTrackingItems = frameDNNYolo.Detect(imgByte, category, ltDNNItem.TriggerLineID, System.Drawing.Brushes.Red, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_SMALL, frameIndex);

                    // object detected by heavy YOLO
                    if (analyzedTrackingItems != null)
                    {
                        foreach (YoloTrackingItem yoloTrackingItem in analyzedTrackingItems)
                        {
                            Item item = Item(yoloTrackingItem);
                            item.RawImageData = imgByte;
                            item.TriggerLine = ltDNNItem.TriggerLine;
                            item.TriggerLineID = ltDNNItem.TriggerLineID;
                            item.Model = "Heavy";
                            ccDNNItem.Add(item);
                            
                            // output heavy YOLO results
                            string blobName_Heavy = $@"frame-{frameIndex}-Heavy-{yoloTrackingItem.Confidence}.jpg";
                            string fileName_Heavy = @OutputFolder.OutputFolderCcDNN + blobName_Heavy;
                            File.WriteAllBytes(fileName_Heavy, yoloTrackingItem.TaggedImageData);
                            File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_Heavy, yoloTrackingItem.TaggedImageData);

                            return ccDNNItem; // if we only return the closest object detected by heavy model
                        }
                    }
                    else
                    {
                        Console.WriteLine("**Not detected by Heavy");
                    }
                }
            }

            return ccDNNItem;
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
