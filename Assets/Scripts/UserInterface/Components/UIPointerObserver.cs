using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UserInterface.Components
{
    public class UIPointerObserver : MonoBehaviour, 
        IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Pointer Events")]
        public UnityEvent onPointerClick;
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke();
        }
    }
}