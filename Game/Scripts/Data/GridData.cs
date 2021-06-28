using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    [System.Serializable]
    public class GridData
    {
        public int Width;
        public int Height;
        public int Layer;

        public bool xOffset;

        public FieldData[] Fields;

        public GridData()
        {
            Width = 1;
            Height = 1;
            Layer = 0;
            Fields = new FieldData[] { new FieldData() };
        }
    }


    [System.Serializable]
    public class FieldData
    {
        public int SpriteId;
        public OffsetData OffsetData;

        /*[System.NonSerialized]
        public bool isLocked;*/

        public FieldData()
        {
            SpriteId = 0;
            OffsetData = new OffsetData();
        }

        public FieldData(FieldData data)
        {
            SpriteId = data.SpriteId;
            OffsetData = new OffsetData();
            OffsetData.Up = data.OffsetData.Up;
            OffsetData.Down = data.OffsetData.Down;
            OffsetData.Left = data.OffsetData.Left;
            OffsetData.Right = data.OffsetData.Right;
        }
    }

    [System.Serializable]
    public class OffsetData
    {
        public bool Left;
        public bool Right;
        public bool Down;
        public bool Up;
    }

    //public enum TileCategory
    //{
    //    Empty,
    //    Beauty,
    //    Bones,
    //    Clothes,
    //    Cutlery,
    //    Electricity,
    //    Fruits,
    //    Pets,
    //    Pizza,
    //    Plants,
    //    Lunch,
    //    Football,
    //    Flowers,
    //    Space,
    //    Cars
    //}
}