using System.Collections;
using Template;
using Template.UIBase;
using TMPro;
using UnityEngine;

namespace GameCore.UI
{
    public class UIBattleHeader : UIElementAnimated
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI tilesText;

        [SerializeField] private UIScoreMultiplier scoreMultiplier;

        [SerializeField] private UIBattleTimer battleTimer;

        private int tiles;
        private int scores;

        private int reward;


        private void OnEnable()
        {
            reward = 0;
            battleTimer.gameObject.SetActive(false);

            UIManager.GetElement<UIBattleField>().OnFindMatch += OnFindMatchHandler;
        }

        private void OnDisable()
        {
            UIManager.GetElement<UIBattleField>().OnFindMatch -= OnFindMatchHandler;
        }

        private void OnFindMatchHandler(TileData tile1, TileData tile2)
        {
            scoreMultiplier.AddScoreTrigger();
            StartCoroutine(UpdateData());
        }

        private IEnumerator UpdateData()
        {
            scores += 25 * scoreMultiplier.Multiplier;
            tiles -= 2;

            UpdateLabels(scores, tiles);

            if (tiles == 0)
            {
                yield return new WaitWhile(() => UIManager.GetElement<UIBattleField>().isMatchingAnimPlay);

                scoreMultiplier.StopAllCoroutines();
                battleTimer.Destroy();

                UIManager.AddState(typeof(UIFade), typeof(UIRewardScreen));

                //Награда за прохождение
                reward += 5;

                UserHelper.AddCoins(reward);
                UIManager.GetElement<UIRewardScreen>().SetContent(scores, reward);
            }
        }

        public void Init(int scores, int tiles)
        {
            this.scores = scores;
            this.tiles = tiles;

            UpdateLabels(scores, tiles);
        }

        public void UpdateLabels(int scores, int tiles)
        {
            scoreText.text = $"{scores}";//I2Loc.Get("Common.Battle.Scores", ("scores", scores));
            tilesText.text = $"{tiles}";//I2Loc.Get("Common.Battle.Pairs", ("pairs", pairs));
        }

        public void AddReward(int value)
        {
            reward += value;
        }

        public void SetTimer(float time)
        {
            battleTimer.Init(time);
            battleTimer.gameObject.SetActive(true);
        }
    }
}