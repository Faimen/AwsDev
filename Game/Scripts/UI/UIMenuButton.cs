using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIMenuButton : MonoBehaviour
    {
        public void ToMainMenu()
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            GameManager.Instance.StateMachine.SetState(new State_Start());
        }
    }
}