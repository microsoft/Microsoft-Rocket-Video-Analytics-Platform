// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using BGSObjectDetector;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace LineDetector
{
    // corresponds to LineCrossingBasedMultiLaneCounter.cs
    public class MultiLaneDetector
    {
        Dictionary<string, ILineBasedDetector> laneDetector;

        public MultiLaneDetector(Dictionary<string, ILineBasedDetector> lineBasedDetector)
        {
            laneDetector = lineBasedDetector;
        }

        public void notifyFrameArrival(int frameNo, List<Box> boxes, Bitmap mask)
        {

            foreach (KeyValuePair<string, ILineBasedDetector> entry in laneDetector)
            {
                entry.Value.notifyFrameArrival(frameNo, boxes, mask);
            }
        }

        public void notifyFrameArrival(int frameNo, Bitmap mask)
        {

            foreach (KeyValuePair<string, ILineBasedDetector> entry in laneDetector)
            {
                entry.Value.notifyFrameArrival(frameNo, mask);
            }
        }

        public Dictionary<string, bool> getOccupancy()
        {
            Dictionary<string, bool> occupancy = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, ILineBasedDetector> entry in laneDetector)
            {
                occupancy.Add(entry.Key, entry.Value.getOccupancy());
            }
            return occupancy;
        }

        public float[] getBboxCenter(string laneID)
        {
            foreach (KeyValuePair<string, ILineBasedDetector> entry in laneDetector)
            {
                if (entry.Key == laneID)
                {
                    return entry.Value.getBbox().Center;
                }
            }
            return null;
        }

        public List<(string key, (int x1, int y1, int x2, int y2) coordinates)> getAllLines()
        {
            List<(string key, (int x1, int y1, int x2, int y2) coordinates)> lines = new List<(string key, (int x1, int y1, int x2, int y2) coordinates)>();
            foreach (KeyValuePair<string, ILineBasedDetector> lane in laneDetector)
            {
                (int x1, int y1, int x2, int y2) coor = lane.Value.getLineCoor();
                lines.Add((lane.Key, coor));
            }
            return lines;
        }
    }
}
