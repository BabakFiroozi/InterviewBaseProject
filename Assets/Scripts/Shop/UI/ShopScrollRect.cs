using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shop.Views
{
    /// <summary>
    /// Vertical scroll list that hands horizontal drags to its parents, so swiping sideways on the shop still
    /// switches tabs (the tab panel is an ancestor) while vertical drags scroll the list.
    /// </summary>
    public sealed class ShopScrollRect : ScrollRect
    {
        private bool _routeToParent;

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            ForwardToParent(eventData, ExecuteEvents.initializePotentialDrag);
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            var moved = eventData.position - eventData.pressPosition;
            _routeToParent = Mathf.Abs(moved.x) > Mathf.Abs(moved.y);
            if (_routeToParent)
                ForwardToParent(eventData, ExecuteEvents.beginDragHandler);
            else
                base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_routeToParent)
                ForwardToParent(eventData, ExecuteEvents.dragHandler);
            else
                base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent)
                ForwardToParent(eventData, ExecuteEvents.endDragHandler);
            else
                base.OnEndDrag(eventData);

            _routeToParent = false;
        }

        private void ForwardToParent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
        {
            if (transform.parent != null)
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, function);
        }
    }
}
