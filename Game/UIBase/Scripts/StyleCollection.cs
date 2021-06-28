using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Template.Tools;
using System.Linq;
using System;

namespace Template.UIBase
{
    [CreateAssetMenu(fileName = "StyleCollection", menuName = "Template/UIBase/StyleCollection")]
    public class StyleCollection : ScriptableObject
    {
        private static StyleCollection _instance = null;
        public static StyleCollection Instance
        {
            get
            {
                return _instance ?? (_instance = Resources.Load<StyleCollection>("StyleCollection"));
            }
        }

        public TMPMaterialsCollection tmpMaterials = new TMPMaterialsCollection();
        public ColorCollection colorCollection = new ColorCollection();

        [Serializable]
        public class ButtonReference : KeyObjectPair<string, Sprite> {}
        [SerializeField] private ButtonReference[] _buttons;
        
        public Sprite GetButtonSprite(string key)
        {
            return _buttons.FirstOrDefault(r => r.Key == key)?.Value ?? throw new ArgumentException($"Button sprite not registered for key {key}");
        }


        [System.Serializable]
        public struct TMPMaterialsCollection
        {
            public Material outlineGreen;
            public Material outlineThinGreen;
            public Material outlineOrange;
            public Material outlineRed;
            public Material outlineBrown;
            public Material outlineThinBrown;
            public Material outlineAqua;

            public Material complexRed;
            public Material complexGreen;

            public Material shadow;
        }

        [System.Serializable]
        public struct ColorCollection
        {
            public Color DefaultText;
            public Color WarningRed;
            public Color WarningOrange;
            public Color AcceptGreen;
            public Color White;
            public Color GemPurple;
            public Color GemPink;
            public Color CoinGold;
        }
    }
}