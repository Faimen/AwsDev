using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.GameTools
{
    public class TileContent_Editable : TileContent
    {
        public FieldData FieldData;

        [SerializeField] private Image emptyImg;

        public void HideBorders()
        {
            emptyImg.enabled = false;
        }

        public override void Set(TileData tileData)
        {
            TileData = tileData;
            FieldData = new FieldData();

            emptyImg.enabled = TileData.Layer == State_Editor.editableGrid.Layer && TileData.Id == 0;
            GetComponent<Image>().color = TileData.Id == 0 ? new Color(1f, 1f, 1f, 0f) : Color.white;

            if (TileData.Id == 0 || TileData.Id == -1)
            {
                image.enabled = false;
                return;
            }

            var sprites = Resources.LoadAll<Sprite>($"Tiles/{TileData.Id}/");
            image.sprite = sprites[sprites.Length == 1 ? 0 : Random.Range(0, sprites.Length)];
        }
    }
}