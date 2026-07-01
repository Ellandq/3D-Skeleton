using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UserInterface.Components
{
    public class SliderReleaseListener : MonoBehaviour, IEndDragHandler
    {
        public Action onReleased;

        public void OnEndDrag(PointerEventData eventData)
        {
            onReleased?.Invoke();
        }
    }
}