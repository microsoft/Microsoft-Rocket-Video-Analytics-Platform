// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Drawing;

using BGSObjectDetector;

namespace LineDetector
{
    /// <summary>
    /// A detector that checks for items crossing a line.
    /// </summary>
    public interface ILineBasedDetector
    {
        /// <summary>
        /// Processes a frame upon arrival.
        /// </summary>
        /// <param name="frameNo">The index of the frame to process.</param>
        /// <param name="boxes">A list of bounding boxes of items in frame.</param>
        /// <param name="mask">A mask detailing the precise layout of items in the frame using black to indicate vacant space, and white to indicate occupied space.</param>
        void notifyFrameArrival(int frameNo, List<Box> boxes, Bitmap mask);

        /// <summary>
        /// Processes a frame upon arrival.
        /// </summary>
        /// <param name="frameNo">The index of the frame to process.</param>
        /// <param name="mask">A mask detailing the precise layout of items in the frame using black to indicate vacant space, and white to indicate occupied space.</param>
        void notifyFrameArrival(int frameNo, Bitmap mask);

        //TODO(iharwell): This should be moved somewhere more appropriate.
        /// <summary>
        /// Activates debug logging.
        /// </summary>
        void setDebug();

        /// <summary>
        /// Provides a history of the occupancy of this line detector, with each entry containing a list of occupancy values for each line considered by this detector.
        /// </summary>
        List<List<double>> getOccupancyHistory();

        /// <summary>
        /// Gets the <c>DetectionLine</c> used by this detector.
        /// </summary>
        DetectionLine getDetectionLine();

        /// <summary>
        /// Gets the current occupancy state of this detector. This updates when the detector is notified of a frame arrival.
        /// </summary>
        /// <returns>Returns true if the line is occupied, and false otherwise.</returns>
        bool getOccupancy();

        /// <summary>
        /// Gets the number of times that this detector has been triggered.
        /// </summary>
        /// <returns></returns>
        int getCount();

        //TODO(iharwell): This seems like it should not be part of the interface.
        /// <summary>
        /// Sets the count of this detector.
        /// </summary>
        /// <param name="value"></param>
        void setCount(int value);

        /// <summary>
        /// Gets the bounding box of the line used by this detector.
        /// </summary>
        /// <returns></returns>
        Box getBbox();

        /// <summary>
        /// Gets a <c>Dictionary</c> of the parameters used by this detector, stored by name.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, Object> getParameters();

        /// <summary>
        /// Gets the line segments used by this detector.
        /// </summary>
        /// <returns></returns>
        List<(Point p1, Point p2)> getLineCoor();
    }
}
