using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.Map
{
    public class MapLevelBehavior : MapLevelAbstractBehavior
    {
        [SerializeField] Image innerCircle;

        [Space]
        [SerializeField] Color reachedText;
        [SerializeField] Color reachedCircle;
        [Space]
        [SerializeField] Color openedText;
        [SerializeField] Color openedCircle;
        [Space]
        [SerializeField] Color closedText;
        [SerializeField] Color closedCircle;

        protected override void InitOpen()
        {
            levelNumber.color = openedText;
            innerCircle.color = openedCircle;

            button.gameObject.SetActive(true);
        }

        protected override void InitClose() 
        {
            levelNumber.color = closedText;
            innerCircle.color = closedCircle;

            button.gameObject.SetActive(false);
        }

        protected override void InitCurrent()
        {
            levelNumber.color = reachedText;
            innerCircle.color = reachedCircle;

            button.gameObject.SetActive(true);
        }
    }
}