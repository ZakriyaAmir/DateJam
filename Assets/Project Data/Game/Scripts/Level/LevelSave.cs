namespace Watermelon.BusStop
{
    [System.Serializable]
    public class LevelSave : ISaveObject
    {
        public int RealLevelNumber = 0;
        public int DisplayLevelNumber = 0;
        public bool ReplayingLevelAgain = false; // true when we lost level in randomization mode - so we want to reload the same level, not other random
        
        public void Flush()
        {

        }
    }
}
