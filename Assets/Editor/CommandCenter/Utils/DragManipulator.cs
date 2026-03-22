using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.CommandCenter.Utils
{
    public class DragManipulator : PointerManipulator
    {
        private readonly Action _onDragComplete;
        private Vector2 _start;

        public DragManipulator(VisualElement target, Action onDragComplete)
        {
            this.target = target;
            _onDragComplete = onDragComplete;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnDown);
            target.RegisterCallback<PointerUpEvent>(OnUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnDown);
            target.UnregisterCallback<PointerUpEvent>(OnUp);
        }

        private void OnDown(PointerDownEvent evt)
        {
            _start = (Vector2)evt.position;
        }

        private void OnUp(PointerUpEvent evt)
        {
            var end = (Vector2)evt.position;
            var delta = end - _start;

            if (Mathf.Abs(delta.x) > 30f)
            {
                _onDragComplete?.Invoke();
            }
        }
    }
}