using UnityEngine;
using UnityEngine.UI;

namespace Template.UIBase
{
    /// <summary>
    /// Базовая реализация кнопки закрытия экрана, чтобы не добавлять обработчик OnClose в каждый UIElement
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIBackButton : MonoBehaviour
    {
        public void OnBackButtonPressed()
        {
            UIManager.CloseTopState();
        }
    }
}