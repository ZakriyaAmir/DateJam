using UnityEngine;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class DecorationElements
    {
        [SerializeField] Element[] decorationElements;

        public void Randomize(Bounds levelBounds)
        {
            for(int i = 0; i < decorationElements.Length; i++)
            {
                Vector3 objectPosition;

                do
                {
                    objectPosition = new Vector3(Random.Range(levelBounds.min.x, levelBounds.max.x), levelBounds.min.y - Random.Range(1.0f, 4.0f), 2.65f);
                }
                while (!IsPositionValid(objectPosition, i));

                decorationElements[i].Transform.position = objectPosition;
            }
        }

        private bool IsPositionValid(Vector3 position, int maxIndex)
        {
            maxIndex = Mathf.Clamp(maxIndex, 0, decorationElements.Length);

            bool isPositionValid = true;

            for(int i = 0; i < maxIndex; i++)
            {
                if(decorationElements[i].IsPositionInRange(position))
                {
                    isPositionValid = false;

                    break;
                }
            }

            return isPositionValid;
        }

        public void OnDrawGizmoSelected()
        {
            if(!decorationElements.IsNullOrEmpty())
            {
                Gizmos.color = Color.blue;

                for(int i = 0; i < decorationElements.Length; i++)
                {
                    if(decorationElements[i].Transform != null)
                        Gizmos.DrawWireCube(decorationElements[i].Transform.position, decorationElements[i].Size);
                }
            }
        }
    }
}
