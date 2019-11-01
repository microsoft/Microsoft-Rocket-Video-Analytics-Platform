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

        public int[] getLineCoor()
        {
            int[] coor = new int[4];
            coor[0] = getDetectionLine().x1;
            coor[1] = getDetectionLine().y1;
            coor[2] = getDetectionLine().x2;
            coor[3] = getDetectionLine().y2;
            return coor;
        }
    }
}
