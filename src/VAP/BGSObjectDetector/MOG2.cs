// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using OpenCvSharp;

namespace BGSObjectDetector
{
    class MOG2
    {
        BackgroundSubtractorMOG2 fgDetector = BackgroundSubtractorMOG2.Create(500, 10); //try sweeping (also set it higher than 25)
        Mat regionOfInterest = null;
        Mat fgMask0 = new Mat();
        Mat fgMask = new Mat();

        int N_FRAMES_TO_LEARN = 120; // Why do we need this?

        public MOG2()
        {
            regionOfInterest = null;
            fgMask0 = new Mat();
            fgMask = new Mat();
        }

        public Mat DetectForeground(Mat image, int nFrames)
        {
            fgDetector.Apply(image, fgMask0);

            if (regionOfInterest != null)
                Cv2.BitwiseAnd(fgMask0, regionOfInterest, fgMask);

            if (nFrames < N_FRAMES_TO_LEARN)
                return null;
            else if (regionOfInterest != null)
                return fgMask;
            else
                return fgMask0;
        }

        public void SetRegionOfInterest(Mat roi)
        {
            if (roi != null)
            {
                regionOfInterest = new Mat();
                roi.CopyTo(regionOfInterest);
            }
        }
    }
}
