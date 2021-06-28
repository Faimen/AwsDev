using GameCore.Data;
using GameCore.UI;
using Template;
using Template.UIBase;

namespace GameCore
{
    public class UserHelper
    {
        public static bool TrySpendCoins(long value)
        {
            if (GameManager.Instance.GameUser.Items.Get<CoinsItem>().Amount < value)
            {
                UIManager.AddState(new UIState(typeof(UIFade), typeof(UINotEnoughResources)) { IsEasyToClose = true });
                return false;
            }

            GameManager.Instance.GameUser.Items.Get<CoinsItem>().Amount -= value;
            GameManager.Instance.StartCoroutine(GameManager.Instance.GameUser.Save());
            return true;
        }

        public static void AddCoins(long value)
        {
            GameManager.Instance.GameUser.Items.Get<CoinsItem>().Amount += value;
            GameManager.Instance.StartCoroutine(GameManager.Instance.GameUser.Save());
        }

        public static void SetGameLevel(int level)
        {
            var json = new MiniJSON.JSONObject();
            json.Add("levelGame", level);
            GameManager.Instance.GameUser.Data.FromJson(json);
            GameManager.Instance.StartCoroutine(GameManager.Instance.GameUser.Save());
        }
    }
}