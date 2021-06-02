using System.Collections.Generic;
using System.Drawing;

using BGSObjectDetector;

namespace LineDetector
{
    //TODO(iharwell): Pull methods into a separate interface for things that overlap with ICrossingDetector and ILineBasedDetector.
    /// <summary>
    /// Interface for a line crossing detector that uses a single line for detection.
    /// </summary>
    interface ISingleLineCrossingDetector
    {
        //notify the arrival of a frame to the detector
        //return true if there is a crossing event detected
        /// <summary>
        /// Processes a frame upon arrival.
        /// </summary>
        /// <param name="frameNo">The index of the frame to process.</param>
        /// <param name="boxes">A list of bounding boxes of items in frame.</param>
        /// <param name="mask">A mask detailing the precise layout of items in the frame using black to indicate vacant space, and white to indicate occupied space.</param>
        /// <returns>
        /// Returns a Tuple that contains a boolean indicating whether a crossing was detected, and the bounding box of the crossing item.
        /// </returns>
        (bool crossingResult, Box b) notifyFrameArrival(int frameNo, List<Box> boxes, Bitmap mask);

        /// <summary>
        /// Processes a frame upon arrival.
        /// </summary>
        /// <param name="frameNo">The index of the frame to process.</param>
        /// <param name="mask">A mask detailing the precise layout of items in the frame using black to indicate vacant space, and white to indicate occupied space.</param>
        /// <returns>
        /// Returns a boolean indicating whether a crossing was detected.
        /// </returns>
        bool notifyFrameArrival(int frameNo, Bitmap mask);

        //returns occupied or not
        /// <summary>
        /// Gets the occupancy state of this detector as of the latest frame.
        /// </summary>
        /// <returns></returns>
        OCCUPANCY_STATE getState();

        /// <summary>
        /// Enables debug logging.
        /// </summary>
        void setDebug();

        /// <summary>
        /// Gets the line occupancy overlap values which are stored while in debug mode.
        /// </summary>
        /// <returns></returns>
        List<double> getLineOccupancyHistory();

        /// <summary>
        /// Gets the <see cref="DetectionLine"/> used by this detector.
        /// </summary>
        /// <returns></returns>
        DetectionLine getDetectionLine();

        /// <summary>
        /// Gets the occupancy state of this detector as of the latest frame.
        /// </summary>
        /// <returns>Returns true if the detector is occupied by one or more items, and false otherwise.</returns>
        bool getOccupancy();
    }
}
