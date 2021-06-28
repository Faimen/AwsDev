using UnityEngine;
using System.Collections;
using GameCore.UI;
using System;
using PluginsRoot.Network;

namespace GameCore.Common
{

    public class AdsManager
    {
        public static bool isShowAds = true;


        private static IEnumerator new_ShowAds(AdType adType, string placement, Action<AdsResult> onShow, float adDelay = 0f)
        {
#if UNITY_EDITOR
            Debug.Log(placement);

            //Statistic.SendPlayerShowAds();
            ////Реклама показана
            //Statistic.SendMarketing(AFAppEvents.SHOW_ADS);
            ////Какой тип рекламы был показан
            //Statistic.SendMarketing(adType == AdType.Interstitial ? AFAppEvents.SHOW_ADS_INT : AFAppEvents.SHOW_ADS_RV);

            onShow(AdsResult.Finished);
            yield break;
#endif

            #region IS Admin
            if (!isShowAds)
            {
                onShow(AdsResult.Finished);
                yield break;
            }
            #endregion

            if (adDelay != 0)
                yield return new WaitForSeconds(adDelay);

            bool isReady = false;
            bool isPlayed = false;
            bool isComplited = false;
            AdsResult result = AdsResult.Failed;

            bool hasIntenet = false;

            yield return InternetConnectionPing.TestInternet((connect) => hasIntenet = connect == InternetConnectionType.HasConnect);

            try
            {
                if (hasIntenet)
                {
                    //Если реклама готова и есть интернет - показываем ее
                    switch (adType)
                    {
                        case AdType.Rewarded:

                            isReady = AppLovinController.IsRewardedAdsReady;
                            if (isReady)
                            {
                                AppLovinController.ShowRewardedAd(placement, res =>
                                {
                                    result = res;
                                    isComplited = true;
                                });

                                isPlayed = true;
                            }
                            break;

                        case AdType.Interstitial:
                            isReady = AppLovinController.IsInterstitialAdsReady;
                            if (isReady)
                            {
                                AppLovinController.ShowInterstitial(placement, res =>
                                {
                                    result = res;
                                    isComplited = true;
                                });

                                isPlayed = true;
                            }
                            break;
                    }
                }

                //Если реклама не готова или не показывается , то выводим диалоговое окно
                if (!isReady || !isPlayed)
                {
                    string message;
                    if (hasIntenet)
                        message = "AdsLoading.Waiting";
                    else
                        message = "AdsLoading.NoInternet";

                    //NoInternetMessage.ShowMessage?.Invoke(message.Localized());

                    result = AdsResult.Failed;
                    isComplited = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                isComplited = true;
                result = AdsResult.Failed;
            }


            //Ждем завершение показа
            while (!isComplited) { yield return null; }

            //NoInternetMessage.ForceDisable?.Invoke();

            //задержка после закрытия рекламы
            yield return new WaitForSecondsRealtime(0.7f);

            //статистика завершился показ
            if (result == AdsResult.Finished)
            {
                //Statistic.SendPlayerShowAds();
                ////Реклама показана
                //Statistic.SendMarketing(AFAppEvents.SHOW_ADS);
                ////Какой тип рекламы был показан
                //Statistic.SendMarketing(adType == AdType.Interstitial ? AFAppEvents.SHOW_ADS_INT : AFAppEvents.SHOW_ADS_RV);

            }
            //Статистика если рекламу скипнули и это интерстишл 
            else if (result == AdsResult.Skipped && adType == AdType.Interstitial)
            {
                ////Реклама показана
                //Statistic.SendMarketing(AFAppEvents.SHOW_ADS);
                ////Какой тип рекламы был показан
                //Statistic.SendMarketing(adType == AdType.Interstitial ? AFAppEvents.SHOW_ADS_INT : AFAppEvents.SHOW_ADS_RV);
            }

            //Вызываем экшен после рекламы
            onShow(result);
        }
             
        
        /// <summary>
        /// Показываем интерстишл рекламу Если есть интернет и загружена реклама
        /// </summary>
        public static void ShowAdsWithNotification(string placement, Action<AdsResult> onShow, bool isPremiumDisable, string message)
        {
            Action<InternetConnectionType> onInternet = (value) =>
           {
               if (value == InternetConnectionType.Disconnect)
               {
                   if (!CanShow(AdType.Interstitial))
                   {
                       onShow?.Invoke(AdsResult.Failed);
                       return;
                   }
               }
               else
               {
                   //AdaptersConfig.Inst.ControlEnabled = false;
                   //AdaptersConfig.Inst.DisableAdapter();
                   //onShow += result =>
                   //{
                   //    AdaptersConfig.Inst.ControlEnabled = true;
                   //    AdaptersConfig.Inst.EnableAdapter();
                   //};                   
                   //NoInternetMessage.ShowMessage?.Invoke(message);
                   ShowAd(AdType.Interstitial, placement, onShow, isPremiumDisable, 2.0f);
               }
           };
            InternetConnectionPing.HasInternet(onInternet);

            //if (!NetSystem.HasInternetConnection || !CanShow(AdType.Interstitial))
            //{
            //    onShow?.Invoke(AdsResult.Failed);
            //    return;
            //}

            //AdaptersConfig.Inst.ControlEnabled = false;
            //AdaptersConfig.Inst.DisableAdapter();
            //onShow += result =>
            //{
            //    AdaptersConfig.Inst.ControlEnabled = true;
            //    AdaptersConfig.Inst.EnableAdapter();
            //};
            //NoInternetMessage.ShowMessage?.Invoke(message);
            //ShowAd(AdType.Interstitial, placement, onShow, isPremiumDisable, 2.0f);
        }

        public static void ShowAd(AdType type, string placement, Action<AdsResult> onShow, bool isPremiumDisable, float adDelay = 0f)
        {
            if(GameManager.Environment == PluginsRoot.Network.Environment.Local)
            {
                onShow.Invoke(AdsResult.Finished);
                return;
            }

            //Выключение рекламы если купили SpecialOffer или не прошли Tutorial
            //if ((UserData.IsPremium && isPremiumDisable) || (!SaveManager.IsFinishGameTutorial && UserData.IsZeroExp))
            //{
            //    onShow.Invoke(AdsResult.Finished);
            //    return;
            //}

            GameManager.Instance.StartCoroutine(new_ShowAds(type, placement, onShow, adDelay));
        }

        public static void TestSetAds(bool value)
        {
            isShowAds = value;
        }

        private static bool CanShow(AdType type)
        {
            switch (type)
            {
                case AdType.Interstitial:
                    return AppLovinController.IsInterstitialAdsReady;
                    
                case AdType.Rewarded:
                    return AppLovinController.IsRewardedAdsReady;

                default: return false;
            }
        }
    }

    public enum AdType
    {
        Rewarded,
        Interstitial
    }


}