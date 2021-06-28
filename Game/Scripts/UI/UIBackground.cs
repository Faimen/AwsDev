using System.Collections;
using System.Collections.Generic;
using Template;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIBackground : UIElement
    {
        [SerializeField] private List<Sprite> backgrounds;
        [SerializeField] private List<Sprite> backgroundsBattle;

        [SerializeField] private Image image;

        public void Set(BackTypes backType)
        {
            switch (backType)
            {
                case BackTypes.Game:
                    image.sprite = backgroundsBattle[Mathf.Min(State_Battle.level / 5, backgroundsBattle.Count - 1)];
                    break;
                default:
                    image.sprite = backgrounds[(int)backType];
                    break;
            }
        }

        public enum BackTypes
        {
            Menu,
            Editor,
            Game
        }
    }
}