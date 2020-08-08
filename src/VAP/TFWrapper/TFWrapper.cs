// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TensorFlow;
using Mono.Options;
using System.Reflection;
using System.Net;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using Wrapper.TF.Common;
using System.Drawing;
using System.Drawing.Imaging;
using Utils.Config;

namespace Wrapper.TF
{
    public class TFWrapper
    {
        public static IEnumerable<CatalogItem> _catalog;
        private static string _currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string _input_relative = "test_images/input.jpg";
        private static string _output_relative = "test_images/output.jpg";
        private static string _input = Path.Combine(_currentDir, _input_relative);
        private static string _output = Path.Combine(_currentDir, _output_relative);
        private static string _catalogPath;
        private static string _modelPath;

        //private static double MIN_SCORE_FOR_OBJECT_HIGHLIGHTING = 0.5;

        static OptionSet options = new OptionSet()
        {
            { "input_image=",  "Specifies the path to an image ", v => _input = v },
            { "output_image=",  "Specifies the path to the output image with detected objects", v => _output = v },
            { "catalog=", "Specifies the path to the .pbtxt objects catalog", v=> _catalogPath = v},
            { "model=", "Specifies the path to the trained model", v=> _modelPath = v},
            { "h|help", v => Help () }
        };

        private static TFGraph graph;
        private static TFSession session;

        public TFWrapper()
        {
            //options.Parse(args);

            if (_catalogPath == null)
            {
                _catalogPath = DownloadDefaultTexts(_currentDir);
            }

            if (_modelPath == null)
            {
                _modelPath = DownloadDefaultModel(_currentDir);
            }

            _catalog = CatalogUtil.ReadCatalogItems(_catalogPath);
            var fileTuples = new List<(string input, string output)>() { (_input, _output) };

            graph = new TFGraph();
            var model = File.ReadAllBytes(_modelPath);
            graph.Import(new TFBuffer(model));
            session = new TFSession(graph);

            var runner = session.GetRunner();

            runner
                .AddInput(graph["image_tensor"][0], ImageUtil.CreateTensorFromImageFile(fileTuples[0].input, TFDataType.UInt8))
                .Fetch(
                graph["detection_boxes"][0],
                graph["detection_scores"][0],
                graph["detection_classes"][0],
                graph["num_detections"][0]);

            TFTensor[] output = null;
            output = runner.Run();
        }

        public (float[,,], float[,], float[,]) Run(byte[] imageByteArray)
        {
            var tensor = ImageUtil.CreateTensorFromByteArray(imageByteArray, TFDataType.UInt8);
            var runner = session.GetRunner();

            runner
                .AddInput(graph["image_tensor"][0], tensor)
                .Fetch(
                graph["detection_boxes"][0],
                graph["detection_scores"][0],
                graph["detection_classes"][0],
                graph["num_detections"][0]);

            TFTensor[] output = null;
            output = runner.Run();

            var boxes = (float[,,])output[0].GetValue(jagged: false);
            var scores = (float[,])output[1].GetValue(jagged: false);
            var classes = (float[,])output[2].GetValue(jagged: false);
            var num = (float[])output[3].GetValue(jagged: false);

            //// output ALL annotated image
            //using (Image image = Image.FromStream(new MemoryStream(imageByteArray)))
            //{
            //    image.Save(@OutputFolder.OutputFolderFrameDNNTF+"input.jpg", ImageFormat.Jpeg);
            //}
            //DrawBoxes(boxes, scores, classes, @OutputFolder.OutputFolderFrameDNNTF + "input.jpg", @OutputFolder.OutputFolderFrameDNNTF + "output.jpg", MIN_SCORE_FOR_OBJECT_HIGHLIGHTING);
            //Console.WriteLine($"Done. See {_output_relative}");

            return (boxes, scores, classes);
        }

        private static string DownloadDefaultModel(string dir)
        {
            string defaultModelUrl = "http://download.tensorflow.org/models/object_detection/faster_rcnn_resnet101_coco_2018_01_28.tar.gz";
            var modelFile = Path.Combine(dir, "faster_rcnn_resnet101_coco_2018_01_28/frozen_inference_graph.pb");
            var zipfile = Path.Combine(dir, "faster_rcnn_resnet101_coco_2018_01_28.tar.gz");

            //string defaultModelUrl = "http://download.tensorflow.org/models/object_detection/ssd_mobilenet_v1_ppn_shared_box_predictor_300x300_coco14_sync_2018_07_03.tar.gz";
            //var modelFile = Path.Combine(dir, "ssd_mobilenet_v1_ppn_shared_box_predictor_300x300_coco14_sync_2018_07_03/frozen_inference_graph.pb");
            //var zipfile = Path.Combine(dir, "ssd_mobilenet_v1_ppn_shared_box_predictor_300x300_coco14_sync_2018_07_03.tar.gz");

            if (File.Exists(modelFile))
                return modelFile;

            if (!File.Exists(zipfile))
            {
                Console.WriteLine("Downloading default model");
                var wc = new WebClient();
                wc.DownloadFile(defaultModelUrl, zipfile);
            }

            ExtractToDirectory(zipfile, dir);
            Console.WriteLine($"dir = {dir}");
            File.Delete(zipfile);

            return modelFile;
        }

        private static void ExtractToDirectory(string file, string targetDir)
        {
            Console.WriteLine("Extracting");

            using (Stream inStream = File.OpenRead(file))
            using (Stream gzipStream = new GZipInputStream(inStream))
            {
                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents(targetDir);
            }
        }

        private static string DownloadDefaultTexts(string dir)
        {
            Console.WriteLine("Downloading default label map");

            string defaultTextsUrl = "https://raw.githubusercontent.com/tensorflow/models/master/research/object_detection/data/mscoco_label_map.pbtxt";
            var textsFile = Path.Combine(dir, "mscoco_label_map.pbtxt");
            var wc = new WebClient();
            wc.DownloadFile(defaultTextsUrl, textsFile);

            return textsFile;
        }

        private static void DrawBoxes(float[,,] boxes, float[,] scores, float[,] classes, string inputFile, string outputFile, double minScore)
        {
            var x = boxes.GetLength(0);
            var y = boxes.GetLength(1);
            var z = boxes.GetLength(2);

            float ymin = 0, xmin = 0, ymax = 0, xmax = 0;

            using (var editor = new ImageEditor(inputFile, outputFile))
            {
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (scores[i, j] < minScore) continue;

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

                        int value = Convert.ToInt32(classes[i, j]);
                        CatalogItem catalogItem = _catalog.FirstOrDefault(item => item.Id == value);
                        editor.AddBox(xmin, xmax, ymin, ymax, $"{catalogItem.DisplayName} : {(scores[i, j] * 100).ToString("0")}%");
                    }
                }
            }
        }

        private static void Help()
        {
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}
