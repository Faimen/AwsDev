using System.Collections;
using System.Collections.Generic;
using Template.Tools.Pool;
using UnityEngine;

namespace GameCore.UI.GameTools
{
    public class UILoadLevelsList : MonoBehaviour
    {
        [SerializeField] private Transform contentTransform;

        [SerializeField] private UILoadlLevelItem levelPrefab;

        private ObjectPool<UILoadlLevelItem> itemsPool = new ObjectPool<UILoadlLevelItem>();

        private void OnEnable()
        {
            itemsPool.DeactivateAll();

            foreach(var level in LevelsConfig.GetConfig.levelList)
            {
                var newItem = itemsPool.Get(levelPrefab, contentTransform);
                newItem.transform.SetAsLastSibling();

                newItem.SetData(level);
                newItem.gameObject.SetActive(true);
            }
        }
    }
}