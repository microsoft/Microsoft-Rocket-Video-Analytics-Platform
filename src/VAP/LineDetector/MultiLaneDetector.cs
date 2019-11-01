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

        public List<Tuple<string, int[]>> getAllLines()
        {
            List<Tuple<string, int[]>> lines = new List<Tuple<string, int[]>>();
            foreach (KeyValuePair<string, ILineBasedDetector> lane in laneDetector)
            {
                int[] coor = lane.Value.getLineCoor();
                Tuple<string, int[]> line = new Tuple<string, int[]>(lane.Key, coor);
                lines.Add(line);
            }
            return lines;
        }
    }
}
