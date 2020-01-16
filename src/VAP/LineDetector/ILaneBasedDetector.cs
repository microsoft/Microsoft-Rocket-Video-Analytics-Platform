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

        void setDebug();

        List<List<double>> getOccupancyHistory();

        DetectionLine getDetectionLine();

        bool getOccupancy();

        int getCount();

        void setCount(int value);

        Box getBbox();

        Dictionary<string, Object> getParameters();

        List<(Point p1, Point p2)> getLineCoor();
    }
}
