namespace Watermelon.BusStop
{
    public class WallElementBehavior : LevelElementBehavior
    {
        public override void Initialise(LevelElement levelElement, ElementPosition elementPosition)
        {
            base.Initialise(levelElement, elementPosition);
        }

        public override void Unload()
        {

        }

        public override bool IsPlayableElement()
        {
            return false;
        }
    }
}
