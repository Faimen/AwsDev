using GameCore.Common;
using Template;
using Template.Systems.Dialogs;

namespace GameCore.UI
{
    public class UINotEnoughResources : UIElementAnimated
    {
        public static void GetCoins()
        {
            AdsManager.ShowAd(AdType.Rewarded, "20_coins", (result) =>
            {
                if (result == AdsResult.Finished)
                {
                    UserHelper.AddCoins(20);
                    Dialog.ShowDialog(dialog => DialogPresets.InitAsSimpleDialog(dialog, "", "<size=55>" + "Common.GetCoins".Localized(), true, "Common.OK".Localized()), dialogResult => { });
                }
            }, false);
        }

        /// <summary>
        /// Для кнопки, не могу использовать статический метод
        /// </summary>
        public void CoinsForAds()
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            GetCoins();
        }
    }
}