namespace LineDetector
{
    /// <summary>
    /// Indicates whether a detector is currently occupied or not.
    /// </summary>
    public enum OCCUPANCY_STATE
    {
        /// <summary>
        /// Indicates that at least one item is present.
        /// </summary>
        OCCUPIED,
        /// <summary>
        /// Indicates that no items are present.
        /// </summary>
        UNOCCUPIED
    };

    /// <summary>
    /// Interface for a crossing detector that fires as a function of occupancy.
    /// </summary>
    interface ICrossingDetector
    {
        /// <summary>
        /// Notifies the detector of a new occupancy state at a given frame.
        /// </summary>
        /// <param name="frameNo">The index of the frame of interest.</param>
        /// <param name="occupancy">The occupancy state at that frame.</param>
        /// <returns>Returns true if an event was detected, and false otherwise.</returns>
        bool notifyOccupancy(int frameNo, bool occupancy);

        //TODO(iharwell): Should be a property.
        /// <summary>
        /// Gets the occupancy state of the detector as of the latest frame.
        /// </summary>
        /// <returns></returns>
        OCCUPANCY_STATE getState();
    }
}
