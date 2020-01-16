using System;
using System.Collections.Generic;

namespace LineDetector
{
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

        public void setDebug()
        {
            debug = true;
            debug_occupancySequence = new List<double>();
        }

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

        public OCCUPANCY_STATE getState()
        {
            return curState;
        }

        public List<double> getLineOccupancyHistory()
        {
            return debug_occupancySequence;
        }
    }
}
