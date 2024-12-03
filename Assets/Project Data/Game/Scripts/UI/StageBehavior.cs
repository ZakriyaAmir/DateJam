using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class StageBehavior : MonoBehaviour
    {
        [SerializeField] Image centerImage;

        [Space]
        [SerializeField] Color defaultColor;
        [SerializeField] Color activeColor;

        public void SetDefaultColor()
        {
            centerImage.color = defaultColor;
        }

        public void SetActiveColor()
        {
            centerImage.color = activeColor;
        }
    }
}
