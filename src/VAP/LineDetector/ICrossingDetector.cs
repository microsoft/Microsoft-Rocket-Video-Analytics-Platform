namespace LineDetector
{
    public enum OCCUPANCY_STATE { OCCUPIED, UNOCCUPIED };
    interface ICrossingDetector
    {
        bool notifyOccupancy(int frameNo, bool occupancy);
        OCCUPANCY_STATE getState();
    }
}
