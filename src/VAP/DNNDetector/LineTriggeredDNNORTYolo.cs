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
using Wrapper.ORT;

namespace DNNDetector
{
    //Todo: merge it with LineTriggeredDNNYolo
    public class LineTriggeredDNNORTYolo
    {
        Dictionary<string, int> counts_prev = new Dictionary<string, int>();

        FrameDNNOnnxYolo frameDNNOnnxYolo;
        FrameBuffer frameBufferLtDNNOnnxYolo;

        public LineTriggeredDNNORTYolo(List<Tuple<string, int[]>> lines, string modelName)
        {
            frameBufferLtDNNOnnxYolo = new FrameBuffer(DNNConfig.FRAME_SEARCH_RANGE);

            frameDNNOnnxYolo = new FrameDNNOnnxYolo(lines, modelName, DNNMode.LT);

            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderLtDNN);
            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderFrameDNNONNX);
        }

        public List<Item> Run(Mat frame, int frameIndex, Dictionary<string, int> counts, List<Tuple<string, int[]>> lines, Dictionary<string, int> category, ref long teleCountsCheapDNN, bool savePictures = false)
        {
            // buffer frame
            frameBufferLtDNNOnnxYolo.Buffer(frame);

            if (counts_prev.Count != 0)
            {
                foreach (string lane in counts.Keys)
                {
                    int diff = Math.Abs(counts[lane] - counts_prev[lane]);
                    if (diff > 0) //object detected by BGS
                    {
                        if (frameIndex >= DNNConfig.FRAME_SEARCH_RANGE)
                        {
                            // call onnx cheap model for crosscheck
                            int lineID = Array.IndexOf(counts.Keys.ToArray(), lane);
                            Mat[] frameBufferArray = frameBufferLtDNNOnnxYolo.ToArray();
                            int frameIndexOnnxYolo = frameIndex - 1;
                            List<Item> analyzedTrackingItems = null;

                            while (frameIndex - frameIndexOnnxYolo < DNNConfig.FRAME_SEARCH_RANGE)
                            {
                                Console.WriteLine("** Calling DNN on " + (DNNConfig.FRAME_SEARCH_RANGE - (frameIndex - frameIndexOnnxYolo)));
                                Mat frameOnnx = frameBufferArray[DNNConfig.FRAME_SEARCH_RANGE - (frameIndex - frameIndexOnnxYolo)];

                                analyzedTrackingItems = frameDNNOnnxYolo.Run(frameOnnx, (DNNConfig.FRAME_SEARCH_RANGE - (frameIndex - frameIndexOnnxYolo)), category, System.Drawing.Brushes.Pink, lineID, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE);
                                teleCountsCheapDNN++;

                                // object detected by cheap model
                                if (analyzedTrackingItems != null)
                                {
                                    List<Item> ltDNNItem = new List<Item>();
                                    foreach (Item item in analyzedTrackingItems)
                                    {
                                        item.RawImageData = Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frameOnnx));
                                        item.TriggerLine = lane;
                                        item.TriggerLineID = lineID;
                                        item.Model = "Cheap";
                                        ltDNNItem.Add(item);

                                        // output cheap onnx results
                                        if (savePictures)
                                        {
                                            string blobName_Cheap = $@"frame-{frameIndex}-DNN-{item.Confidence}.jpg";
                                            string fileName_Cheap = @OutputFolder.OutputFolderLtDNN + blobName_Cheap;
                                            File.WriteAllBytes(fileName_Cheap, item.TaggedImageData);
                                            File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_Cheap, item.TaggedImageData);
                                        }
                                    }
                                    updateCount(counts);
                                    return ltDNNItem;
                                }
                                frameIndexOnnxYolo--;
                            }
                        }
                    }
                }
            }
            updateCount(counts);
            return null;
        }

        void updateCount(Dictionary<string, int> counts)
        {
            foreach (string dir in counts.Keys)
            {
                counts_prev[dir] = counts[dir];
            }
        }
    }
}
