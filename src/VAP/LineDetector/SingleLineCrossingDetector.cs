// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using BGSObjectDetector;
using System.Collections.Generic;
using System.Drawing;

namespace LineDetector
{
    class SingleLineCrossingDetector : ILineBasedDetector
    {
        DetectionLine line;
        bool occupancy;
        Box bbox;

        public SingleLineCrossingDetector(int a, int b, int c, int d)
        {
            line = new DetectionLine(a, b, c, d);
        }

        public SingleLineCrossingDetector(int a, int b, int c, int d, double threshold, int sFactor)
        {
            line = new DetectionLine(a, b, c, d, threshold);
        }

        public void notifyFrameArrival(int frameNo, List<Box> boxes, Bitmap mask)
        {
            (occupancy, bbox) = line.isOccupied(boxes, mask);
        }

        public void notifyFrameArrival(int frameNo, Bitmap mask)
        {
            occupancy = line.isOccupied(mask);
        }

        public bool getOccupancy()
        {
            return occupancy;
        }

        public DetectionLine getDetectionLine()
        {
            return line;
        }

        public Box getBbox()
        {
            return bbox;
        }

        public (Point p1, Point p2) getLineCoor()
        {
            return (getDetectionLine().p1, getDetectionLine().p2);
        }
    }
}
