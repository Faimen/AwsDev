using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Template.UIBase
{

    public class UIToggle : MonoBehaviour, IPointerClickHandler
    {

        [SerializeField] private BroTweener tweener;

        [System.Serializable]
        public class OnChange : UnityEvent<bool> { }
        public OnChange onChange;


        /// <summary>
        /// Только визуальное состояние тоггла
        /// </summary>
        public bool Value
        {
            get; private set;
        }



        /// <summary>
        /// Обработчик клика по тогглу
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Set(!Value);
        }


        /// <summary>
        /// Засетать тоггл так же, как и по клику по нему
        /// </summary>
        public void Set(bool value)
        {
            Set(value, new Settings());
        }

        /// <summary>
        /// Задать начальное состояние тоггла, без вызовов и анимаций
        /// </summary>
        public void SetBase(bool value)
        {
            Set(value, new Settings() { animated = false, withEvent = false });
        }

        /// <summary>
        /// Засетать тоггл с настройками
        /// </summary>
        public void Set(bool value, Settings stngs)
        {
            Value = value;

            // Вызываем событие, если нужно
            if (stngs.withEvent)
            {
                onChange?.Invoke(Value);
            }

            // Если всё ещё не нашли, выходим
            if (tweener == null) return;


            if (stngs.animated)
            {
                if (Value) tweener.Play(false);
                else tweener.PlayReverse(false);
            } else
            {
                tweener.SetProgress(value ? 1 : 0);
            }

        }

        /// <summary>
        /// Настройки сета тоггла
        /// </summary>
        public class Settings
        {
            public bool withEvent = true;
            public bool animated = true;
        }
    }
}