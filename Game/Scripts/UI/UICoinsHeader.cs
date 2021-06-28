using GameCore.Data;
using TMPro;
using UnityEngine;

namespace GameCore.UI
{
    public class UICoinsHeader : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinsValue;

        private void OnEnable()
        {
            GameManager.Instance.GameUser.Items.Get<CoinsItem>().OnAmountChanged += UpdateValue;
            UpdateValue(GameManager.Instance.GameUser.Items.Get<CoinsItem>().Amount);
        }

        private void OnDisable()
        {
            GameManager.Instance.GameUser.Items.Get<CoinsItem>().OnAmountChanged -= UpdateValue;
        }

        private void UpdateValue(long value)
        {
            coinsValue.text = $"<sprite=\"coinIcon\" index=0> {GameManager.Instance.GameUser.Items.Get<CoinsItem>().Amount:N0}";
            coinsValue.ForceMeshUpdate(true, true);

            //StartCoroutine(UpdateVisual(value));
        }

        public void AddCoins()
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));

            //Стейт магазина, скрыл пока нет покупок
            //UIManager.AddState(new UIState(typeof(UIFade), typeof(UIShop)) { IsEasyToClose = true });

            UINotEnoughResources.GetCoins();

            //if (GameManager.Environment == PluginsRoot.Network.Environment.Production)
            //    UINotEnoughResources.GetCoins();
            //else
            //    UserHelper.AddCoins(100);
        }

        //private IEnumerator UpdateVisual(long value)
        //{
        //    coinsValue.SetAllDirty

        //    coinsValue.text
        //}
    }
}
