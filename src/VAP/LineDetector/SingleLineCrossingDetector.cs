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

        public (int x1, int y1, int x2, int y2) getLineCoor()
        {
            return (getDetectionLine().x1, getDetectionLine().y1, getDetectionLine().x2, getDetectionLine().y2);
        }
    }
}
