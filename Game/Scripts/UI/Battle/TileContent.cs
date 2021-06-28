using System.Collections;
using System.Collections.Generic;
using Template.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class TileContent : MonoBehaviour
    {
        public TileData TileData;

        [SerializeField] protected Image image;
        [SerializeField] protected Image shadow;

        public GameObject Selection;
        public GameObject Highlight;

        public static Dictionary<int, int> SpritesId = new Dictionary<int, int>();

        public virtual void Set(TileData tileData)
        {
            TileData = tileData;

            SetSpawnAnim();

            if (TileData.Id == 0)
            {
                //GetComponent<Image>().enabled = false;
                //image.enabled = false;
                return;
            }

            if(TileData.Id == -1)
            {
                image.enabled = false;
                Debug.LogError("-1 spawn index");
                return;
            }

            var sprites = Resources.LoadAll<Sprite>($"Tiles/{TileData.Id}/");

            if (SpritesId.ContainsKey(TileData.Id))
            {
                image.sprite = sprites[SpritesId[TileData.Id]];
            }
            else
            {
                var newKeyValue = sprites.Length == 1 ? 0 : Random.Range(0, sprites.Length);
                SpritesId.Add(TileData.Id, newKeyValue);
                image.sprite = sprites[newKeyValue];
                //image.sprite = sprites[sprites.Length == 1 ? 0 : Random.Range(0, sprites.Length)];
            }

            shadow.enabled = TileData.Layer != 0;
        }

        public virtual void SetOffset(OffsetData offsetData, float xOffset, float yOffset)
        {
            var rect = GetComponent<RectTransform>();

            Vector2 offset = new Vector2(offsetData.Left ? -xOffset : (offsetData.Right ? xOffset : 0),
                                         offsetData.Down ? -yOffset : (offsetData.Up ? yOffset : 0));
            rect.anchoredPosition += offset;
        }

        /// <summary>
        /// Устанавливает анимацию появление тайла(основывается на TileData)
        /// </summary>
        protected virtual void SetSpawnAnim()
        {
            var tweener = GetComponent<BroTweener>();
            var rect = GetComponent<RectTransform>();

            var gridData = LevelsConfig.GetConfig.levelList[State_Battle.level].grids[TileData.Layer];

            if (tweener == null)
            {
                tweener = gameObject.AddComponent<BroTweener>();
            }

            tweener.Duration = 0.45f;
            tweener.PlayOnEnable = true;

            tweener.Delay = 0.09f * (gridData.Width - TileData.xPos) + 0.14f * (gridData.Height - TileData.yPos) + 0.19f * (TileData.Layer + 1) /*- 0.42f*/;

            tweener.Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            tweener.TweenDataList.Clear();

            //Падение сверху
            tweener.AddTween(new Bro.TweenData(BroTweener.TypeEnum.TweenPosition)
            {
                fromVector3 = rect.anchoredPosition + Vector2.up * Screen.height / 2f,
                toVector3 = rect.anchoredPosition
            });

            //Появление из фейда
            tweener.AddTween(new Bro.TweenData(BroTweener.TypeEnum.TweenAlpha)
            {
                fromFloat = 0f,
                toFloat = 1f,
                rightEdge = 0.5f
            });

            //Звук по завершению анимации
            tweener.OnFinish.RemoveAllListeners();
            tweener.OnFinish.AddListener((call) => AudioManager.Play(new PlaySettings($"TileFall_{Random.Range(1, 6)}")));
        }
    }
}