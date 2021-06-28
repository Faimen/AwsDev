using Template.GameUsers;
using Template.Systems.StateMachine;
using Template.Tools;
using UnityEngine;
using Environment = PluginsRoot.Network.Environment;


namespace GameCore
{
    public class GameManager : Singleton_MonoBehaviour<GameManager>
    {
        public StateMachine StateMachine { get; private set; }
        public GameUser GameUser;

        void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            StateMachine = new StateMachine("Game", this);
            // Fix? Remove?
            // UICover.Show<UI.UICover_Start>(new CoverSettings() { immediately = true });
            StateMachine.SetState(new State_Start());
        }


        // private void OnApplicationPause(bool pause)
        // {
        //     //Если мы вошли в приложение после сворачивания
        //     if (pause && GameNet.IsReady)
        //     {
        //         GameEvents.OnGameEnter.Invoke();
        //     }
        // }

        public static Environment Environment
        {
            get => PluginsRoot.PluginsConfig.Get().Server.GetEnvironment();
        }
        public static bool IsProduction => Environment == Environment.Production;
    }
}
