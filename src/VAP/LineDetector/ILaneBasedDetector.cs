// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Drawing;

using BGSObjectDetector;

namespace LineDetector
{
    public interface ILineBasedDetector
    {
        void notifyFrameArrival(int frameNo, List<Box> boxes, Bitmap mask);

        void notifyFrameArrival(int frameNo, Bitmap mask);

        bool getOccupancy();

        Box getBbox();

        (int x1, int y1, int x2, int y2) getLineCoor();

        DetectionLine getDetectionLine();
    }
}
