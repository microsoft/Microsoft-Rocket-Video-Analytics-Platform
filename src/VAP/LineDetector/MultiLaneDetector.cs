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

        public Dictionary<string, int> getCounts()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (KeyValuePair<string, ILineBasedDetector> entry in laneDetector)
            {
                counts.Add(entry.Key, entry.Value.getCount());
            }
            return counts;
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

        public PointF? getBboxCenter(string laneID)
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

        public List<(string key, (Point p1, Point p2) coordinates)> getAllLines()
        {
            List<(string key, (Point p1, Point p2) coordinates)> lines = new List<(string key, (Point p1, Point p2) coordinates)>();
            foreach (KeyValuePair<string, ILineBasedDetector> lane in laneDetector)
            {
                (Point p1, Point p2) coor = lane.Value.getLineCoor()[0];
                lines.Add((lane.Key, coor));
            }
            return lines;
        }
    }
}
