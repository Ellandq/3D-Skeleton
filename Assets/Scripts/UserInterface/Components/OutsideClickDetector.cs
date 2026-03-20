using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface.Components
{
    [RequireComponent(typeof(Image))]
    public class OutsideClickDetector : MonoBehaviour, IPointerClickHandler
    {
        private Action _onOutsideClick;

        public void Subscribe(Action listener) => _onOutsideClick += listener;
        public void Unsubscribe(Action listenerToRemove) => _onOutsideClick -= listenerToRemove;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            _onOutsideClick?.Invoke();
        }
    }
}