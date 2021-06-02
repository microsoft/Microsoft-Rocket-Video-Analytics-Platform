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

        /// <summary>
        /// The initial delay to allow for the background subtractor to kick in, N_FRAMES_TO_LEARN in MOG2.cs
        /// </summary>
        public int START_DELAY = 120;


        public MultiLaneDetector multiLaneDetector;

        Dictionary<string, int> counts = new Dictionary<string, int>();
        Dictionary<string, int> counts_prev = new Dictionary<string, int>();
        Dictionary<string, bool> occupancy = new Dictionary<string, bool>();
        Dictionary<string, bool> occupancy_prev = new Dictionary<string, bool>();

        /// <summary>
        /// Constructs a <see cref="Detector"/> object with the provided 
        /// </summary>
        /// <param name="sFactor">Sampling rate scaling factor.</param>
        /// <param name="rFactor">Resolution scaling factor.</param>
        /// <param name="linesFile">A file specifying the lines used for the line-crossing algorithm.</param>
        /// <param name="displayBGS">True to display a separate image each frame with the current frame number, the lines used, and the changes from the previous frame.</param>
        public Detector(int sFactor, double rFactor, string linesFile, bool displayBGS)
        {
            Dictionary<string, ILineBasedDetector> lineBasedDetectors = LineSets.readLineSet_LineDetector_FromTxtFile(linesFile, sFactor, rFactor);

            multiLaneDetector = (lineBasedDetectors != null) ? new MultiLaneDetector(lineBasedDetectors) : null;

            this.DISPLAY_BGS = displayBGS;
            Console.WriteLine(linesFile);
        }

        /// <summary>
        /// Checks for items crossing the provided LineSet.
        /// </summary>
        /// <param name="frame">The frame to check.</param>
        /// <param name="frameIndex">The index of the frame given.</param>
        /// <param name="fgmask">The foreground mask of the frame.</param>
        /// <param name="boxes">A list of bounding boxes of items in the frame which deviate from the background.</param>
        /// <returns>
        ///   <para>Returns a tuple with two <see cref="Dictionary{TKey, TValue}">Dictionaries</see>.</para>
        ///   <para>The first dictionary contains the number of items which cross the lines of interest, indexed by line name.</para>
        ///   <para>The second dictionary contains a boolean for each line indicating whether or not an item is present at that line.</para>
        /// </returns>
        public (Dictionary<string, int>, Dictionary<string, bool>) updateLineResults(Mat frame, int frameIndex, Mat fgmask, List<Box> boxes)
        {
            if (frameIndex > START_DELAY)
            {
                Bitmap fgmaskBit = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(fgmask);

                multiLaneDetector.notifyFrameArrival(frameIndex, boxes, fgmaskBit);

                // bgs visualization with lines
                if (DISPLAY_BGS)
                {
                    List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines = this.multiLaneDetector.getAllLines();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        System.Drawing.Point p1 = lines[i].coordinates.p1;
                        System.Drawing.Point p2 = lines[i].coordinates.p2;
                        Cv2.Line(fgmask, p1.X, p1.Y, p2.X, p2.Y, new Scalar(255, 0, 255, 255), 5);
                    }
                    Cv2.ImShow("BGS Output", fgmask);
                    //Cv2.WaitKey(1);
                }
            }
            counts = multiLaneDetector.getCounts();

            if (counts_prev.Count != 0)
            {
                foreach (string lane in counts.Keys)
                {
                    int diff = Math.Abs(counts[lane] - counts_prev[lane]);
                    if (diff > 0) //object detected by BGS-based counter
                    {
                        Console.WriteLine($"Line: {lane}\tCounts: {counts[lane]}");
                        string blobName_BGS = $@"frame-{frameIndex}-BGS-{lane}-{counts[lane]}.jpg";
                        string fileName_BGS = @OutputFolder.OutputFolderBGSLine + blobName_BGS;
                        frame.SaveImage(fileName_BGS);
                        frame.SaveImage(@OutputFolder.OutputFolderAll + blobName_BGS);
                    }
                }
            }
            updateCount(counts);

            //occupancy
            occupancy = multiLaneDetector.getOccupancy();
            foreach (string lane in occupancy.Keys)
            {
                //output frames that have line occupied by objects
                //if (frameIndex > 1)
                //{
                //    if (occupancy[lane])
                //    {
                //        string blobName_BGS = $@"frame-{frameIndex}-BGS-{lane}-{occupancy[lane]}.jpg";
                //        string fileName_BGS = @OutputFolder.OutputFolderBGSLine + blobName_BGS;
                //        frame.SaveImage(fileName_BGS);
                //        frame.SaveImage(@OutputFolder.OutputFolderAll + blobName_BGS);
                //    }
                //}
                updateCount(lane, occupancy);
            }

            return (counts, occupancy);
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

        void updateCount(Dictionary<string, int> counts)
        {
            foreach (string dir in counts.Keys)
            {
                counts_prev[dir] = counts[dir];
            }
        }
    }
}
