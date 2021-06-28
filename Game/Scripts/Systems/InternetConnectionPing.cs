using GameCore;
using System;
using System.Collections;
using System.Collections.Generic;
using Template.Tools;
using UnityEngine;
using UnityEngine.Networking;

namespace GameCore
{
    public enum InternetConnectionType 
    {
        HasConnect,
        Disconnect
    }
    public class InternetConnectionPing : MonoBehaviour
    {
        private static InternetConnectionPing inst;

        public static void HasInternet(Action<InternetConnectionType> Action)
        {
            inst?.StartCoroutine(TestInternet(Action));
        }

        private void Start()
        {
            inst = this;
        }

        private static bool isCheck;


        private void Update()
        {
        }

        public static IEnumerator TestInternet(Action<InternetConnectionType> Action)
        {
            //Пингуем сервер карт
            var ping = new Ping("46.4.4.73");
            isCheck = true;
            Timer timer = new Timer().Set(2f , true);

            Debug.LogError("Start Ping");

            while(!timer.IsComplete && !ping.isDone)
            {
                yield return null;
            }

            Debug.LogError($"End Ping with result { ping.isDone } and Time {ping.time}");

            Action(ping.time != -1 ? InternetConnectionType.HasConnect : InternetConnectionType.Disconnect);

            ping.DestroyPing();

            //var www = new UnityWebRequest("https://google.com");
            //www.timeout = 2;

            //yield return www.SendWebRequest();

            //if (www.isNetworkError || www.isHttpError)
            //{
            //    Action(InternetConnectionType.Disconnect);                
            //}
            //else
            //{
            //    Action(InternetConnectionType.HasConnect);
            //}
        }
    }
}
