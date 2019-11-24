// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using DNNDetector.Config;
using DNNDetector.Model;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.Config;

namespace TFDetector
{
    public class LineTriggeredDNNTF
    {
        //static string TFCONFIG = "";
        FrameDNNTF frameDNNTF;
        FrameBuffer frameBufferLtDNNTF;

        public LineTriggeredDNNTF(List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines)
        {
            frameBufferLtDNNTF = new FrameBuffer(DNNConfig.FRAME_SEARCH_RANGE);

            frameDNNTF = new FrameDNNTF(lines);

            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderBGSLine);
            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderLtDNN);
        }

        public List<Item> Run(Mat frame, int frameIndex, Dictionary<string, bool> occupancy, List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines, HashSet<string> category)
        {
            // buffer frame
            frameBufferLtDNNTF.Buffer(frame);

            foreach (string lane in occupancy.Keys)
            {
                if (occupancy[lane]) //object detected by BGS
                {
                    if (frameIndex >= DNNConfig.FRAME_SEARCH_RANGE)
                    {
                        // call tf cheap model for crosscheck
                        int lineID = Array.IndexOf(occupancy.Keys.ToArray(), lane);
                        Mat[] frameBufferArray = frameBufferLtDNNTF.ToArray();
                        int frameIndexTF = frameIndex - 1;
                        DateTime start = DateTime.Now;
                        List<Item> analyzedTrackingItems = null;

                        while (frameIndex - frameIndexTF < DNNConfig.FRAME_SEARCH_RANGE)
                        {
                            Console.WriteLine("** Calling Cheap on " + (DNNConfig.FRAME_SEARCH_RANGE - (frameIndex - frameIndexTF)));
                            Mat frameTF = frameBufferArray[DNNConfig.FRAME_SEARCH_RANGE - (frameIndex - frameIndexTF)];

                            analyzedTrackingItems = frameDNNTF.Run(frameTF, frameIndexTF, category, System.Drawing.Brushes.Pink, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE);

                            // object detected by cheap model
                            if (analyzedTrackingItems != null)
                            {
                                List<Item> ltDNNItem = new List<Item>();
                                foreach (Item item in analyzedTrackingItems)
                                {
                                    item.RawImageData = Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frameTF));
                                    item.TriggerLine = lane;
                                    item.TriggerLineID = lineID;
                                    item.Model = "Cheap";
                                    ltDNNItem.Add(item);

                                    // output cheap TF results
                                    string blobName_Cheap = $@"frame-{frameIndex}-Cheap-{item.Confidence}.jpg";
                                    string fileName_Cheap = @OutputFolder.OutputFolderLtDNN + blobName_Cheap;
                                    File.WriteAllBytes(fileName_Cheap, item.TaggedImageData);
                                    File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_Cheap, item.TaggedImageData);
                                }
                                return ltDNNItem;
                            }
                            frameIndexTF--;
                        }
                    }
                }
            }

            return null;
        }
    }
}
