using System.Collections;
using System.Collections.Generic;
using Template;
using UnityEngine;

namespace GameCore.UI
{
    public class UIShop : UIElementAnimated
    {
        public void GetCoins()
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            UINotEnoughResources.GetCoins();
        }
    }
}