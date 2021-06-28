using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Template.UIBase
{
    [RequireComponent(typeof(Button))]
    public class UIButtonAnimator : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Transform targetTransform;
        public BroTweener pressTweener;
        public BroTweener clickTweener;

        private Button _button;

        private void Awake()
        {
            if (!_button) _button = GetComponentInParent<Button>();
            if (!targetTransform) targetTransform = transform.parent;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressTweener.enabled = true;
            clickTweener.enabled = false;
            pressTweener.Target = targetTransform;
            pressTweener.Play();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressTweener.PlayReverse();
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            pressTweener.enabled = false;
            clickTweener.enabled = true;
            clickTweener.Target = targetTransform;
            clickTweener.Play();
            clickTweener.SetOnFinish((state) => clickTweener.enabled = false);
        }
    }
}