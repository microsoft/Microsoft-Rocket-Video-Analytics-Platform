// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;

using OpenCvSharp;

namespace FramePreProcessor
{
    public class PreProcessor
    {
        public static Mat returnFrame(Mat sourceMat, int frameIndex, int SAMPLING_FACTOR, double RESOLUTION_FACTOR, bool display)
        {
            Mat resizedFrame = null;

            if (frameIndex % SAMPLING_FACTOR != 0) return resizedFrame;

            try
            {
                resizedFrame = sourceMat.Resize(new OpenCvSharp.Size((int)(sourceMat.Size().Width * RESOLUTION_FACTOR), (int)(sourceMat.Size().Height * RESOLUTION_FACTOR)));
                if (display)
                    FrameDisplay.display(resizedFrame);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("********RESET RESIZE*****");
                return null;
            }
            return resizedFrame;
        }
    }
}
