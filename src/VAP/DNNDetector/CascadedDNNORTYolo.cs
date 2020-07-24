// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using DNNDetector.Config;
using DNNDetector.Model;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using Utils.Config;
using Wrapper.ORT;

namespace DNNDetector
{
    public class CascadedDNNORTYolo
    {
        FrameDNNOnnxYolo frameDNNOnnxYolo;

        public CascadedDNNORTYolo(List<Tuple<string, int[]>> lines, string modelName)
        {
            frameDNNOnnxYolo = new FrameDNNOnnxYolo(lines, modelName, DNNMode.CC);

            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderCcDNN);
        }

        public List<Item> Run(int frameIndex, List<Item> ltDNNItemList, List<Tuple<string, int[]>> lines, Dictionary<string, int> category, ref long teleCountsHeavyDNN, bool savePictures = false)
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
                    List<Item> analyzedTrackingItems = null;

                    Console.WriteLine("** Calling Heavy DNN **");
                    analyzedTrackingItems = frameDNNOnnxYolo.Run(Cv2.ImDecode(ltDNNItem.RawImageData, ImreadModes.Color), frameIndex, category, System.Drawing.Brushes.Yellow, ltDNNItem.TriggerLineID, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE);
                    teleCountsHeavyDNN++;

                    // object detected by heavy YOLO
                    if (analyzedTrackingItems != null)
                    {
                        foreach (Item item in analyzedTrackingItems)
                        {
                            item.RawImageData = ltDNNItem.RawImageData;
                            item.TriggerLine = ltDNNItem.TriggerLine;
                            item.TriggerLineID = ltDNNItem.TriggerLineID;
                            item.Model = "Heavy";
                            ccDNNItem.Add(item);

                            // output heavy YOLO results
                            if (savePictures)
                            {
                                string blobName_Heavy = $@"frame-{frameIndex}-Heavy-{item.Confidence}.jpg";
                                string fileName_Heavy = @OutputFolder.OutputFolderCcDNN + blobName_Heavy;
                                File.WriteAllBytes(fileName_Heavy, item.TaggedImageData);
                                File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_Heavy, item.TaggedImageData);
                            }

                            return ccDNNItem; // if we only return the closest object detected by heavy model
                        }
                    }
                    else
                    {
                        Console.WriteLine("** Not detected by Heavy DNN **");
                    }
                }
            }

            return ccDNNItem;
        }
    }
}
