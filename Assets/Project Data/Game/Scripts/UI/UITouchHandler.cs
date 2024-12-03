#pragma warning disable 0414

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{

    // UI Module v0.9.0
    public class UITouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private bool isMouseDown = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            isMouseDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isMouseDown = false;
        }
    }
}