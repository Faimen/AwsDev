using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using TMPro;
using Template.UIBase;

namespace Template.UIBase
{
    public enum ButtonViewPreset { green, orange, red, brown }

    [RequireComponent(typeof(Button))]
    public class UIBaseButton : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI text;

        [SerializeField] private Button button;

        public void SetText(string txt)
        {
            text.text = txt;
        }

        public void SetTextColor(Color color)
        {
            text.color = color;
        }

        public void SetTextMaterial(Material material)
        {
            text.fontMaterial = material;
        }

        public void SetColor(Color color)
        {
            image.color = color;
        }

        public void SetSprite(Sprite sprite)
        {
            image.sprite = sprite;
        }

        public void SetAction(Action action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }

        public void SetView(ButtonViewPreset type)
        {
            switch (type)
            {
                case ButtonViewPreset.orange:
                    //image.color = StyleCollection.Instance.colorCollection.WarningOrange;
                    SetSprite(StyleCollection.Instance.GetButtonSprite("button_orange"));
                    SetTextMaterial(StyleCollection.Instance.tmpMaterials.outlineOrange);
                    break;
                case ButtonViewPreset.red:
                    //image.color = StyleCollection.Instance.colorCollection.WarningRed;
                    SetSprite(StyleCollection.Instance.GetButtonSprite("button_red"));
                    SetTextMaterial(StyleCollection.Instance.tmpMaterials.outlineRed);
                    break;
                case ButtonViewPreset.brown:
                    SetSprite(StyleCollection.Instance.GetButtonSprite("button_brown"));
                    SetTextMaterial(StyleCollection.Instance.tmpMaterials.outlineBrown);
                    break;
                default:
                    //image.color = StyleCollection.Instance.colorCollection.AcceptGreen;
                    SetSprite(StyleCollection.Instance.GetButtonSprite("button_green"));
                    SetTextMaterial(StyleCollection.Instance.tmpMaterials.outlineThinGreen);
                    break;
            }
        }

    }
}