using System.Collections.Generic;
using System.Drawing;

using BGSObjectDetector;

namespace LineDetector
{
    interface ISingleLineCrossingDetector
    {
        //notify the arrival of a frame to the detector
        //return true if there is a crossing event detected
        (bool crossingResult, Box b) notifyFrameArrival(int frameNo, List<Box> boxes, Bitmap mask);
        bool notifyFrameArrival(int frameNo, Bitmap mask);

        //returns occupied or not
        OCCUPANCY_STATE getState();

        void setDebug();

        List<double> getLineOccupancyHistory();

        DetectionLine getDetectionLine();

        bool getOccupancy();
    }
}
