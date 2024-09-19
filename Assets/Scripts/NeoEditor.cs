using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ADOFAI;
using ADOFAI.Editor.ParticleEditor;
using ADOFAI.LevelEditor.Controls;
using DG.Tweening;
using HarmonyLib;
using NeoEditor.Patches;
using NeoEditor.PopupWindows;
using NeoEditor.Tabs;
using SA.GoogleDoc;
using SFB;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeoEditor
{
    public class NeoEditor : ADOBase
    {
        public enum EditorTab
        {
            Project,
            Chart,
            Camera,
            Filter,
            Decoration,
            Effect,
            Export
        }

        public static NeoEditor Instance { get; private set; }

        public LevelData levelData => customLevel.levelData;
        public EventsArray<LevelEvent> events => levelData.levelEvents;
        public DecorationsArray<LevelEvent> decorations => levelData.decorations;
        public List<scrDecoration> allDecorations => scrDecorationManager.instance.allDecorations;
        public List<scrFloor> floors => customLevel.levelMaker.listFloors;

        public new scnGame customLevel;

        public GameObject[] tabContainers;
        public TabBase[] tabs;
        public Button[] tabButtons;
        public RawImage[] gameViews;
        public RawImage[] sceneViews;

        public Camera mainCamera;
        public Camera uiCamera;

        public GameObject popupWindows;
        public GameObject savePopupContainer;
        public GameObject missingFilesPopupContainer;
        public GameObject unsavedChangesPopupContainer;
        public GameObject confirmPopupContainer;
        public GameObject confirmPopupLargeContainer;

        public SaveBeforeImportPopup savePopup;
        public MissingFilesPopup missingFilesPopup;
        public UnsavedChangesPopup unsavedChangesPopup;
        public ConfirmPopup confirmPopup;
        public ConfirmPopup confirmPopupLarge;
        public RDColorPickerPopup colorPickerPopup;

		public Button popupBlocker;
		private List<Action> _popupStack = new List<Action>();
		private int _currentPopupSortOrder = 30000;

		[Header("Particle Editor Popup")]
		public Image particleEditorContainer;
		public ParticleEditor particleEditor;

        [Header("Prefabs")]
		public GameObject prefab_inspector;
        public GameObject prefab_eventInspector;
        public GameObject prefab_inspectorTabButton;
        public GameObject prefab_eventButton;
        public GameObject prefab_property;
        public GameObject prefab_controlBool;
        public GameObject prefab_controlText;
        public GameObject prefab_controlLongText;
        public GameObject prefab_controlColor;
        public GameObject prefab_controlBrowse;
        public GameObject prefab_controlVector2;
        public GameObject prefab_controlTile;
        public GameObject prefab_controlToggle;

        [NonSerialized]
        public Color editingColor = new Color(0.898f, 0.376f, 0.376f, 1f);

        [NonSerialized]
        public Color defaultButtonColor = Color.white;

        public Texture2D floorConnectorTex;
        private List<GameObject> floorConnectorGOs = new List<GameObject>();
        private Material lineMaterial;
        private GameObject floorConnectors;

        public float playbackSpeed;
        private float prevPlaySpeed = 1f;

        public bool showingPopup = false;

        public bool paused => scrController.instance.paused;

		public string filename { get; private set; }

		private bool shouldScrub = true;
        private int scrubTo = 0;

        public Camera BGcamCopy;
        public Camera BGcamstaticCopy;

        public float camUserSizeTarget = 1f;
        public float camUserSize = 1f;
        public float scrollSpeed;
        public Tween anchorZoomTween;
		private bool unsavedChanges;
		private readonly Color grayColor = new Color(0.42352942f, 0.42352942f, 0.42352942f);
        private readonly Color lineGreen = new Color(0.4f, 1f, 0.4f, 1f);
        private readonly Color lineYellow = new Color(1f, 1f, 0.4f, 1f);
        private readonly Color linePurple = new Color(0.75f, 0.5f, 1f, 1f);
        private readonly Color lineBlue = new Color(0.4f, 0.4f, 1f, 1f);

        public TabBase selectedTab { get; private set; }

		public EditorWebServices webServices;

		[Header("Inspector")]
		public InspectorPanel settingsPanel;

		[Header("Undo Redo")]
		[NonSerialized]
		public bool initialized;

		[NonSerialized]
		public int changingState;

		[NonSerialized]
		public List<scnEditor.LevelState> undoStates;

		[NonSerialized]
		public List<scnEditor.LevelState> redoStates;

		[NonSerialized]
		public LevelEventType settingsEventType;

		[NonSerialized]
		public LevelEventType filteredEventType;

		private int saveStateLastFrame;

		[NonSerialized]
		public PropertyControl_DecorationsList propertyControlDecorationsList;

		[NonSerialized]
		public PropertyControl_EventsList propertyControlEventsList;

		[NonSerialized]
		public RectTransform decorationsListContent;

		[NonSerialized]
		public RectTransform eventsListContent;

		public static bool selectingFloorID = false;

		private void Awake()
        {
            NeoEditor.Instance = this;
            Main.harmony.Patch(
                typeof(scnGame).GetMethod("Play", AccessTools.all),
                transpiler: typeof(SceneGamePatch.PlayWithoutCountdown).GetMethod(
                    "Transpiler",
                    AccessTools.all
				)
			);
			Main.harmony.Patch(
				typeof(scnGame).GetMethod("FinishCustomLevelLoading", AccessTools.all),
				transpiler: typeof(SceneGamePatch.FixLoadLevel).GetMethod(
					"Transpiler",
					AccessTools.all
				)
			);
			Main.harmony.Patch(
				typeof(scrController).GetMethod("Update", AccessTools.all),
				transpiler: typeof(ControllerPatch.BlockEscPause).GetMethod(
					"Transpiler",
					AccessTools.all
				)
			);
			Main.harmony.Patch(
				typeof(InspectorPanel).GetMethod("ToggleArtistPopup", AccessTools.all),
				transpiler: typeof(PropertyPatch.ArtistPopupPositionPatch).GetMethod(
					"Transpiler",
					AccessTools.all
				)
			);
			EditorPatch.ForceNeoEditor.Patcher(Main.harmony);
            LoadGameScene();
        }

        private void Start()
        {
			customLevel = scnGame.instance;
            Application.wantsToQuit += TryApplicationQuit;

            LoadLevelEventSprites();
            LoadLevelCategorySprites();
            RDString.LoadLevelEditorFonts();

            lineMaterial = new Material(Shader.Find("ADOFAI/ScrollingSprite"));
            lineMaterial.SetTexture("_MainTex", floorConnectorTex);
            lineMaterial.SetVector("_ScrollSpeed", new Vector2(-0.4f, 0f));
            lineMaterial.SetFloat("_Time0", 0f);

            scrCamera camera = scrCamera.instance;
            if (camera.Field("camRT") == null)
            {
                camera.camobj.targetTexture = Assets.GameRenderer;
                camera.BGcam.targetTexture = Assets.GameRenderer;
                camera.Bgcamstatic.targetTexture = Assets.GameRenderer;
                camera.Overlaycam.targetTexture = Assets.GameRenderer;
            }
            else
            {
                RenderTexture oldTexture = Assets.GameRenderer;
                oldTexture?.Release();
                camera.Method("SetupRTCam").Invoke(camera, new object[] { true });
            }

            mainCamera.targetTexture = Assets.SceneRenderer;
            mainCamera.cullingMask = camera.camobj.cullingMask;
            mainCamera.cullingMask |= 1 << LayerMask.NameToLayer("Foreground");
            mainCamera.cullingMask |= 1 << 26;

            BGcamCopy = Instantiate(camera.BGcam, mainCamera.transform);
            BGcamstaticCopy = Instantiate(camera.Bgcamstatic, mainCamera.transform);
            BGcamCopy.targetTexture = Assets.SceneRenderer;
            BGcamstaticCopy.targetTexture = Assets.SceneRenderer;
            BGcamCopy.GetComponent<scrMatchCameraSize>().enabled = false;

            foreach (var gameView in gameViews)
                gameView.texture = Assets.GameRenderer;
            foreach (var sceneView in sceneViews)
                sceneView.texture = Assets.SceneRenderer;

            scrUIController uIController = scrUIController.instance;
            uiController.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiController.canvas.worldCamera = uiCamera;

            //floorConnectors = GameObject.Find("Floor Connector Lines");
            floorConnectors = new GameObject("Floor Connector Lines");

			webServices.LoadAllArtists(null);

			var dictionary = GCS
                .settingsInfo.Concat(GCS.levelEventsInfo)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            SelectTab(EditorTab.Project);

            customLevel.RemakePath();
            customLevel.ResetScene();

            FloorMesh.UpdateAllRequired();

            // Setup particle editor.
            particleEditor.previewDec = 
                GameObject.Instantiate(scrDecorationManager.instance.prefab_particleDecoration, particleEditor.transform.Find("Preview"))
                .GetComponent<scrParticleDecoration>();

            for (int i = 0; i < 7; i++)
            {
                int tab = i;
                tabs[i].InitTab(dictionary);
                tabButtons[i].onClick.AddListener(() => SelectTab((EditorTab)tab));
            }

            //OpenLevel();

            scrController.instance.paused = true;
            GameObject.Find("Error Meter(Clone)")?.SetActive(false);
			//customLevel.Play(0, false);

			void LoadLevelEventSprites()
			{
				if (GCS.levelEventIcons != null)
				{
					return;
				}
				GCS.levelEventIcons = new Dictionary<LevelEventType, Sprite>();
				foreach (object obj in Enum.GetValues(typeof(LevelEventType)))
				{
					Sprite sprite = Resources.Load<Sprite>("LevelEditor/LevelEvents/" + ((obj != null) ? obj.ToString() : null));
					if (sprite != null)
					{
						GCS.levelEventIcons.Add((LevelEventType)obj, sprite);
					}
				}
			}

			void LoadLevelCategorySprites()
			{
				if (GCS.eventCategoryIcons != null)
				{
					return;
				}
				GCS.eventCategoryIcons = new Dictionary<ADOFAI.LevelEventCategory, Sprite>();
				foreach (object obj in Enum.GetValues(typeof(ADOFAI.LevelEventCategory)))
				{
					Sprite sprite = Resources.Load<Sprite>("LevelEditor/EventCategories/" + ((obj != null) ? obj.ToString() : null));
					if (sprite != null)
					{
						GCS.eventCategoryIcons.Add((ADOFAI.LevelEventCategory)obj, sprite);
					}
				}
			}
		}

        private void Update() { }

        private void LateUpdate() { }

        private void OnDestroy()
        {
            Main.harmony.Unpatch(
                typeof(scnGame).GetMethod("Play", AccessTools.all),
                HarmonyPatchType.Transpiler,
                Main.Entry.Info.Id
            );
            Main.harmony.Unpatch(
                typeof(scnGame).GetMethod("FinishCustomLevelLoading", AccessTools.all),
                HarmonyPatchType.Transpiler,
                Main.Entry.Info.Id
            );
            Main.harmony.Unpatch(
                typeof(scrController).GetMethod("Update", AccessTools.all),
                HarmonyPatchType.Transpiler,
                Main.Entry.Info.Id
            );
            Main.harmony.Unpatch(
                typeof(InspectorPanel).GetMethod("ToggleArtistPopup", AccessTools.all),
                HarmonyPatchType.Transpiler,
                Main.Entry.Info.Id
            );
			EditorPatch.ForceNeoEditor.Unpatcher(Main.harmony);

			ADOStartup.SetupLevelEventsInfo();
        }

        private void LoadGameScene()
        {
            if (scnGame.instance != null)
            {
                //this.fromScnGame = true;
                return;
            }
            SceneManager.LoadScene("scnGame", LoadSceneMode.Additive);
        }

        private bool TryApplicationQuit()
        {
            //if (this.unsavedChanges && !this.forceQuit)
            //{
            //	if (this.playMode)
            //	{
            //		this.TogglePause(false);
            //	}
            //	this.CheckUnsavedChanges(delegate
            //	{
            //		this.ApplicationQuit();
            //	}, false);
            //	return false;
            //}
            return true;
        }

        public void SelectTab(EditorTab tab)
        {
            selectedTab = tabs[(int)tab];
            for (int i = 0; i < 7; i++)
            {
                tabContainers[i].SetActive(i == (int)tab);
                tabButtons[i].interactable = i != (int)tab;
                if (i == (int)tab)
                    tabs[i].OnActive();
                else
                    tabs[i].OnInactive();
            }
        }

        public void TogglePauseGame()
        {
            scrController.instance.TogglePauseGame();
            GameObject.Find("Error Meter(Clone)")?.SetActive(false);
        }

        public void PlayPause()
        {
            if (paused)
            {
                if (prevPlaySpeed != (RDInput.holdingControl ? playbackSpeed : 1f))
                    ScrubTo(scrController.instance.currentSeqID);
                Play();
            }
            else
                Pause();
        }

        public void Play()
        {
            if (!paused && !shouldScrub)
                return;

            RDC.auto = true;

            if (shouldScrub)
            {
                customLevel.ResetScene(true);
                customLevel.isLoading = false;
                customLevel.UpdateDecorationObjects(true);
                GCS.useUnlockKeyLimiter = true;
                customLevel.Play(scrubTo, false);
                GCS.useUnlockKeyLimiter = false;
                prevPlaySpeed = RDInput.holdingControl ? playbackSpeed : 1f;
                shouldScrub = false;
            }
            else
            {
                TogglePauseGame();
            }

            GameObject.Find("Error Meter(Clone)")?.SetActive(false);

            foreach (var tab in tabs)
                tab.OnPlayLevel();
        }

        public void Pause()
        {
            if (paused)
                return;
            TogglePauseGame();
        }

        public void Rewind()
        {
            ScrubTo(0);
            Pause();
            customLevel.ResetScene(true);
            RemakePath(true, true);
            //this.lastSelectedFloor = null;
            //this.SelectFirstFloor();
            Vector3 targetPos = floors[0].transform.position.WithZ(-10f);
            scrCamera.instance.transform.position = targetPos;
            //this.UpdateSongAndLevelSettings();
            customLevel.ReloadAssets(true, true);

            //UpdateDecorationObjects();
            customLevel.UpdateDecorationObjects(true);
        }

        public void Skip(int skip)
        {
            SkipTo(Math.Clamp(scrController.instance.currentSeqID + skip, 0, floors.Count - 1));
        }

        public void SkipTo(int seqID)
        {
            ScrubTo(Math.Clamp(seqID, 0, floors.Count - 1));

            if (paused)
            {
                customLevel.ResetScene(true);
                RemakePath(true, true);
                Vector3 targetPos = floors[scrubTo].transform.position.WithZ(-10f);
                scrCamera.instance.transform.position = targetPos;
                customLevel.ReloadAssets(true, true);
                scrController.instance.currentSeqID = scrubTo;
            }
            else
            {
                Pause();
                Play();
            }
        }

        public void SetPlaybackSpeed(int speed)
        {
            playbackSpeed = speed / 100f;
        }

        public void ScrubTo(int seqID)
        {
            shouldScrub = true;
            scrubTo = seqID;
            if (paused)
                scrController.instance.currentSeqID = scrubTo;
        }

        public void ShowPopup(bool show, string type)
        {
            if (showingPopup && show)
            {
                return;
            }
            showingPopup = show;

            if (show)
            {
                foreach (Transform obj in popupWindows.transform)
                {
                    obj.gameObject.SetActive(false);
                }
                switch (type)
                {
                    case "SaveBeforeSongImport":
                    case "SaveBeforeImageImport":
                    case "SaveBeforeVideoImport":
                    case "SaveBeforeLevelExport":
                        string key = "";
                        switch (type)
                        {
                            case "SaveBeforeSongImport":
                                key = "editor.dialog.saveBeforeImportingSounds";
                                break;
                            case "SaveBeforeImageImport":
                                key = "editor.dialog.saveBeforeImportingImages";
                                break;
                            case "SaveBeforeVideoImport":
                                key = "editor.dialog.saveBeforeImportingVideos";
                                break;
                            case "SaveBeforeLevelExport":
                                key = "editor.dialog.saveBeforeLevelExport";
                                break;
                        }
                        string text = RDString.Get(key, null, LangSection.Translations);
                        savePopupContainer.SetActive(true);
                        savePopup.content.text = text;
                        break;
                    case "MissingFiles":
                        missingFilesPopupContainer.SetActive(true);
                        //List<string> missingFiles = GetMissingFiles();
                        StringBuilder stringBuilder = new StringBuilder();
                        //foreach (string value in missingFiles)
                        //{
                        //	stringBuilder.Append("- ").Append(value).Append('\n');
                        //}
                        missingFilesPopup.missingFiles.text = stringBuilder.ToString();
                        break;
                    case "UnsavedChanges":
                        unsavedChangesPopupContainer.SetActive(true);
                        break;
                }
            }
        }

		public int PushPopupBlocker(Action onClickAction)
		{
			int result = _currentPopupSortOrder + 1;
			popupBlocker.gameObject.SetActive(true);
			Button component = popupBlocker.GetComponent<Button>();
			component.onClick.RemoveAllListeners();
			component.onClick.AddListener(new UnityAction(PopPopupBlocker));
			popupBlocker.GetComponent<Canvas>().sortingOrder = _currentPopupSortOrder;
			_popupStack.Add(onClickAction);
			_currentPopupSortOrder += 2;
			return result;
		}

		public void ClearPopupBlocker()
		{
			while (_popupStack.Count > 0)
			{
				PopPopupBlocker();
			}
		}

		public void PopPopupBlocker()
		{
			if (_popupStack.Count == 0)
			{
				return;
			}
			List<Action> popupStack = _popupStack;
			Action action = popupStack[popupStack.Count - 1];
			_popupStack.Remove(action);
			action();
			if (_popupStack.Count == 0)
			{
				popupBlocker.gameObject.SetActive(false);
			}
			_currentPopupSortOrder -= 2;
			popupBlocker.GetComponent<Canvas>().sortingOrder = _currentPopupSortOrder - 2;
		}

		public void ShowNotificationPopupBase(
            ConfirmPopup popup,
            string text,
            string title = null,
            Action callbackAction = null
        )
        {
            if (showingPopup)
            {
                return;
            }
            showingPopup = true;

            popup.title.text = title;
            popup.content.text = text;

            popup.okButton.onClick.AddListener(
                delegate()
                {
                    Action callbackAction2 = callbackAction;
                    if (callbackAction2 != null)
                    {
                        callbackAction2();
                    }
                    popup.okButton.onClick.RemoveAllListeners();
                    popup.ClosePopup();
                }
            );
            popup.gameObject.SetActive(true);
        }

        public void ShowNotificationPopup(
            string text,
            string title = null,
            Action callbackAction = null
        )
        {
            ShowNotificationPopupBase(confirmPopup, text, title, callbackAction);
        }

        public void ShowNotificationPopupLarge(
            string text,
            string title = null,
            Action callbackAction = null
        )
        {
            ShowNotificationPopupBase(confirmPopupLarge, text, title, callbackAction);
        }

        public void OpenLevel()
        {
            StartCoroutine(OpenLevelCo());

            DrawFloorOffsetLines();
        }

		public void SaveLevel()
		{
			if (!string.IsNullOrEmpty(ADOBase.levelPath))
			{
				try
				{
					string data = levelData.Encode();
					RDFile.WriteAllText(ADOBase.levelPath, data);
					//ShowNotification(RDString.Get("editor.notification.levelSaved"));
					unsavedChanges = false;
				}
				catch (Exception ex)
				{
					ShowNotificationPopup(RDString.Get("editor.notification.savingFailed"));
					Debug.Log("Failed saving at path " + ADOBase.levelPath + ": " + ex.Message);
					return;
				}
			}
			else
			{
				SaveLevelAs();
			}
		}

		public void SaveLevelAs(bool newLevel = false)
		{
			StartCoroutine(SaveLevelAsCo(newLevel));
		}

		public scrFloor NextFloor(scrFloor floor)
        {
            List<scrFloor> floors = this.floors;
            int num = floors.IndexOf(floor) + 1;
            if (num >= floors.Count)
            {
                return null;
            }
            return floors[num];
        }

        public scrFloor PreviousFloor(scrFloor floor)
        {
            List<scrFloor> floors = this.floors;
            int num = floors.IndexOf(floor) - 1;
            if (num < 0)
            {
                return null;
            }
            return floors[num];
        }

        public void RemakePath(bool applyEventsToFloors = true, bool remakeLevel = true)
        {
            customLevel.RemakePath(applyEventsToFloors, remakeLevel);
            DrawFloorOffsetLines();
            DrawHolds(!remakeLevel);
            DrawFloorNums();
            DrawMultiPlanet();
        }

		public void ApplyEventsToFloors()
		{
			customLevel.ApplyEventsToFloors(this.floors);
			DrawFloorOffsetLines();
			DrawHolds(false);
			DrawMultiPlanet();
			//refreshDecSprites = true;
		}

		public void UpdateDecorationObject(LevelEvent e)
        {
            if (e.isFake)
            {
                e.ApplyPropertiesToRealEvents();
                return;
            }
            scrDecoration scrDecoration = allDecorations.Find(
                (scrDecoration d) => d.sourceLevelEvent == e
            );
            if (scrDecoration != null)
            {
                bool flag;
                scrDecoration.Setup(e, out flag);
                scrDecoration.UpdateHitbox();
            }
        }

        private string SanitizeLevelPath(string path)
        {
            return Uri.UnescapeDataString(path.Replace("file:", ""));
        }

        private void ClearAllFloorOffsets()
        {
            foreach (GameObject obj in floorConnectorGOs)
            {
                Destroy(obj);
            }
            floorConnectorGOs.Clear();
        }

        private void DrawFloorOffsetLines()
        {
            foreach (GameObject obj in floorConnectorGOs)
            {
                Destroy(obj);
            }
            floorConnectorGOs.Clear();
            int num = -2;
            Vector3 vector = Vector3.zero;
            foreach (LevelEvent levelEvent in events)
            {
                if (levelEvent.eventType == LevelEventType.PositionTrack && levelEvent.floor > 0)
                {
                    int floor = levelEvent.floor;
                    if (
                        (
                            !(floors[floor].prevfloor != null)
                            || floors[floor].prevfloor.holdLength <= -1
                        )
                        && (
                            !levelEvent.data.Keys.Contains("justThisTile")
                            || !levelEvent.GetBool("justThisTile")
                        )
                    )
                    {
                        if (floor != num)
                        {
                            vector = new Vector2(0f, 0f);
                        }
                        Vector3 vector2 =
                            (Vector2)levelEvent.data["positionOffset"] * customLevel.GetTileSize();
                        Vector2 vector3 = new Vector2(
                            -0.75f * Mathf.Cos((float)floors[floor - 1].exitangle + 1.5707964f),
                            0.75f * Mathf.Sin((float)floors[floor - 1].exitangle + 1.5707964f)
                        );
                        Vector3 vector4 = new Vector3(
                            vector.x + floors[floor - 1].transform.position.x + vector3.x,
                            vector.y + floors[floor - 1].transform.position.y + vector3.y,
                            floors[floor - 1].transform.position.z
                        );
                        Vector3 vector5 = new Vector3(
                            vector4.x + vector2.x,
                            vector4.y + vector2.y,
                            floors[floor].transform.position.z
                        );
                        if (Vector3.Distance(vector4, vector5) >= 0.05f)
                        {
                            GameObject gameObject = new GameObject();
                            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
                            lineRenderer.positionCount = 2;
                            lineRenderer.material = lineMaterial;
                            lineRenderer.textureMode = LineTextureMode.Tile;
                            if (levelEvent.GetBool("editorOnly"))
                            {
                                lineRenderer.startColor = lineGreen;
                                lineRenderer.endColor = lineYellow;
                            }
                            else
                            {
                                lineRenderer.startColor = linePurple;
                                lineRenderer.endColor = lineBlue;
                            }
                            lineRenderer.SetPosition(0, vector4);
                            lineRenderer.SetPosition(1, vector5);
                            lineRenderer.startWidth = 0.1f;
                            lineRenderer.endWidth = 0.1f;
                            lineRenderer.name = "Floor connector";
                            lineRenderer.transform.parent = floorConnectors.transform;
                            floorConnectorGOs.Add(gameObject);
                            Vector2 vector6 =
                                (Vector2)levelEvent.data["positionOffset"]
                                * customLevel.GetTileSize();
                            vector += new Vector3(vector6.x, vector6.y, 0f);
                            num = floor;
                        }
                    }
                }
            }
        }

        private void DrawFloorNums()
        {
            foreach (scrFloor scrFloor in floors)
            {
                if (scrFloor.enabled)
                {
                    scrFloor.editorNumText.gameObject.SetActive(false && !scrFloor.isFake);
                }
            }
        }

        private void DrawHolds(bool unfillHolds = false)
        {
            customLevel.levelMaker.DrawHolds(unfillHolds);
        }

        private void DrawMultiPlanet()
        {
            customLevel.levelMaker.DrawMultiPlanet(false);
        }

        private string FindAdofaiLevelOnDirectory(string path)
        {
            string[] files = Directory.GetFiles(path, "*.adofai", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                return null;
            }
            string text = null;
            for (int i = 0; i < files.Length; i++)
            {
                if (
                    !(Path.GetFileName(files[i]) == "backup.adofai")
                    && !Path.GetFileName(files[i]).StartsWith(".")
                )
                {
                    text = files[i];
                    MonoBehaviour.print("selected file: " + text);
                    break;
                }
            }
            if (text == null)
            {
                MonoBehaviour.print("was null");
                return null;
            }
            return text;
        }

        public void UpdateSongAndLevelSettings()
        {
            //var dictionary = GCS
            //    .settingsInfo.Concat(GCS.levelEventsInfo)
            //    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            //for (int i = 0; i < 7; i++)
            //    tabs[i].InitTab(dictionary);
        }

		private void RefreshFilenameText()
		{
			string text = (string.IsNullOrEmpty(ADOBase.levelPath) ? RDString.Get("editor.levelNotSaved") : Path.GetFileName(ADOBase.levelPath));
			if (unsavedChanges)
			{
				text += "*";
			}

			filename = text;
		}

        public LevelEvent AddEvent(int floor, LevelEventType type)
        {
            LevelEvent levelEvent = new LevelEvent(floor, type);
			events.Add(levelEvent);

            return levelEvent;
		}

		private IEnumerator OpenLevelCo(string definedLevelPath = null)
        {
            Instance.Pause();
            ClearAllFloorOffsets();
            //this.redoStates.Clear();
            //this.undoStates.Clear();
            bool flag = definedLevelPath == null;
            string lastLevelPath = customLevel.levelPath;
            if (flag)
            {
                string[] levelPaths = StandaloneFileBrowser.OpenFilePanel(
                    RDString.Get("editor.dialog.openFile", null, LangSection.Translations),
                    Persistence.GetLastUsedFolder(),
                    new ExtensionFilter[]
                    {
                        new ExtensionFilter(
                            RDString.Get(
                                "editor.dialog.adofaiLevelDescription",
                                null,
                                LangSection.Translations
                            ),
                            GCS.levelExtensions
                        )
                    },
                    false
                );
                yield return null;
                if (levelPaths.Length == 0 || string.IsNullOrEmpty(levelPaths[0]))
                {
                    yield break;
                }
                string text = SanitizeLevelPath(levelPaths[0]);
                string text2 = Path.GetExtension(text).ToLower();
                string value = text2.Substring(1, text2.Length - 1);
                if (GCS.levelZipExtensions.Contains(value))
                {
                    string availableDirectoryName = RDUtils.GetAvailableDirectoryName(
                        Path.Combine(
                            Path.GetDirectoryName(text),
                            Path.GetFileNameWithoutExtension(text)
                        )
                    );
                    RDDirectory.CreateDirectory(availableDirectoryName);
                    try
                    {
                        ZipUtils.Unzip(text, availableDirectoryName);
                    }
                    catch (Exception ex)
                    {
                        ShowNotificationPopup(
                            RDString.Get(
                                "editor.notification.unzipFailed",
                                null,
                                LangSection.Translations
                            ),
                            null,
                            null
                        );
                        string str = "Unzip failed: ";
                        Exception ex2 = ex;
                        Debug.LogError(str + ((ex2 != null) ? ex2.ToString() : null));
                        Directory.Delete(availableDirectoryName, true);
                        yield break;
                    }
                    string text3 = FindAdofaiLevelOnDirectory(availableDirectoryName);
                    if (text3 == null)
                    {
                        ShowNotificationPopup(
                            RDString.Get(
                                "editor.notification.levelNotFound",
                                null,
                                LangSection.Translations
                            ),
                            null,
                            null
                        );
                        Directory.Delete(availableDirectoryName, true);
                        yield break;
                    }
                    customLevel.levelPath = text3;
                }
                else
                {
                    customLevel.levelPath = text;
                }
                levelPaths = null;
            }
            else
            {
                customLevel.levelPath = definedLevelPath;
            }
            scrController.deaths = 0;
            string customLevelId = GCS.customLevelId;
            GCS.customLevelId = null;
            Persistence.UpdateLastUsedFolder(ADOBase.levelPath);
            Persistence.UpdateLastOpenedLevel(ADOBase.levelPath);
            bool flag2 = false;
            LoadResult loadResult = LoadResult.Error;
            string text4 = "";
            //this.isLoading = true;
            try
            {
                flag2 = customLevel.LoadLevel(ADOBase.levelPath, out loadResult);
            }
            catch (Exception ex3)
            {
                text4 = string.Concat(
                    new string[]
                    {
                        "Error loading level file at ",
                        ADOBase.levelPath,
                        ": ",
                        ex3.Message,
                        ", Stacktrace:\n",
                        ex3.StackTrace
                    }
                );
                Debug.Log(text4);
            }
            if (flag2)
            {
                Instance.Rewind();
                //this.errorImageResult.Clear();
                //this.isUnauthorizedAccess = false;
                RemakePath(true, true);
                //this.lastSelectedFloor = null;
                //this.SelectFirstFloor();
                UpdateSongAndLevelSettings();
                customLevel.ReloadAssets(true, true);

                //this.UpdateDecorationObjects();
                customLevel.UpdateDecorationObjects(true);

                //this.ShowNotification(RDString.Get("editor.notification.levelLoaded", null, LangSection.Translations), null, 1.25f);
                //this.unsavedChanges = false;
            }
            else
            {
                customLevel.levelPath = lastLevelPath;
                GCS.customLevelId = customLevelId;
                ShowNotificationPopupLarge(
                    text4,
                    RDString.Get(
                        string.Format("editor.notification.loadingFailed.{0}", loadResult),
                        null,
                        LangSection.Translations
                    )
                );
            }
            //this.isLoading = false;
            //this.CloseAllPanels(null);
            //yield return null;
            //this.ShowImageLoadResult();

            foreach (var tab in tabs)
                tab.OnOpenLevel();
            yield break;
        }

		private IEnumerator SaveLevelAsCo(bool newLevel = false)
		{
			string defaultName = ((newLevel || string.IsNullOrEmpty(customLevel.levelPath)) ? "level" : Path.GetFileNameWithoutExtension(customLevel.levelPath));
			StandaloneFileBrowser.SaveFilePanelAsync(RDString.Get("editor.dialog.saveLevel"), Persistence.GetLastUsedFolder(), defaultName, "adofai", delegate (string levelPath)
			{
				if (!string.IsNullOrEmpty(levelPath))
				{
					string text = SanitizeLevelPath(levelPath);
					if (!text.EndsWith(".adofai"))
					{
						text += ".adofai";
					}

					customLevel.levelPath = text;
					RefreshFilenameText();
					Persistence.UpdateLastUsedFolder(levelPath);
					Persistence.UpdateLastOpenedLevel(levelPath);
					DiscordController.instance?.UpdatePresence();
					SaveLevel();
				}
			});
            yield break;
		}

		public void Undo()
		{
			this.UndoOrRedo(false);
		}

		public void Redo()
		{
			this.UndoOrRedo(true);
		}

		public void UndoOrRedo(bool redo)
		{
			//Debug.Log("UndoOrRedo - I was called");
			//if (this.changingState != 0)
			//{
			//	return;
			//}
			//List<scnEditor.LevelState> list = redo ? this.redoStates : this.undoStates;
			//if (list.Count > 0)
			//{
			//	bool dataHasChanged = list.Count > 0 && list.Last<scnEditor.LevelState>().data != null;
			//	using (new SaveStateScope(this, false, dataHasChanged, false))
			//	{
			//		if (!redo)
			//		{
			//			this.redoStates.Add(this.undoStates.Pop<scnEditor.LevelState>());
			//		}
			//		scnEditor.LevelState levelState = list.Last<scnEditor.LevelState>();
			//		int[] selectedDecorationIndices = levelState.selectedDecorationIndices;
			//		if (levelState.data != null)
			//		{
			//			this.customLevel.levelData = levelState.data;
			//		}
			//		this.DeselectFloors(false);
			//		this.RemakePath(true, true);
			//		this.DeselectAllDecorations();
			//		this.UpdateDecorationObjects();
			//		foreach (int num in selectedDecorationIndices)
			//		{
			//			if (this.customLevel.levelData.decorations.Count > num)
			//			{
			//				LevelEvent levelEvent = this.customLevel.levelData.decorations[num];
			//				this.SelectDecoration(levelEvent, false, false, true, false);
			//			}
			//		}
			//		if (!this.SelectionDecorationIsEmpty())
			//		{
			//			LevelEvent levelEvent2 = this.selectedDecorations[this.selectedDecorations.Count - 1];
			//			this.levelEventsPanel.ShowInspector(true, true);
			//			this.levelEventsPanel.ShowPanel(levelEvent2.eventType, 0);
			//		}
			//		this.propertyControlDecorationsList.RefreshItemsList(true);
			//		List<int> list2 = levelState.selectedFloors;
			//		if (list2.Count > 1)
			//		{
			//			this.MultiSelectFloors(this.floors[list2[0]], this.floors[list2[list2.Count - 1]], false);
			//		}
			//		else if (list2.Count == 1)
			//		{
			//			int index = list2[0];
			//			this.SelectFloor(this.floors[index], true);
			//			this.levelEventsPanel.ShowPanel(levelState.floorEventType, levelState.floorEventTypeIndex);
			//		}
			//		this.settingsPanel.ShowPanel(levelState.settingsEventType, 0);
			//		if (this.particleEditor.gameObject.activeSelf && this.particleEditor.SelectedEvent != null)
			//		{
			//			if (this.selectedDecorations.Count == 0)
			//			{
			//				this.HideParticleEditor();
			//			}
			//			else
			//			{
			//				ParticleEditor particleEditor = this.particleEditor;
			//				List<LevelEvent> list3 = this.selectedDecorations;
			//				particleEditor.SetEvent(list3[list3.Count - 1]);
			//			}
			//		}
			//		list.RemoveAt(list.Count - 1);
			//	}
			//}
		}

		public void SaveState(bool clearRedo = true, bool dataHasChanged = true)
		{
			//if (this.changingState != 0 || !this.initialized)
			//{
			//	return;
			//}
			//List<int> list = new List<int>();
			//if (!this.SelectionIsEmpty())
			//{
			//	if (this.SelectionIsSingle())
			//	{
			//		list.Add(this.selectedFloors[0].seqID);
			//	}
			//	else
			//	{
			//		foreach (scrFloor scrFloor in this.selectedFloors)
			//		{
			//			list.Add(scrFloor.seqID);
			//		}
			//	}
			//}
			//LevelData data = this.levelData.Copy();
			//int[] array = new int[this.selectedDecorations.Count];
			//int num = 0;
			//foreach (LevelEvent dec in this.selectedDecorations)
			//{
			//	array[num] = scrDecorationManager.GetDecorationIndex(dec);
			//	num++;
			//}
			//scnEditor.LevelState levelState = new scnEditor.LevelState(data, list, array, dataHasChanged);
			//levelState.settingsEventType = this.settingsPanel.selectedEventType;
			//levelState.floorEventType = this.levelEventsPanel.selectedEventType;
			//levelState.floorEventTypeIndex = this.levelEventsPanel.EventNumOfTab(levelState.floorEventType);
			//this.undoStates.Add(levelState);
			//if (clearRedo)
			//{
			//	this.redoStates.Clear();
			//}
			//if (this.undoStates.Count > 100)
			//{
			//	this.undoStates.RemoveAt(0);
			//}
			//if (dataHasChanged)
			//{
			//	this.unsavedChanges = true;
			//}
			//this.saveStateLastFrame = Time.frameCount;
		}

		public void ShowExportWindow(int state)
        {

        }

		public void ShowParticleEditor(LevelEvent targetEvent)
		{
			particleEditor.SetEvent(targetEvent);
			particleEditorContainer.gameObject.SetActive(true);
		}

		public void HideParticleEditor()
		{
            particleEditorContainer.gameObject.SetActive(false);
		}
	}
}
