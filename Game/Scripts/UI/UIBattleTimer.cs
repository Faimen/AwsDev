using GameCore.Common;
using System.Collections;
using System.Collections.Generic;
using Template.Systems.Dialogs;
using Template.Tools;
using TMPro;
using UnityEngine;

namespace GameCore.UI
{
    public class UIBattleTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText;

        private Timer timer;

        private void OnDisable()
        {
            timer = null;
        }

        public void Init(float second)
        {
            timer = new Timer();
            timer.Set(second);
            timer.SetOnUpdate(() => timeText.text = "<sprite=\"timerIcon\" index=0>" + timer.Format(@"mm\:ss"));

            timer.SetOnFinish(() => Dialog.ShowDialog(dialog => MahjongDialogPresets.InitAsAdsDialog(dialog, "Common.BattleTimer.TimeIsOver".Localized(), "", true, "+" + "Common.BattleTimer.1min".Localized()),
                    result =>
                    {
                        AudioManager.Play(new PlaySettings("ButtonClick"));

                        if (result is Close_DialogButton)
                        {
                            GameManager.Instance.StateMachine.SetState(new State_Start());
                            return;
                        }

                        if (UserHelper.TrySpendCoins(50))
                            timer.AddSeconds(60f);
                        //AdsManager.ShowAd(AdType.Rewarded, "+1minute", (res) => timer.AddSeconds(60), false);
                    }));
        }

        public void Destroy()
        {
            timer = null;
        }

        private void Update()
        {
            if (timer != null) timer.Update();
        }
    }
}