using AppsFlyerSDK;
using System.Collections.Generic;
using Template;
using Template.Systems.Dialogs;
using TMPro;
using UnityEngine;

namespace GameCore.UI
{
    public class UIRewardScreen : UIElementAnimated
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI titleText;

        public void SetContent(int score, int reward)
        {
            scoreText.text = score.ToString();
            rewardText.text = reward.ToString();
        }

        private void OnEnable()
        {
            AudioManager.Play(new PlaySettings("RewardScreen"));
            titleText.text = $"Common.RewardScreen.Title.{Random.Range(1, 6)}".Localized();

            var values = new Dictionary<string, string>();
            values.Add(AFInAppEvents.QUANTITY, "1");
            AppsFlyer.sendEvent($"af_lvl_{State_Battle.level + 1}", values);
        }

        private void OnDisable()
        {
            AudioManager.Stop(new StopSettings("RewardScreen"));
        }

        public void ToMainMenu()
        {
            State_Battle.level++;
            UserHelper.SetGameLevel(State_Battle.level);

            AudioManager.Play(new PlaySettings("ButtonClick"));

            //if (State_Battle.level > 2 && State_Battle.level % 3 == 0)
            //{
            //    AdsManager.ShowAd(AdType.Interstitial, "levelCompleted", result => GameManager.Instance.StateMachine.SetState(new State_Start()), false);
            //}
            //else
            GameManager.Instance.StateMachine.SetState(new State_Start());

            if (State_Battle.level == 7 && !PlayerPrefs.HasKey("LeaveReview"))
            {
                Dialog.ShowDialog(dialog => MahjongDialogPresets.InitAsReviewDialog(dialog), result =>
                {
                    if (result is Ok_DialogButton)
                    {
                        Application.OpenURL("https://play.google.com/store/apps/details?id=com.skytec.mahjong");
                        PlayerPrefs.SetInt("LeaveReview", 1);
                        PlayerPrefs.Save();
                    }
                });
            }

        }
    }
}