using System.Collections;
using System.Collections.Generic;
using Template;
using Template.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIScoreMultiplier : MonoBehaviour
    {
        [SerializeField] private BroTweener textTween;
        [SerializeField] private TextMeshProUGUI multiplierText;

        [SerializeField] private Image fillImage;

        [SerializeField] private int MaxMultiplier = 7;

        [HideInInspector]
        public int Multiplier = 1;
        //private int gettedPairs = 0;

        private void OnEnable()
        {
            Reset();
        }

        private void Reset(bool witAnim = false)
        {
            Multiplier = 1;
            //gettedPairs = 0;
            fillImage.fillAmount = 0f;
            SetText("x1", witAnim);
        }

        public void AddScoreTrigger(System.Action onEndAnim = null)
        {
            StopAllCoroutines();
            StartCoroutine(AddScore(onEndAnim));
        }

        public void SetText(string text, bool isAnim)
        {
            multiplierText.text = text;

            if (isAnim)
            {
                textTween.Play();
            }
        }

        public void SetFill(float fillAmount)
        {
            StartCoroutine(FillMultiplier(fillAmount));
        }

        private IEnumerator AddScore(System.Action onEndAnim = null)
        {
            //gettedPairs++;

            yield return FillMultiplier(Multiplier == 1f ? Mathf.Min(fillImage.fillAmount + 0.33f, 1f) : 1f);

            if (fillImage.fillAmount == 1f)
            {
                var isAnim = Multiplier != MaxMultiplier;
                Multiplier = Mathf.Min(++Multiplier, MaxMultiplier);
                SetText($"x{Multiplier}", isAnim);

                if (isAnim) AudioManager.Play(new PlaySettings($"Multiplier_{Multiplier}"));

                //yield return FillMultiplier(1f / Multiplier);
                //gettedPairs = 0;
            }

            onEndAnim?.Invoke();

            StartCoroutine(Defill(Multiplier == 1f ? 12f : 5f));

            yield return null;
        }

        private IEnumerator FillMultiplier(float targetFill, float targetTime = 0.2f)
        {
            if (Multiplier > 1 && targetFill == 1f)
                UIManager.GetElement<UIBattleHeader>().AddReward(Multiplier);

            float startFill = fillImage.fillAmount;
            float leftTime = 0f;

            while (fillImage.fillAmount != targetFill)
            {
                fillImage.fillAmount = Mathf.Lerp(startFill, targetFill, leftTime / targetTime);
                yield return null;
                leftTime += Time.deltaTime;
            }

            yield break;
        }

        private IEnumerator Defill(float waitTime)
        {
            //yield return new WaitForSeconds(waitTime);

            float startFill = fillImage.fillAmount;
            float leftTime = 0f;

            while (fillImage.fillAmount > 0)
            {
                fillImage.fillAmount = Mathf.Lerp(startFill, 0f, leftTime / (Multiplier == 1f ? 12f : 5f));
                yield return null;
                leftTime += Time.deltaTime;
            }
            yield return new WaitForSeconds(0.5f);

            Reset(true);
            AudioManager.Play(new PlaySettings($"MultiplierFail"));

            yield break;
        }
    }
}