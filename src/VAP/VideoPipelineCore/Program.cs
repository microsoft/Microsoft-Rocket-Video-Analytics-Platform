// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using AML.Client;
using BGSObjectDetector;
using DarknetDetector;
using DNNDetector.Model;
using LineDetector;
using OpenCvSharp;
using PostProcessor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using TFDetector;
using Utils.Config;

namespace VideoPipelineCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //parse arguments
            if (args.Length < 5)
            {
                Console.WriteLine(args.Length);
                Console.WriteLine("Usage: <exe> <video url> <cfg file> <samplingFactor> <resolutionFactor> <category1> <category2> ...");
                return;
            }

            string videoUrl = args[0];
            string[] videoUrls;
            bool isVideoStream;
            if (videoUrl.Substring(0, 4) == "rtmp" || videoUrl.Substring(0, 4) == "http" || videoUrl.Substring(0, 3) == "mms" || videoUrl.Substring(0, 4) == "rtsp")
            {
                videoUrls = new[] { videoUrl };
                isVideoStream = true;
            }
            else
            {
                isVideoStream = false;
                videoUrls = Directory.GetFiles(@"..\..\..\..\..\..\media\", args[0]);
            }

            string lineFile = @"..\..\..\..\..\..\cfg\" + args[1];
            int SAMPLING_FACTOR = int.Parse(args[2]);
            double RESOLUTION_FACTOR = double.Parse(args[3]);

            Dictionary<string, int> category = new Dictionary<string, int>();
            for (int i = 4; i < args.Length; i++)
            {
                category.Add(args[i], 0);
            }

            //initialize pipeline settings
            int pplConfig = Convert.ToInt16(ConfigurationManager.AppSettings["PplConfig"]);
            bool loop = false;
            bool displayRawVideo = true;
            bool displayBGSVideo = false;
            Utils.Utils.cleanFolder(@OutputFolder.OutputFolderAll);

            //create pipeline components (initialization based on pplConfig)

            //-----Decoder-----
            bool skipLastFrame = !isVideoStream; //skip the last frame which could be wrongly encoded from vlc capture
            Decoder.Decoder decoder = new Decoder.Decoder(videoUrls, loop, skipLastFrame);

            //-----Background Subtraction-based Detector-----
            BGSObjectDetector.BGSObjectDetector bgs = new BGSObjectDetector.BGSObjectDetector();

            //-----Line Detector-----
            Detector lineDetector = new Detector(SAMPLING_FACTOR, RESOLUTION_FACTOR, lineFile, displayBGSVideo);
            Dictionary<string, bool> occupancy = null;
            List<Tuple<string, int[]>> lines = lineDetector.multiLaneDetector.getAllLines();

            //-----LineTriggeredDNN (Darknet)-----
            LineTriggeredDNNDarknet ltDNNDarknet = null;
            List<Item> ltDNNItemListDarknet = null;
            if (new int[] { 3, 4 }.Contains(pplConfig))
            {
                ltDNNDarknet = new LineTriggeredDNNDarknet(lines);
                ltDNNItemListDarknet = new List<Item>();
            }

            //-----LineTriggeredDNN (TensorFlow)-----
            LineTriggeredDNNTF ltDNNTF = null;
            List<Item> ltDNNItemListTF = null;
            if (new int[] { 5 }.Contains(pplConfig))
            {
                ltDNNTF = new LineTriggeredDNNTF(lines);
                ltDNNItemListTF = new List<Item>();
            }

            //-----CascadedDNN (Darknet)-----
            CascadedDNNDarknet ccDNNDarknet = null;
            List<Item> ccDNNItemList = null;
            if (new int[] { 3 }.Contains(pplConfig))
            {
                ccDNNDarknet = new CascadedDNNDarknet(lines);
                ccDNNItemList = new List<Item>();
            }

            //-----DNN on every frame (Darknet)-----
            FrameDNNDarknet frameDNNDarknet = null;
            List<Item> frameDNNDarknetItemList = null;
            if (new int[] { 1 }.Contains(pplConfig))
            {
                frameDNNDarknet = new FrameDNNDarknet("YoloV3TinyCoco", Wrapper.Yolo.DNNMode.Frame, lines);
                frameDNNDarknetItemList = new List<Item>();
                Utils.Utils.cleanFolder(@OutputFolder.OutputFolderFrameDNNDarknet);
            }

            //-----DNN on every frame (TensorFlow)-----
            FrameDNNTF frameDNNTF = null;
            List<Item> frameDNNTFItemList = null;
            if (new int[] { 2 }.Contains(pplConfig))
            {
                frameDNNTF = new FrameDNNTF(lines);
                frameDNNTFItemList = new List<Item>();
                Utils.Utils.cleanFolder(@OutputFolder.OutputFolderFrameDNNTF);
            }

            //-----Call ML models deployed on Azure Machine Learning Workspace-----
            AMLCaller amlCaller = null;
            List<bool> amlConfirmed;
            if (new int[] { 5 }.Contains(pplConfig))
            {
                amlCaller = new AMLCaller(ConfigurationManager.AppSettings["AMLHost"],
                Convert.ToBoolean(ConfigurationManager.AppSettings["AMLSSL"]),
                ConfigurationManager.AppSettings["AMLAuthKey"],
                ConfigurationManager.AppSettings["AMLServiceID"]);
            }

            //-----Write to DB-----
            List<Item> ItemList = null;

            int frameIndex = 0;
            int? videoTotalFrame = 0;
            if (!isVideoStream)
                videoTotalFrame = decoder.getTotalFrameNum() - 1;


            //RUN PIPELINE 
            DateTime startTime = DateTime.Now;
            DateTime prevTime = DateTime.Now;
            while (true)
            {
                //decoder
                Mat frame = decoder.getNextFrame();
                if (frame is null)
                    break;

                
                //frame pre-processor
                frame = FramePreProcessor.PreProcessor.returnFrame(frame, frameIndex, SAMPLING_FACTOR, RESOLUTION_FACTOR, displayRawVideo);
                frameIndex++;
                if (frame == null) continue;
                //Console.WriteLine("Frame ID: " + frameIndex);


                //background subtractor
                Mat fgmask = null;
                List<Box> foregroundBoxes = bgs.DetectObjects(DateTime.Now, frame, frameIndex, out fgmask);
                

                //line detector
                if (new int[] { 0, 3, 4, 5 }.Contains(pplConfig))
                {
                    occupancy = lineDetector.updateLineOccupancy(frame, frameIndex, fgmask, foregroundBoxes);
                }


                //cheap DNN
                if (new int[] { 3, 4 }.Contains(pplConfig))
                {
                    ltDNNItemListDarknet = ltDNNDarknet.Run(frame, frameIndex, occupancy, lines, category);
                    ItemList = ltDNNItemListDarknet;
                }
                else if (new int[] { 5 }.Contains(pplConfig))
                {
                    ltDNNItemListTF = ltDNNTF.Run(frame, frameIndex, occupancy, lines, category);
                    ItemList = ltDNNItemListTF;
                }


                //heavy DNN
                if (new int[] { 3 }.Contains(pplConfig))
                {
                    ccDNNItemList = ccDNNDarknet.Run(frame, frameIndex, ltDNNItemListDarknet, lines, category);
                    ItemList = ccDNNItemList;
                }


                //frameDNN with Darknet Yolo
                if (new int[] { 1 }.Contains(pplConfig))
                {
                    frameDNNDarknetItemList = frameDNNDarknet.Run(Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame)), frameIndex, lines, category, System.Drawing.Brushes.Pink);
                    ItemList = frameDNNDarknetItemList;
                }


                //frame DNN TF
                if (new int[] { 2 }.Contains(pplConfig))
                {
                    frameDNNTFItemList = frameDNNTF.Run(frame, frameIndex, category, System.Drawing.Brushes.Pink, 0.2);
                    ItemList = frameDNNTFItemList;
                }


                //Azure Machine Learning
                if (new int[] { 5 }.Contains(pplConfig))
                {
                    amlConfirmed = AMLCaller.Run(frameIndex, ItemList, category).Result;
                }


                //DB Write
                if (new int[] { 4 }.Contains(pplConfig))
                {
                    Position[] dir = { Position.Unknown, Position.Unknown }; // direction detection is not included
                    DataPersistence.PersistResult("test", videoUrl, 0, frameIndex, ItemList, dir, "Cheap", "Heavy", // ArangoDB database
                                                            "test"); // Azure blob
                }


                //display counts
                if (ItemList != null)
                {
                    Dictionary<string, string> kvpairs = new Dictionary<string, string>();
                    foreach (Item it in ItemList)
                    {
                        if (!kvpairs.ContainsKey(it.TriggerLine))
                            kvpairs.Add(it.TriggerLine, "1");
                    }
                    FramePreProcessor.FrameDisplay.updateKVPairs(kvpairs);
                }


                //print out stats
                double fps = 1000 * (double)(1) / (DateTime.Now - prevTime).TotalMilliseconds;
                double avgFps = 1000 * (long)frameIndex / (DateTime.Now - startTime).TotalMilliseconds;
                Console.WriteLine("{0} {1,-5} {2} {3,-5} {4} {5,-15} {6} {7,-10:N2} {8} {9,-10:N2}", 
                                    "sFactor:", SAMPLING_FACTOR, "rFactor:", RESOLUTION_FACTOR, "FrameID:", frameIndex, "FPS:", fps, "avgFPS:", avgFps);
                prevTime = DateTime.Now;
            }
            Console.WriteLine("Done!");
        }
    }
}
