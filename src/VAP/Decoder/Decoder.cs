// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using OpenCvSharp;

namespace Decoder
{
    public class Decoder
    {
        VideoCapture capture = null;
        string inputURL;

        bool toLoop;

        int objTotal, objDirA, objDirB;

        public Decoder(string input, bool loop)
        {
            capture = new VideoCapture(input);
            inputURL = input;

            toLoop = loop;
        }

        public Mat getNextFrame()
        {
            Mat sourceMat = new Mat();

            try
            {
                capture.Read(sourceMat);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("********RESET*****");

                capture = new VideoCapture(inputURL);

                return null;
            }

            if (sourceMat == null)
                return sourceMat;

            if (toLoop)
            {
                if (sourceMat.Height == 0 && sourceMat.Width == 0)
                {
                    capture = new VideoCapture(inputURL);
                    capture.Read(sourceMat);
                }
            }
 
            return sourceMat;
        }

        public int getTotalFrameNum()
        {
            int length;
            length = (int)Math.Floor(capture.Get(CaptureProperty.FrameCount));

            return length;
        }

        public double getVideoFPS()
        {
            double framerate;
            framerate = capture.Get(CaptureProperty.Fps);

            return framerate;
        }

        public void updateObjNum(int[] dirCount)
        {
            objTotal = dirCount[0] + dirCount[1] + dirCount[2];
            objDirA = dirCount[0];
            objDirB = dirCount[1];
        }
    }
}
