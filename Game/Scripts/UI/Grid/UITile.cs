using GameCore.UI;
using System.Collections;
using System.Collections.Generic;
using Template.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Tools
{
    public class UITile : UIGridPickElement
    {
        [SerializeField] private Image hideShadowImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private GameObject highlight;

        public new TileContent Content { get; private set; }

        public void SetContent(int ind, TileContent content)
        {
            Ind = ind;
            Content = content;
            Content.transform.SetParent(transform, worldPositionStays: false);
            //Selection.GetComponent<RectTransform>().anchoredPosition += Content.GetComponent<RectTransform>().anchoredPosition;
            //Selection.transform.SetAsLastSibling();

            if (content.Selection) Selection = content.Selection;

            if (content.Highlight)
            {
                highlight = content.Highlight;
                //highlight.GetComponent<RectTransform>().anchoredPosition += Content.GetComponent<RectTransform>().anchoredPosition;
                //highlight.transform.SetAsLastSibling();
            }

            if (hideShadowImage)
            {
                hideShadowImage.GetComponent<RectTransform>().anchoredPosition += Content.GetComponent<RectTransform>().anchoredPosition;
                hideShadowImage.transform.SetAsLastSibling();
            }
        }

        public void SetHidden(bool isHide)
        {
            if (Content.TileData.Id == 0) return;

            canvasGroup.alpha = isHide ? 0.5f : 1f;

            hideShadowImage.enabled = isHide;
        }

        public void Highlight(bool isHighlight)
        {
            highlight.SetActive(isHighlight);
            Selection.SetActive(false);
        }
    }
}