using GameCore.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Template.Tools;
using UnityEngine;

namespace GameCore.UI.GameTools
{
    public class UIGrid_Editable : UIGrid
    {
        public CanvasGroup canvas;

        public void HideHelpBorders()
        {
            foreach(var pick in _elements)
            {
                ((UITile)pick).Content.GetComponent<TileContent_Editable>().HideBorders();
            }
        }

        protected override void OnElementPressed(UIGridPickElement element)
        {
            AudioManager.Play(new PlaySettings("TileClick"));

            var isElementAlreadySelected = Selected.Contains(element);
            if (isElementAlreadySelected)
            {
                DeselectElement(element);
                return;
            }

            // Isn't already selected
            if (!IsMultiSelect && IsElementSelected)
            {
                DeselectElement(Selected.Single());
            }
            SelectElement(element);
        }
    }

}
