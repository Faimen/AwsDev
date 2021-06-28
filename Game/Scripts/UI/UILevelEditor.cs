using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Template;
using GameCore.Tools;
using Template.Tools.Pool;
using TMPro;
using UnityEngine.UI;
using Template.UIBase;
using Template.Tools;
using System.Linq;

namespace GameCore.UI.GameTools
{
    public class UILevelEditor : UIElement
    {
        [SerializeField] private Transform gridParentTransform;
        [SerializeField] private UIGrid_Editable gridPrefab;
        private PoolList<UIGrid_Editable> gridLayers = new PoolList<UIGrid_Editable>();

        [SerializeField] private TileContent_Editable contentPrefab;

        [Header("Tabs")]
        [SerializeField] private List<Canvas> tabsList;

        [Header("Grid Controls")]
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private Image modeButtonImage;
        [SerializeField] private List<Sprite> buttonModeSprites;
        [SerializeField] private TextMeshProUGUI layerText;
        [SerializeField] private TMP_InputField inputFieldX;
        [SerializeField] private TMP_InputField inputFieldY;
        private int layerId = 0;
        private int gridX = 1;
        private int gridY = 1;

        [Header("Tile Controls")]
        [SerializeField] private UIToggle leftToggle;
        [SerializeField] private UIToggle rightToggle;
        [SerializeField] private UIToggle upToggle;
        [SerializeField] private UIToggle downToggle;
        [SerializeField] private TMP_InputField inputFieldTileId;
        private List<(TileData tileData, FieldData fieldData)> selectedTiles = new List<(TileData, FieldData)>();

        [Header("Save Controls")]
        [SerializeField] private UIToggle horizontalBlockToggle;
        [SerializeField] private UIToggle verticalBlockToggle;
        [SerializeField] private TMP_InputField inputFieldLevelName;
        [SerializeField] private TMP_InputField inputFieldEmptyLines;

        public event System.Action<int, int> OnChangeGridSize;
        public event System.Action<int> OnChangeLayer;
        public event System.Action<int> OnDeleteLayer;
        public event System.Action OnClearAll;
        public event System.Action<EditMode> OnChangeEditMode;

        public event System.Action<List<(TileData, FieldData)>> OnChangeTileData;

        public event System.Action<LevelData> OnSaveLevel;

        public enum EditMode
        {
            Draw,
            Select,
            Delete
        }

        private EditMode editMode = EditMode.Draw;

        private void Awake()
        {
            inputFieldX.onEndEdit.AddListener(OnChangeX);
            inputFieldY.onEndEdit.AddListener(OnChangeY);

            inputFieldTileId.onEndEdit.AddListener(ChangeTileID);

            leftToggle.onChange.AddListener(ChangeLeftOffset);
            rightToggle.onChange.AddListener(ChangeRightOffset);
            upToggle.onChange.AddListener(ChangeUpOffset);
            downToggle.onChange.AddListener(ChangeDownOffset);

            ResetTileWindow();
            ChangeEditMode();

            horizontalBlockToggle.SetBase(false);
            verticalBlockToggle.SetBase(false);

            UILoadlLevelItem.LoadEvent += OnLoadLevelHandler;
        }

        private void OnDestroy()
        {
            inputFieldX.onEndEdit.RemoveListener(OnChangeX);
            inputFieldY.onEndEdit.RemoveListener(OnChangeY);

            inputFieldTileId.onEndEdit.RemoveListener(ChangeTileID);

            leftToggle.onChange.RemoveListener(ChangeLeftOffset);
            rightToggle.onChange.RemoveListener(ChangeRightOffset);
            upToggle.onChange.RemoveListener(ChangeUpOffset);
            downToggle.onChange.RemoveListener(ChangeDownOffset);

            UILoadlLevelItem.LoadEvent -= OnLoadLevelHandler;
        }

        private void OnEnable()
        {
            //ChangeTab(0);
        }

        private void ResetTileWindow(FieldData fieldData = null)
        {
            inputFieldTileId.text = fieldData == null ? "" : fieldData.SpriteId.ToString();

            leftToggle.SetBase(fieldData == null ? false : fieldData.OffsetData.Left);
            rightToggle.SetBase(fieldData == null ? false : fieldData.OffsetData.Right);
            upToggle.SetBase(fieldData == null ? false : fieldData.OffsetData.Up);
            downToggle.SetBase(fieldData == null ? false : fieldData.OffsetData.Down);
        }

        public void SetGrid(GridData gridData, int index)
        {
            UIGrid grid;
            selectedTiles.Clear();

            if (gridLayers.Count <= index)
            {
                grid = gridLayers.Add(gridPrefab, gridParentTransform);
                grid.gameObject.SetActive(true);
                grid.transform.SetAsLastSibling();
            }
            else
            {
                grid = gridLayers[index];
            }

            grid.Resizer.Width = gridData.Width;
            grid.Resizer.Height = gridData.Height;
            grid.Resizer.Resize();

            grid.Resizer.SetConstraintWidth();

            List<TileContent> content = new List<TileContent>();

            for (int i = 0; i <= (gridData.Height * gridData.Width - gridData.Width); i += gridData.Width)
            {
                for (int j = 0; j < gridData.Width; j++)
                {
                    TileContent_Editable tile = Instantiate(contentPrefab);

                    int spriteId = gridData.Fields[i + j].SpriteId;

                    tile.Set(new TileData()
                    {
                        Id = spriteId,
                        Layer = gridData.Layer,
                        xPos = j,
                        yPos = i / gridData.Width
                    });

                    tile.FieldData = gridData.Fields[i + j];

                    tile.gameObject.SetActive(true);
                    content.Add(tile);

                    tile.SetOffset(gridData.Fields[i + j].OffsetData, grid.Resizer.TileSize.x / 2.5f, grid.Resizer.TileSize.y / 2.5f);
                }
            }

            grid.GetComponent<RectTransform>().anchoredPosition = new Vector2(grid.Resizer.TileSize.x / -12.5f, grid.Resizer.TileSize.y / 17f) * index;
            grid.ElementSelectedEvent = OnSelectTile;
            grid.ElementDeselectedEvent = OnDeselectTile;
            grid.Set(content);
        }

        public void ChangeTab(int tabID)
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            for (int i = 0; i < tabsList.Count; i++)
            {
                tabsList[i].gameObject.SetActive(i == tabID);
            }
        }

        public void ToMenu()
        {
            GameManager.Instance.StateMachine.SetState(new State_Start());
        }

        public void SaveLevel()
        {
           var saveData = new LevelData()
            {
                Name = inputFieldLevelName.text,
                HorizontalBlock = horizontalBlockToggle.Value,
                VerticalBlock = verticalBlockToggle.Value,
                emptyLinesCircle = int.Parse(inputFieldEmptyLines.text)
            };

            OnSaveLevel?.Invoke(saveData);
            ClearAll();
        }

        private void OnSelectTile(UIGridPickElement element)
        {
            var content = ((UITile)element).Content.GetComponent<TileContent_Editable>();

            switch (editMode)
            {
                case EditMode.Select:
                    Debug.LogError("OnSelectTile");
                    selectedTiles.Add((content.TileData, content.FieldData));
                    ResetTileWindow(content.FieldData);
                    break;
                case EditMode.Draw:
                    Debug.LogError("OnDrawTile");
                    content.FieldData.SpriteId = -1;
                    OnChangeTileData?.Invoke(selectedTiles);
                    break;
                case EditMode.Delete:
                    Debug.LogError("OnDeleteTile");
                    content.FieldData.SpriteId = 0;
                    if (selectedTiles.Contains((content.TileData, content.FieldData)))
                    {
                        selectedTiles.Remove((content.TileData, content.FieldData));
                    }
                    OnChangeTileData?.Invoke(selectedTiles);
                    break;
            }
        }

        private void OnDeselectTile(UIGridPickElement element)
        {
            var content = ((UITile)element).Content.GetComponent<TileContent_Editable>();

            switch (editMode)
            {
                case EditMode.Select:
                    Debug.LogError("OnDeselectTile");
                    selectedTiles.Remove((content.TileData, content.FieldData));
                    break;
            }
        }

        #region Grid

        public void ChangeEditMode()
        {
            switch (editMode)
            {
                case EditMode.Draw:
                    editMode = EditMode.Select;
                    modeButtonImage.sprite = buttonModeSprites[1];
                    break;
                case EditMode.Select:
                    editMode = EditMode.Delete;
                    selectedTiles.Clear();
                    modeButtonImage.sprite = buttonModeSprites[2];
                    break;
                case EditMode.Delete:
                    editMode = EditMode.Draw;
                    modeButtonImage.sprite = buttonModeSprites[0];
                    break;
            }

            modeText.text = editMode.ToString();
            OnChangeEditMode?.Invoke(editMode);
        }

        public void ChangeLayerValue(int addValue)
        {
            if (layerId < gridLayers.Count)
            {
                gridLayers[layerId].CancelSelection();
                gridLayers[layerId].HideHelpBorders();
            }
            selectedTiles.Clear();

            layerId += addValue;

            if (layerId < 0) layerId = 0;

            layerText.text = layerId.ToString();
            OnChangeLayer?.Invoke(layerId);

            gridX = State_Editor.editableGrid.Width;
            inputFieldX.text = gridX.ToString();
            gridY = State_Editor.editableGrid.Height;
            inputFieldY.text = gridY.ToString();

            for (int i = 0; i < gridLayers.Count; i++)
            {
                gridLayers[i].canvas.alpha = i <= layerId ? 1f : 0.4f;
                gridLayers[i].canvas.interactable = i == layerId;
                gridLayers[i].canvas.blocksRaycasts = i == layerId;
            }
        }

        public void DeleteLayer()
        {
            gridLayers.List[layerId].gameObject.SetActive(false);
            gridLayers.List[layerId].ElementSelectedEvent -= OnSelectTile;
            gridLayers.List[layerId].ElementDeselectedEvent = OnDeselectTile;
            gridLayers.List.RemoveAt(layerId);

            OnDeleteLayer?.Invoke(layerId);

            ChangeLayerValue(-1);
        }

        public void ClearAll()
        {
            layerId = 0;
            layerText.text = layerId.ToString();
            gridX = 1;
            inputFieldX.text = "1";
            gridY = 1;
            inputFieldY.text = "1";

            horizontalBlockToggle.SetBase(false);
            verticalBlockToggle.SetBase(false);
            inputFieldLevelName.text = "";
            inputFieldEmptyLines.text = "0";


            gridLayers.Clear();
            OnClearAll?.Invoke();
        }

        private void OnChangeX(string newValue)
        {
            if (int.TryParse(newValue, out int x))
            {
                if (x < 1)
                {
                    gridX = 1;
                    inputFieldX.text = "1";
                }
                else
                    gridX = x;

                OnChangeGridSize?.Invoke(gridX, gridY);
            }
        }

        private void OnChangeY(string newValue)
        {
            if (int.TryParse(newValue, out int y))
            {
                if (y < 1)
                {
                    gridY = 1;
                    inputFieldY.text = "1";
                }
                else
                    gridY = y;

                OnChangeGridSize?.Invoke(gridX, gridY);
            }
        }

        #endregion

        #region Tile

        private void ChangeLeftOffset(bool state)
        {
            foreach (var tile in selectedTiles)
            {
                tile.fieldData.OffsetData.Left = state;
            }

            OnChangeTileData?.Invoke(selectedTiles);
        }

        private void ChangeRightOffset(bool state)
        {
            foreach (var tile in selectedTiles)
            {
                tile.fieldData.OffsetData.Right = state;
            }
            OnChangeTileData?.Invoke(selectedTiles);
        }
        private void ChangeDownOffset(bool state)
        {
            foreach (var tile in selectedTiles)
            {
                tile.fieldData.OffsetData.Down = state;
            }
            OnChangeTileData?.Invoke(selectedTiles);
        }

        private void ChangeUpOffset(bool state)
        {
            foreach (var tile in selectedTiles)
            {
                tile.fieldData.OffsetData.Up = state;
            }
            OnChangeTileData?.Invoke(selectedTiles);
        }

        private void ChangeTileID(string ID)
        {
            if (int.TryParse(ID, out int id))
            {
                if (id < -1)
                {
                    inputFieldTileId.text = "-1";

                    foreach (var tile in selectedTiles)
                    {
                        tile.fieldData.SpriteId = -1;
                    }
                }
                else
                {
                    foreach (var tile in selectedTiles)
                    {
                        tile.fieldData.SpriteId = id;
                    }
                }
                OnChangeTileData?.Invoke(selectedTiles);
            }
        }

        #endregion

        public void OnLoadLevelHandler(LevelData loadData)
        {
            ClearAll();

            inputFieldLevelName.text = loadData.Name;
            inputFieldEmptyLines.text = loadData.emptyLinesCircle.ToString();

            horizontalBlockToggle.SetBase(loadData.HorizontalBlock);
            verticalBlockToggle.SetBase(loadData.VerticalBlock);

            inputFieldX.text = loadData.grids.Last().Width.ToString();
            inputFieldY.text = loadData.grids.Last().Height.ToString();

            layerId = loadData.grids.Length - 1;
            layerText.text = layerId.ToString();
        }
    }
}