using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UISoundButton : MonoBehaviour
    {
        [SerializeField] private Sprite enableSprite;
        [SerializeField] private Sprite disableSprite;

        [SerializeField] private Image buttonImg;

        private void OnEnable()
        {
            buttonImg.sprite = GameSettings.Sound ? enableSprite : disableSprite;
        }

        public void ChangeState()
        {
            GameSettings.Music = !GameSettings.Music;
            GameSettings.Sound = !GameSettings.Sound;

            buttonImg.sprite = GameSettings.Sound ? enableSprite : disableSprite;

            AudioManager.Play(new PlaySettings("ButtonClick"));
        }
    }
}