using GameCore.UI;
using System.Collections;
using Template;
using Template.GameUsers;
using Template.Systems.StateMachine;

namespace GameCore
{
    public class State_Start : SMState
    {
        public override IEnumerator Enter()
        {
            if(GameManager.Instance.GameUser == null)
            {
                GameManager.Instance.GameUser = new GameUser(new PlayerPrefsGameUserImpl(Machine.initiator, "UserConfig"));
                yield return GameManager.Instance.GameUser.Load();
            }

            UIManager.SetBaseStateAndActivate(UIStateCollection.MainMenu);
            UIManager.GetElement<UIBackground>().Set(UIBackground.BackTypes.Menu);

            yield break;
        }
    }
}