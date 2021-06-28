using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Template.UIBase
{
    public class UIFade : UIElementAnimated, IPointerClickHandler
    {

        public void OnPointerClick(PointerEventData eventData)
        {

            UIState topState;
            if (UIManager.GetTopState(out topState))
            {
                if (topState.IsEasyToClose)
                {
                    UIManager.CloseTopState();
                }
            }
        }

        public override void Hide()
        {
            ApplySorting(UIManagerRoot.sortingOrderBase);

            base.Hide();
        }
    }
}