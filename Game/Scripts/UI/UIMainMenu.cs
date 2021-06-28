using System.Collections;
using System.Collections.Generic;
using Template;
using Template.Systems.Dialogs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMainMenu : UIElementAnimated
    {
        [SerializeField] private TextMeshProUGUI levelText;

        private void OnEnable()
        {
            if (State_Battle.level > LevelsConfig.GetConfig.levelList.Count - 1)
            {
                State_Battle.level = 5;

                Dialog.ShowDialog(dialog => MahjongDialogPresets.InitAsEndGameDialog(dialog), result=> 
                {
                    if(result is Ok_DialogButton)
                    {
                        Application.OpenURL("https://play.google.com/store/apps/details?id=com.skytec.mahjong");
                    }
                });

                UserHelper.SetGameLevel(State_Battle.level);
            }
           
            levelText.text = I2Loc.Get("Common.MainMenu.Level", ("level", State_Battle.level + 1));
        }

        public void StartGame()
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            GameManager.Instance.StateMachine.SetState(new State_Battle());
            //LevelsConfig.GetConfig.Save();
        }

        public void OpenEditor()
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            GameManager.Instance.StateMachine.SetState(new State_Editor());
        }

        public void ChangeLevelValue(int value)
        {
            State_Battle.level += value;
            if (State_Battle.level < 0) State_Battle.level = LevelsConfig.GetConfig.levelList.Count - 1;
            if (State_Battle.level > LevelsConfig.GetConfig.levelList.Count - 1) State_Battle.level = 0;
            levelText.text = I2Loc.Get("Common.MainMenu.Level", ("level", State_Battle.level + 1));
        }
    }
}