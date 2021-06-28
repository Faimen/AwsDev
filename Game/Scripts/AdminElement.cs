using UnityEngine;
using System.Collections;

namespace GameCore.Utilities.UI
{

    public class AdminElement : MonoBehaviour
    {
        private void Awake()
        {
            if (!GameManager.IsProduction) return;

            gameObject.SetActive(false);
        }
    }
}