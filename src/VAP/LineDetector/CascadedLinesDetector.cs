using System;
using System.Collections.Generic;
using System.Drawing;

using BGSObjectDetector;

namespace LineDetector
{
    /// <summary>
    /// Detector that uses a series of lines to detect the approach of items and fires when the items cross the final line.
    /// </summary>
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

        /// <summary>
        /// Activates debug logging.
        /// </summary>
        public void setDebug()
        {
            debug = true;
            foreach (ISingleLineCrossingDetector d in lineCrossingDetectors)
            {
                d.setDebug();
            }
        }

        /// <summary>
        /// Provides a history of the occupancy of this line detector, with each entry containing a list of occupancy values for each line considered by this detector.
        /// </summary>
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

        /// <summary>
        /// Creates a <see cref="CascadedLinesDetector"/> using the provided detectors, 
        /// </summary>
        /// <param name="l_lineDetectors">A list of <see cref="ISingleLineCrossingDetector"/> objects to use in this detector.</param>
        /// <param name="l_minLags">The minimum number of events to store in the internal buffer.</param>
        /// <param name="l_maxLags">The maximum number of events to store in the internal buffer.</param>
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

        /// <summary>
        /// Processes a frame upon arrival.
        /// </summary>
        /// <param name="frameNo">The index of the frame to process.</param>
        /// <param name="boxes">A list of bounding boxes of items in frame.</param>
        /// <param name="mask">A mask detailing the precise layout of items in the frame using black to indicate vacant space, and white to indicate occupied space.</param>
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

        /// <summary>
        /// Processes a frame upon arrival.
        /// </summary>
        /// <param name="frameNo">The index of the frame to process.</param>
        /// <param name="mask">A mask detailing the precise layout of items in the frame using black to indicate vacant space, and white to indicate occupied space.</param>
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

        /// <summary>
        /// Gets the number of times that this detector has been triggered.
        /// </summary>
        /// <returns></returns>
        public int getCount()
        {
            return Count;
        }

        /// <summary>
        /// Gets the bounding box of the line used by this detector.
        /// </summary>
        /// <returns></returns>
        public Box getBbox()
        {
            return Bbox;
        }

        /// <summary>
        /// Sets the count of this detector.
        /// </summary>
        /// <param name="value"></param>
        public void setCount(int value)
        {
            Count = value;
        }

        int getPendingNow(int frameNo, int lineNo)
        {
            purgeOldEvents(frameNo, lineNo);
            return CrossingEventTimeStampBuffers[lineNo].Count;
        }

        /// <summary>
        /// Gets a <c>Dictionary</c> of the parameters used by this detector, stored by name.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Object> getParameters()
        {
            Dictionary<string, Object> ret = new Dictionary<string, object>();
            ret.Add("LINES", lineCrossingDetectors);
            ret.Add("MIN_LAGS", minLags);
            ret.Add("MAX_LAGS", maxLags);
            return ret;
        }

        /// <summary>
        /// Gets the current occupancy state of this detector. This updates when the detector is notified of a frame arrival.
        /// </summary>
        /// <returns>Returns true if the line is occupied, and false otherwise.</returns>
        public bool getOccupancy()
        {
            return lineCrossingDetectors[0].getOccupancy();
        }

        /// <summary>
        /// Gets the line segments used by this detector.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the <see cref="DetectionLine"/> used by this detector.
        /// </summary>
        public DetectionLine getDetectionLine()
        {
            return lineCrossingDetectors[0].getDetectionLine();
        }
    }
}
