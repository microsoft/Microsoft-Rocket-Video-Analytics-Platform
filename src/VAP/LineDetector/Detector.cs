// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenCvSharp;
using Utils.Config;
using BGSObjectDetector;

namespace LineDetector
{
    public class Detector
    {
        bool DISPLAY_BGS;

        public int START_DELAY = 120; //this is an initial delay we put in for the background subtractor to kick in, N_FRAMES_TO_LEARN in MOG2.cs

        public MultiLaneDetector multiLaneDetector;

        Dictionary<string, bool> occupancy = new Dictionary<string, bool>();
        Dictionary<string, bool> occupancy_prev = new Dictionary<string, bool>();

        public Detector(int sFactor, double rFactor, string linesFile, bool displayBGS)
        {
            Dictionary<string, ILineBasedDetector> lineBasedDetectors = LineSets.readLineSet_LineDetector_FromTxtFile(linesFile, sFactor, rFactor);

            multiLaneDetector = (lineBasedDetectors != null) ? new MultiLaneDetector(lineBasedDetectors) : null;

            this.DISPLAY_BGS = displayBGS;
            Console.WriteLine(linesFile);
        }

        public Dictionary<string, bool> updateLineOccupancy(Mat frame, int frameIndex, Mat fgmask, List<Box> boxes)
        {
            //frameIndex++;
            //if (frameIndex > START_DELAY && frameIndex % SUB_SAMPLING_FACTOR != 0) return frameIndex;

            if (frameIndex > START_DELAY)
            {
                Bitmap fgmaskBit = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(fgmask);

                multiLaneDetector.notifyFrameArrival(frameIndex, boxes, fgmaskBit);

                // bgs visualization with lines
                if (DISPLAY_BGS)
                {
                    List<Tuple<string, int[]>> lines = this.multiLaneDetector.getAllLines();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        System.Drawing.Point p1 = new System.Drawing.Point(lines[i].Item2[0], lines[i].Item2[1]);
                        System.Drawing.Point p2 = new System.Drawing.Point(lines[i].Item2[2], lines[i].Item2[3]);
                        Cv2.Line(fgmask, p1.X, p1.Y, p2.X, p2.Y, new OpenCvSharp.Scalar(255, 0, 255, 255), 5);
                    }
                    Cv2.ImShow("BGS Output", fgmask);
                    Cv2.WaitKey(1);
                }
            }

            occupancy = multiLaneDetector.getOccupancy();
            foreach (string lane in occupancy.Keys)
            {
                if (frameIndex > 1)
                {
                    if (occupancy[lane])
                    {
                        string blobName_BGS = $@"frame-{frameIndex}-BGS-{lane}-{occupancy[lane]}.jpg";
                        string fileName_BGS = @OutputFolder.OutputFolderBGSLine + blobName_BGS;
                        frame.SaveImage(fileName_BGS);
                        frame.SaveImage(@OutputFolder.OutputFolderAll + blobName_BGS);
                    }
                }
                updateCount(lane, occupancy);
            }

            return occupancy;
        }

        bool occupancyChanged(string lane)
        {
            bool diff = false;
            if (occupancy_prev.Count != 0)
            {
                diff = occupancy[lane] != occupancy_prev[lane];
            }

            return diff;
        }

        void updateCount(string lane, Dictionary<string, bool> counts)
        {
            occupancy_prev[lane] = counts[lane];
        }
    }
}
