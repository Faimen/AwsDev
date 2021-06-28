using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Template.UIBase
{
    public class UIBadge : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI textValue;

        private int value = 0;

        /// <summary>
        /// Значение бейджика
        /// </summary>
        public int Value
        {
            get
            {
                return value;
            }

            set
            {
                gameObject.SetActive(value != 0);

                this.value = value;
                textValue.text = this.value.ToString();
            }
        }
    }
}