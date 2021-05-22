using System;
using System.Collections.Generic;

namespace LineDetector
{
    /// <summary>
    /// A line crossing detector that fires when an item leaves the line area.
    /// </summary>
    class FallingEdgeCrossingDetector : ICrossingDetector
    {
        List<int> FrameNoList = new List<int>();
        List<double> OccupancyValueList = new List<double>();
        int UP_STATE_TRANSITION_LENGTH = 4;
        int DOWN_STATE_TRANSITION_LENGTH = 10;
        int History;
        OCCUPANCY_STATE curState = OCCUPANCY_STATE.UNOCCUPIED;
        bool debug = false;
        public List<double> debug_occupancySequence;

        /// <summary>
        /// Enables debug mode. Within <see cref="FallingEdgeCrossingDetector"/>, this enables logging occupancy values.
        /// </summary>
        /// <remarks>
        /// The occupancy log may be accessed with <see cref="getLineOccupancyHistory"/>.</remarks>
        public void setDebug()
        {
            debug = true;
            debug_occupancySequence = new List<double>();
        }

        /// <summary>
        /// Creates a <see cref="FallingEdgeCrossingDetector"/> with the provided frame rate sampling factor.
        /// </summary>
        /// <param name="sFactor">The frame rate sampling factor. Note that particularly large values could behave unexpectedly.</param>
        public FallingEdgeCrossingDetector(int sFactor)
        {
            UP_STATE_TRANSITION_LENGTH = (int)Math.Ceiling((double)UP_STATE_TRANSITION_LENGTH / sFactor);
            DOWN_STATE_TRANSITION_LENGTH = (int)Math.Ceiling((double)DOWN_STATE_TRANSITION_LENGTH / sFactor);
            History = Math.Max(UP_STATE_TRANSITION_LENGTH, DOWN_STATE_TRANSITION_LENGTH);
        }

        private bool CheckForStateTransision()
        {
            if (OccupancyValueList.Count < History)
            {
                return false;
            }
            if (curState == OCCUPANCY_STATE.UNOCCUPIED)
            {
                for (int i = 0; i < UP_STATE_TRANSITION_LENGTH; i++)
                {
                    if (OccupancyValueList[OccupancyValueList.Count - i - 1] < 0)
                    {
                        return false;
                    }
                }
                curState = OCCUPANCY_STATE.OCCUPIED;
                return false;
            }
            else
            {
                for (int i = 0; i < DOWN_STATE_TRANSITION_LENGTH; i++)
                {
                    if (OccupancyValueList[OccupancyValueList.Count - i - 1] > 0)
                    {
                        return false;
                    }
                }
                curState = OCCUPANCY_STATE.UNOCCUPIED;
                return true;
            }
        }


        //return true if there was a line crossing event
        /// <summary>
        /// Notifies the detector of a new occupancy state at a given frame.
        /// </summary>
        /// <param name="frameNo">The index of the frame of interest.</param>
        /// <param name="occupancy">The occupancy state at that frame.</param>
        /// <returns>Returns true if an event was detected, and false otherwise.</returns>
        public bool notifyOccupancy(int frameNo, bool occupancy)
        {
            while (FrameNoList.Count > 0)
            {
                if (FrameNoList[0] <= frameNo - History)
                {
                    FrameNoList.RemoveAt(0);
                    OccupancyValueList.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            double finalOccupancyValue = -1;
            if (occupancy)
            {
                finalOccupancyValue = 1;
            }

            //interpolate for missing frames
            if (FrameNoList.Count > 0)
            {
                int curFrameNo = FrameNoList[FrameNoList.Count - 1];
                int nextFrameNo = frameNo;
                int diff = nextFrameNo - curFrameNo;
                if (diff > 1)
                {
                    double initialOccupancyValue = OccupancyValueList[OccupancyValueList.Count - 1];
                    double occupancyDiff = finalOccupancyValue - initialOccupancyValue;
                    double ratio = occupancyDiff / (double)diff;
                    for (int f = curFrameNo + 1; f < nextFrameNo; f++)
                    {
                        double value = (f - curFrameNo) * ratio + initialOccupancyValue;
                        FrameNoList.Add(f);
                        OccupancyValueList.Add(value);
                        if (debug)
                        {
                            debug_occupancySequence.Add(value);
                        }
                    }
                }
            }
            FrameNoList.Add(frameNo);
            OccupancyValueList.Add(finalOccupancyValue);
            if (debug)
            {
                debug_occupancySequence.Add(finalOccupancyValue);
            }

            //Console.WriteLine("finalOccupancyValue:" + finalOccupancyValue);
            return CheckForStateTransision();
        }

        /// <summary>
        /// Gets the occupancy state of the detector as of the latest frame.
        /// </summary>
        /// <returns></returns>
        public OCCUPANCY_STATE getState()
        {
            return curState;
        }

        /// <summary>
        /// Gets a list of all occupancy values observed by the detector while debugging has been enabled. No frame indices are included.
        /// </summary>
        /// <returns></returns>
        public List<double> getLineOccupancyHistory()
        {
            return debug_occupancySequence;
        }
    }
}
