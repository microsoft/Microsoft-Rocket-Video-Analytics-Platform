// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using OpenCvSharp;

namespace Decoder
{
    public class Decoder
    {
        VideoCapture capture;
        string[] inputURLs;
        int inputIndex;

        bool toLoop;

        int currentFrame;
        int totalFrames;
        bool skipLastFrame;

        Mat resizedFrame;

        int objTotal, objDirA, objDirB;

        public Decoder(string[] inputs, bool loop, bool skipLastFrame)
        {
            inputIndex = -1;
            inputURLs = inputs;
            toLoop = loop;
            this.skipLastFrame = skipLastFrame;

            MoveToNextInput();
        }

        private bool MoveToNextInput()
        {
            if (!toLoop && inputIndex == inputURLs.Length - 1)
            {
                return false;
            }

            inputIndex = (inputIndex + 1) % inputURLs.Length;
            capture = new VideoCapture(inputURLs[inputIndex]);
            currentFrame = -1;
            totalFrames = (int)Math.Floor(capture.Get(CaptureProperty.FrameCount));
            return true;
        }

        public Mat getNextFrame()
        {
            Mat sourceMat = new Mat();

            currentFrame++;
            if (skipLastFrame && currentFrame == totalFrames - 1)
            {
                if (!MoveToNextInput())
                {
                    return null;
                }
            }

            try
            {
                capture.Read(sourceMat);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("********RESET*****");

                capture = new VideoCapture(inputURLs[inputIndex]);
                currentFrame = -1;

                return null;
            }

            if (sourceMat == null)
                return sourceMat;

            if (sourceMat.Height == 0 && sourceMat.Width == 0)
            {
                if (MoveToNextInput())
                {
                    capture.Read(sourceMat);
                }
            }
 
            return sourceMat;
        }

        public int? getTotalFrameNum()
        {
            if (toLoop || inputIndex < inputURLs.Length - 1)
            {
                // the total frame count is not known
                return null;
            }

            return totalFrames;
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
