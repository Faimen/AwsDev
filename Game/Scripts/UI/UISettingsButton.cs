using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UISettingsButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private List<Sprite> statesSprites;
        [SerializeField] private Image buttonGraphic;

        [System.Serializable]
        public class OnChange : UnityEvent<bool> { }
        public OnChange onChange;

        public bool Value { get; private set; }

        public void OnPointerClick(PointerEventData eventData)
        {
            Set(!Value);
            AudioManager.Play(new PlaySettings("ButtonClick"));
        }

        public void Set(bool newValue)
        {
            SetBase(newValue);
            onChange?.Invoke(Value);
        }

        public void SetBase(bool newValue)
        {
            Value = newValue;
            buttonGraphic.sprite = statesSprites[Value ? 0 : 1];
        }
    }
}