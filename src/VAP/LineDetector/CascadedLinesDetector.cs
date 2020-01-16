using System;
using System.Collections.Generic;
using System.Drawing;

using BGSObjectDetector;

namespace LineDetector
{
    class CascadedLinesDetector : ILineBasedDetector
    {
        List<ISingleLineCrossingDetector> lineCrossingDetectors;
        List<int> minLags;
        List<int> maxLags;
        int noLines;
        List<List<int>> CrossingEventTimeStampBuffers;
        int Count;
        Box Bbox;
        int SUPRESSION_INTERVAL = 1;
        List<int> lastEventFrame;
        bool debug = false;

        public void setDebug()
        {
            debug = true;
            foreach (ISingleLineCrossingDetector d in lineCrossingDetectors)
            {
                d.setDebug();
            }
        }

        public List<List<double>> getOccupancyHistory()
        {
            List<List<double>> ret = new List<List<double>>();
            if (debug)
            {
                foreach (ISingleLineCrossingDetector lineDetector in lineCrossingDetectors)
                {
                    ret.Add(lineDetector.getLineOccupancyHistory());
                }
                return ret;
            }
            else
            {
                return null;
            }
        }

        public CascadedLinesDetector(List<ISingleLineCrossingDetector> l_lineDetectors, List<int> l_minLags, List<int> l_maxLags)
        {
            lineCrossingDetectors = l_lineDetectors;
            noLines = lineCrossingDetectors.Count;
            Count = 0;
            minLags = l_minLags;
            maxLags = l_maxLags;

            CrossingEventTimeStampBuffers = new List<List<int>>();

            //the last line does not have a buffer at all!
            for (int i = 0; i < noLines - 1; i++)
            {
                List<int> buffer = new List<int>();
                CrossingEventTimeStampBuffers.Add(buffer);
            }

            lastEventFrame = new List<int>();
            for (int i = 0; i < noLines - 1; i++)
            {
                lastEventFrame.Add(0);
            }
        }


        private void purgeOldEvents(int currentFrame, int lineNo)
        {
            while (CrossingEventTimeStampBuffers[lineNo].Count > 0)
            {
                if (CrossingEventTimeStampBuffers[lineNo][0] <= currentFrame - maxLags[lineNo])
                {
                    CrossingEventTimeStampBuffers[lineNo].RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
        }


        bool recursiveCrossingEventCheck(int lineNo, int frameNo)
        {
            bool result = false;
            purgeOldEvents(frameNo, lineNo - 1);
            if (CrossingEventTimeStampBuffers[lineNo - 1].Count > 0)
            {
                if (frameNo - CrossingEventTimeStampBuffers[lineNo - 1][0] > minLags[lineNo - 1])
                {
                    if (frameNo - lastEventFrame[lineNo - 1] >= SUPRESSION_INTERVAL)
                    {
                        if (lineNo - 1 == 0) //reached the source line - base case
                        {
                            result = true;
                        }
                        else
                        {
                            result = recursiveCrossingEventCheck(lineNo - 1, frameNo);
                        }
                    }
                }
            }
            if (result)
            {
                CrossingEventTimeStampBuffers[lineNo - 1].RemoveAt(0);
                lastEventFrame[lineNo - 1] = frameNo;
            }
            return result;
        }

        void NotifyCrossingEvent(int frameNo, int lineNo)
        {
            if (lineNo != noLines - 1)
            {
                purgeOldEvents(frameNo, lineNo);
                CrossingEventTimeStampBuffers[lineNo].Add(frameNo);
            }
            else //this is the exit line
            {
                if (noLines == 1)
                {
                    Count++;
                }
                else
                {
                    if (recursiveCrossingEventCheck(lineNo, frameNo))
                    {
                        Count++;
                    }
                }
            }
        }

        public void notifyFrameArrival(int frameNo, List<Box> boxes, Bitmap mask)
        {
            for (int i = 0; i < noLines; i++)
            {
                (bool val, Box b) = lineCrossingDetectors[i].notifyFrameArrival(frameNo, boxes, mask);
                if (b != null) Bbox = b;
                if (val)
                {
                    NotifyCrossingEvent(frameNo, i);
                }
            }
        }

        public void notifyFrameArrival(int frameNo, Bitmap mask)
        {
            for (int i = 0; i < noLines; i++)
            {
                bool val = lineCrossingDetectors[i].notifyFrameArrival(frameNo, mask);
                if (val)
                {
                    NotifyCrossingEvent(frameNo, i);
                }
            }
        }

        public int getCount()
        {
            return Count;
        }

        public Box getBbox()
        {
            return Bbox;
        }

        public void setCount(int value)
        {
            Count = value;
        }

        int getPendingNow(int frameNo, int lineNo)
        {
            purgeOldEvents(frameNo, lineNo);
            return CrossingEventTimeStampBuffers[lineNo].Count;
        }

        public Dictionary<string, Object> getParameters()
        {
            Dictionary<string, Object> ret = new Dictionary<string, object>();
            ret.Add("LINES", lineCrossingDetectors);
            ret.Add("MIN_LAGS", minLags);
            ret.Add("MAX_LAGS", maxLags);
            return ret;
        }

        public bool getOccupancy()
        {
            return lineCrossingDetectors[0].getOccupancy();
        }

        public List<(Point p1, Point p2)> getLineCoor()
        {
            List<(Point p1, Point p2)> coors = new List<(Point p1, Point p2)>();
            for (int i = 0; i < lineCrossingDetectors.Count; i++)
            {
                (Point p1, Point p2) coor = (lineCrossingDetectors[i].getDetectionLine().p1, lineCrossingDetectors[i].getDetectionLine().p2);
                coors.Add(coor);
            }
            return coors;
        }

        public DetectionLine getDetectionLine()
        {
            return lineCrossingDetectors[0].getDetectionLine();
        }
    }
}
