using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ADOFAI;
using ADOFAI.Editor;
using ADOFAI.Editor.ParticleEditor;
using ADOFAI.LevelEditor.Controls;
using DG.Tweening;
using HarmonyLib;
using NeoEditor.Inspector.Media;
using NeoEditor.Inspector;
using NeoEditor.Patches;
using NeoEditor.PopupWindows;
using SA.GoogleDoc;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NeoEditor.Inspector.Timeline;
using DynamicPanels;

namespace NeoEditor
{
    public class NeoEditor : ADOBase
    {
        public static NeoEditor Instance { get; private set; }

        public LevelData levelData => customLevel.levelData;
        public EventsArray<LevelEvent> events => levelData.levelEvents;
        public DecorationsArray<LevelEvent> decorations => levelData.decorations;
        public List<scrDecoration> allDecorations => scrDecorationManager.instance.allDecorations;
        public List<scrFloor> floors => customLevel.levelMaker.listFloors;

        public Canvas levelEditorCanvas;
		public new scnGame customLevel;

        public RawImage gameView;
        public RawImage sceneView;

        public Camera mainCamera;
        public Camera uiCamera;

		public EventSystem eventSystem;

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
        public GameObject prefab_controlDecorationsList;

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

		public TMP_Text filenameText;

		private bool shouldScrub = true;
        private int scrubTo = 0;

        public Camera BGcamCopy;
        public Camera BGcamstaticCopy;

        public float camUserSizeTarget = 1f;
        public float camUserSize = 1f;
        public float scrollSpeed;
        public Tween anchorZoomTween;

		private bool unsavedChanges
        {
            get
            {
                return _unsavedChanges;
            }
            set
            {
                _unsavedChanges = value;
                RefreshFilenameText();
            }
        }
        private bool _unsavedChanges;
		
        private readonly Color grayColor = new Color(0.42352942f, 0.42352942f, 0.42352942f);
        private readonly Color lineGreen = new Color(0.4f, 1f, 0.4f, 1f);
        private readonly Color lineYellow = new Color(1f, 1f, 0.4f, 1f);
        private readonly Color linePurple = new Color(0.75f, 0.5f, 1f, 1f);
        private readonly Color lineBlue = new Color(0.4f, 0.4f, 1f, 1f);

		public EditorWebServices webServices;

		[Header("Inspectors")]
		public InspectorPanel settingsPanel;
        public InspectorPanel levelEventsPanel;

        [Header("Panels")]
		public ProjectPanel projectPanel;
		public MediaPanel mediaPanel;
		public EventPanel eventsPanel;
		public TimelinePanel timelinePanel;
		public DecorationPanel decorationsPanel;

        [Header("DynamicPanels")]
		public DynamicPanelsCanvas panelCanvas;
		public RectTransform gameViewPanelContent;
		public RectTransform sceneViewPanelContent;
		public RectTransform inspectorPanelContent;
		public RectTransform projectPanelContent;
		public RectTransform decorationsPanelContent;
		public RectTransform mediaPanelContent;
		public RectTransform timelinePanelContent;
        public RectTransform controlsPanelContent;
		public Dictionary<string, Panel> panels;
		public Dictionary<string, PanelTab> panelTabs;

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

		private bool refreshBgSprites;
		private bool refreshDecSprites;

		[NonSerialized]
		public RectTransform decorationsListContent;

		[NonSerialized]
		public RectTransform eventsListContent;

		public static bool selectingFloorID = false;

        [NonSerialized]
        public List<scrFloor> selectedFloors = new List<scrFloor>();

		[NonSerialized]
		public int selectedFloorCached;

		[NonSerialized]
		public List<LevelEvent> selectedDecorations = new List<LevelEvent>();

		[NonSerialized]
		public int cacheSelectedEventIndex;

		public DecorationPivot decPivot;
		public TransformGizmoHolder decTransformGizmo;

		[NonSerialized]
		public new Camera camera;
		public float cameraSelectDuration;

		public bool userIsEditingAnInputField
        {
            get
            {
                GameObject currentSelectedGameObject = eventSystem.currentSelectedGameObject;
                if (currentSelectedGameObject == null)
                {
                    return false;
                }
                TMP_InputField component = currentSelectedGameObject.GetComponent<TMP_InputField>();
                return component != null && component.isFocused;
            }
        }

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
            eventSystem = EventSystem.current;
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

			this.camera = camera.camobj;

			mainCamera.targetTexture = Assets.SceneRenderer;
            mainCamera.cullingMask = camera.camobj.cullingMask;
            mainCamera.cullingMask |= 1 << LayerMask.NameToLayer("Foreground");
            mainCamera.cullingMask |= 1 << 26;

            BGcamCopy = Instantiate(camera.BGcam, mainCamera.transform);
            BGcamstaticCopy = Instantiate(camera.Bgcamstatic, mainCamera.transform);
            BGcamCopy.targetTexture = Assets.SceneRenderer;
            BGcamstaticCopy.targetTexture = Assets.SceneRenderer;
            BGcamCopy.GetComponent<scrMatchCameraSize>().enabled = false;

			gameView.texture = Assets.GameRenderer;
			sceneView.texture = Assets.SceneRenderer;

            scrUIController uIController = scrUIController.instance;
            uiController.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiController.canvas.worldCamera = uiCamera;

            //floorConnectors = GameObject.Find("Floor Connector Lines");
            floorConnectors = new GameObject("Floor Connector Lines");

            RefreshFilenameText();
			webServices.LoadAllArtists(null);

			var dictionary = GCS
                .settingsInfo.Concat(GCS.levelEventsInfo)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			InitializeLayout();

			customLevel.RemakePath();
            customLevel.ResetScene();

            FloorMesh.UpdateAllRequired();

            // Setup particle editor.
            particleEditor.previewDec = 
                GameObject.Instantiate(scrDecorationManager.instance.prefab_particleDecoration, particleEditor.transform.Find("Preview"))
                .GetComponent<scrParticleDecoration>();

            Initialize(dictionary);

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

        private void Update()
        {
			if (refreshBgSprites)
			{
				UpdateBackgroundSprites();
			}
			if (refreshDecSprites)
			{
				UpdateDecorationObjects();
			}
		}

        private void LateUpdate()
        {
			FloorMesh.UpdateAllRequired();
		}

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

        private void Initialize(Dictionary<string, LevelEventInfo> levelEventsInfo)
        {
			projectPanel.Init(
				levelEventsInfo["SongSettings"],
				levelEventsInfo["LevelSettings"],
				levelEventsInfo["MiscSettings"]
			);

            mediaPanel.Init();

			decorationsPanel.Init(levelEventsInfo["DecorationSettings"]);

			eventsPanel.Init(levelEventsInfo.Values.Where(info => !info.type.IsSetting()).ToList());

			OnOpenLevel(true);
		}

        private void InitializeLayout()
        {
            panels = new Dictionary<string, Panel>();
            panelTabs = new Dictionary<string, PanelTab>();

			panels.Add("Game", PanelUtils.CreatePanelFor(gameViewPanelContent, panelCanvas));
            PanelTab gameViewTab = panels["Game"][0];
            gameViewTab.Icon = null;
            gameViewTab.Label = "Preview";
            gameViewTab.MinSize = new Vector2(320, 180);
            gameViewTab.ID = "Preview";
            panelTabs.Add("Game", gameViewTab);

			PanelTab sceneViewTab = panels["Game"].AddTab(sceneViewPanelContent);
            sceneViewTab.Icon = null;
            sceneViewTab.Label = "Scene";
            sceneViewTab.MinSize = new Vector2(320, 180);
            sceneViewTab.ID = "Scene";
            panelTabs.Add("Scene", sceneViewTab);

            panels["Game"].DockToRoot(Direction.None);
            panels["Game"].ActiveTab = 0;
            panels["Game"].ResizeTo(new Vector2(1091, 649));

			panels.Add("ProjectAndMedia", PanelUtils.CreatePanelFor(projectPanelContent, panelCanvas));
			PanelTab projectTab = panels["ProjectAndMedia"][0];
			projectTab.Icon = null;
			projectTab.Label = "Project";
			projectTab.MinSize = new Vector2(276, 200);
            projectTab.ID = "Project";
            panelTabs.Add("Project", projectTab);

			PanelTab mediaTab = panels["ProjectAndMedia"].AddTab(mediaPanelContent);
            mediaTab.Icon = null;
            mediaTab.Label = "Media";
            mediaTab.MinSize = new Vector2(276, 200);
            mediaTab.ID = "Media";
			panelTabs.Add("Media", mediaTab);

			panels["ProjectAndMedia"].ActiveTab = 0;
			panels["ProjectAndMedia"].ResizeTo(new Vector2(276, 649));

            panels.Add("Decorations", PanelUtils.CreatePanelFor(decorationsPanelContent, panelCanvas));
            PanelTab decorationsTab = panels["Decorations"][0];
            decorationsTab.Icon = null;
            decorationsTab.Label = "Decorations";
            decorationsTab.MinSize = new Vector2(276, 200);
            decorationsTab.ID = "Decorations";
			panelTabs.Add("Decorations", decorationsTab);

			panels["Decorations"].ResizeTo(new Vector2(276, 649));

            PanelGroup group = new PanelGroup(panelCanvas, Direction.Left);
            group.AddElement(panels["Decorations"]);
            group.AddElement(panels["ProjectAndMedia"]);
            group.DockToRoot(Direction.Left);

            panels.Add("Timeline", PanelUtils.CreatePanelFor(timelinePanelContent, panelCanvas));
            PanelTab timelineTab = panels["Timeline"][0];
            timelineTab.Icon = null;
            timelineTab.Label = "Timeline";
            timelineTab.MinSize = new Vector2(400, 300);
            timelineTab.ID = "Timeline";
			panelTabs.Add("Timeline", timelineTab);

			panels["Timeline"].DockToRoot(Direction.Bottom);
			panels["Timeline"].ResizeTo(new Vector2(1643, 403));

            panels.Add("Play", PanelUtils.CreatePanelFor(controlsPanelContent, panelCanvas));
            PanelTab controlsTab = panels["Play"][0];
            controlsTab.Icon = null;
            controlsTab.Label = "Play";
            controlsTab.MinSize = new Vector2(277, 157);
            controlsTab.ID = "Play";
			panelTabs.Add("Play", controlsTab);

			panels["Play"].ResizeTo(new Vector2(277, 157));

            panels.Add("Inspector", PanelUtils.CreatePanelFor(inspectorPanelContent, panelCanvas));
            PanelTab inspectorTab = panels["Inspector"][0];
            inspectorTab.Icon = null;
            inspectorTab.Label = "Inspector";
            inspectorTab.MinSize = new Vector2(276, 200);
            inspectorTab.ID = "Inspector";
			panelTabs.Add("Inspector", inspectorTab);

			panels["Inspector"].ResizeTo(new Vector2(277, 895));

			PanelGroup right = new PanelGroup(panelCanvas, Direction.Top);
			right.AddElement(panels["Inspector"]);
			right.AddElement(panels["Play"]);
			right.DockToRoot(Direction.Right);
		}

        private void OnOpenLevel(bool noLevel = false)
        {
            projectPanel.SetProperties(levelData.songSettings, levelData.levelSettings, levelData.miscSettings);
			projectPanel.SelectTab(0);

			decorationsPanel.SetProperties(levelData.decorationSettings);

			if (!noLevel)
            {
				mediaPanel.SetupItems(this);
				timelinePanel.Init();
            }
		}

        private void OnPlayLevel()
        {

        }

        public void SelectEvent(LevelEvent levelEvent)
        {
			eventsPanel.SetProperties(levelEvent.eventType, levelEvent);
		}

		public void UnselectEvent()
		{
            eventsPanel.HidePanel();
		}

		public virtual void SetEventSelector()
		{
			UnselectEvent();
			eventsPanel.SetSelector();
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

            OnPlayLevel();
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

		public void UpdateDecorationObjects()
		{
			customLevel.UpdateDecorationObjects(true);
			refreshDecSprites = false;
		}

		public void UpdateBackgroundSprites()
		{
			customLevel.UpdateBackgroundSprites();
			refreshBgSprites = false;
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
            if (text == "level.adofai" || text == "main.adofai")
            {
                text = new DirectoryInfo(Path.GetDirectoryName(ADOBase.levelPath)).Name + Path.DirectorySeparatorChar + text;
            }
			if (unsavedChanges)
			{
				text += "<color=#ffffff80> · unsaved</color>";
			}

			filenameText.text = text;
		}

        public LevelEvent AddEvent(int floor, LevelEventType type)
        {
            LevelEvent levelEvent = new LevelEvent(floor, type);
			events.Add(levelEvent);

            return levelEvent;
		}

		public void AddEvent(LevelEventType type)
		{
			timelinePanel.ApplySelector(type);
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
            RefreshFilenameText();
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

            OnOpenLevel();
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

		public void EnableEvent(LevelEvent e, bool enabled)
		{
			e.active = enabled;
			ApplyEventsToFloors();
		}

		public void ShowEvent(LevelEvent e, bool visible)
		{
			e.visible = visible;
			UpdateEventVisibility(e);
		}

		public void ForceHideEvent(scrDecoration dec, bool forceHide)
		{
			dec.forceHide = forceHide;
			UpdateEventVisibility(dec.sourceLevelEvent);
		}

		public void LockEvent(LevelEvent e, bool locked)
		{
			e.locked = locked;
		}

		public void ForceLockEvent(scrDecoration dec, bool forceLock)
		{
			dec.forceLock = forceLock;
		}

		private void UpdateEventVisibility(LevelEvent e)
		{
			scrDecoration scrDecoration = this.allDecorations.Find((scrDecoration d) => d.sourceLevelEvent == e);
			scrDecoration.SetVisible(e.visible && !scrDecoration.forceHide);
		}

		public void OnDecorationSelected(LevelEvent decorationEvent)
		{
			//if (!this.SelectionIsEmpty())
			//{
			//	this.DeselectFloors(false);
			//}
			//this.ShowSelectedColorForLastSelectedFloor();
		}

		public void OnDecorationAllItemsDeselected()
		{
			//if (this.lastSelectedFloor && this.SelectionIsEmpty())
			//{
			//	DOTween.Kill("selectedColorTween", false);
			//	this.ShowDeselectedColor(this.lastSelectedFloor);
			//}
		}

		public void SelectDecoration(int itemIndex, bool jumpToDecoration = true, bool showPanel = true, bool ignoreDeselection = false, bool ignoreAdjustRect = false)
		{
			LevelEvent sourceLevelEvent = scrDecorationManager.GetDecoration(itemIndex).sourceLevelEvent;
			if (sourceLevelEvent != null)
			{
				SelectDecoration(sourceLevelEvent, jumpToDecoration, showPanel, ignoreDeselection, ignoreAdjustRect);
			}
		}

		public void SelectDecoration(LevelEvent levelEvent, bool jumpToDecoration = true, bool showPanel = true, bool ignoreDeselection = false, bool ignoreAdjustRect = false)
		{
			using (new SaveStateScope(this, false, false, false))
			{
				bool flag = selectedDecorations.Contains(levelEvent);
				if (flag && RDInput.holdingControl && !ignoreDeselection)
				{
					DeselectDecoration(levelEvent);
				}
				else
				{
					if (!RDInput.holdingShift && !RDInput.holdingControl && !ignoreDeselection)
					{
						DeselectAllDecorations();
						//this.DeselectFloors(false);
						flag = false;
					}
					if (!(scrDecorationManager.GetDecoration(levelEvent) == null))
					{
						if (!flag)
						{
							selectedDecorations.Add(levelEvent);
						}
						if (jumpToDecoration && !Persistence.disableCameraDecorationFocus)
						{
							MoveCameraToDecoration(levelEvent);
						}
						scrDecorationManager.instance.ShowSelectionBorders(levelEvent, true);
						bool enable = SelectionDecorationIsSingle();
						decPivot.UpdatePivotCrossImage(enable);
						if (selectedDecorations.Count <= 1)
						{
							decTransformGizmo.Setup(levelEvent);
						}
						else
						{
							decTransformGizmo.UpdateGizmosVisibility();
						}
						int decorationIndex = scrDecorationManager.GetDecorationIndex(levelEvent);
						if (showPanel)
						{
                            //levelEventsPanel.ShowInspector(true, true);
                            levelEventsPanel.ShowPanel(levelEvent.eventType, 0);
                        }
						propertyControlDecorationsList.lastSelectedIndex = decorationIndex;
						propertyControlDecorationsList.RefreshItemsList(false);
						if (!ignoreAdjustRect)
						{
							propertyControlDecorationsList.RefreshScrollRectPosition(levelEvent);
						}
						if (propertyControlDecorationsList.OnItemSelected != null)
						{
							propertyControlDecorationsList.OnItemSelected(levelEvent);
						}
						selectingFloorID = false;
					}
				}
			}
		}

		public void DeselectDecoration(LevelEvent levelEvent)
		{
			using (new SaveStateScope(this, false, true, false))
			{
				if (selectedDecorations.Count <= 1)
				{
					DeselectAllDecorations();
				}
				else if (!(scrDecorationManager.GetDecoration(levelEvent) == null))
				{
					scrDecorationManager.instance.ShowSelectionBorders(levelEvent, false);
					decTransformGizmo.UpdateGizmosVisibility();
					selectedDecorations.Remove(levelEvent);
					LevelEvent levelEvent2 = selectedDecorations[selectedDecorations.Count - 1];
					SelectDecoration(levelEvent2, false, true, true, false);
				}
			}
		}

		public void DeselectAllDecorations()
		{
			if (SelectionDecorationIsEmpty())
			{
				return;
			}
			using (new SaveStateScope(this, false, false, false))
			{
				//levelEventsPanel.ShowInspector(false, false);
				scrDecorationManager.instance.ClearDecorationBorders();
				int count = selectedDecorations.Count;
				selectedDecorations.Clear();
				propertyControlDecorationsList.RefreshItemsList(false);
				decPivot.UpdatePivotCrossImage(false);
				decTransformGizmo.UpdateGizmosVisibility();
				if (count > 0)
				{
					levelEventsPanel.HideAllInspectorTabs();
					if (propertyControlDecorationsList.OnAllItemsDeselected != null)
					{
						propertyControlDecorationsList.OnAllItemsDeselected();
					}
				}
				selectingFloorID = false;
			}
		}

		public bool SelectionDecorationIsEmpty()
		{
			return selectedDecorations.Count == 0;
		}

		public bool SelectionDecorationIsSingle()
		{
			return selectedDecorations.Count == 1 && selectedDecorations[0] != null;
		}

		public LevelEvent AddDecoration(LevelEventType eventType, int index = -1)
		{
			LevelEvent levelEvent = CreateDecoration(eventType);
			AddDecoration(levelEvent, index);
			return levelEvent;
		}

		public void AddDecoration(LevelEvent dec, int index = -1)
		{
			using (new SaveStateScope(this, false, true, false))
			{
				int index2 = (index == -1) ? levelData.decorations.Count : (index + 1);
				levelData.decorations.Insert(index2, dec);
				bool flag;
				scrDecorationManager.instance.CreateDecoration(dec, out flag, index);
			}
		}

		private LevelEvent CreateDecoration(LevelEventType eventType)
		{
			LevelEvent levelEvent = new LevelEvent(-1, eventType);
			Vector3 position = Camera.main.transform.position;
			if (selectedFloors.Count == 1)
			{
				levelEvent["relativeTo"] = DecPlacementType.Tile;
				levelEvent.floor = selectedFloors[0].seqID;
				levelEvent["position"] = Vector2.zero;
			}
			else
			{
				levelEvent["position"] = new Vector2(position.x, position.y) / customLevel.GetTileSize();
			}
			return levelEvent;
		}

		public bool EventHasBackgroundSprite(LevelEvent evnt)
		{
			return evnt.eventType == LevelEventType.CustomBackground && !string.IsNullOrEmpty(evnt.data["bgImage"].ToString());
		}

		public void RemoveEvent(LevelEvent evnt, bool skipDecorationUpdate = false)
		{
			if (evnt == null)
			{
				return;
			}
			using (new SaveStateScope(this, false, true, false))
			{
				if (evnt.IsDecoration)
				{
					decorations.Remove(evnt);
					selectedDecorations.Remove(evnt);
					if (!skipDecorationUpdate)
					{
						decTransformGizmo.UpdateGizmosVisibility();
						UpdateDecorationObjects();
						levelEventsPanel.ShowPanel(LevelEventType.None, 0);
						levelEventsPanel.HideAllInspectorTabs();
						refreshBgSprites = true;
						if (decorations.Count == 0)
						{
							decPivot.UpdatePivotCrossImage(false);
						}
					}
				}
				else
				{
					events.Remove(evnt);
				}
				if (EventHasBackgroundSprite(evnt))
				{
					refreshBgSprites = true;
				}
			}
		}

		public void RemoveEvents(List<LevelEvent> events)
		{
			if (events == null || events.Count == 0)
			{
				return;
			}
			using (new SaveStateScope(this, false, true, false))
			{
				for (int i = 0; i < events.Count; i++)
				{
					RemoveEvent(events[i], i != events.Count - 1);
				}
			}
		}

		public void DeleteMultiSelectionDecorations()
		{
			if (SelectionDecorationIsEmpty())
			{
				return;
			}
			List<LevelEvent> events = new List<LevelEvent>(selectedDecorations);
			RemoveEvents(events);
			decTransformGizmo.UpdateGizmosVisibility();
		}

		public List<LevelEvent> GetSelectedFloorEvents(LevelEventType eventType)
		{
			return GetFloorEvents(selectedFloors[0].seqID, eventType);
		}

		public List<LevelEvent> GetFloorEvents(int floorID, LevelEventType eventType)
		{
			if (eventType.IsSetting())
			{
				return null;
			}
			List<LevelEvent> list = new List<LevelEvent>();
			foreach (LevelEvent levelEvent in events)
			{
				if (floorID == levelEvent.floor && levelEvent.eventType == eventType)
				{
					list.Add(levelEvent);
				}
			}
			return list;
		}

		private void MoveCameraToFloor(scrFloor floor)
		{
			Vector3 endValue = new Vector3(floor.x, floor.y, -10f);
			camera.transform.DOMove(endValue, cameraSelectDuration, false).SetUpdate(true);
		}

		public void MoveCameraToDecoration(LevelEvent levelEvent)
		{
			scrDecoration decoration = scrDecorationManager.GetDecoration(levelEvent);
			Vector2 v = decoration.transform.position.xy() - (ADOBase.controller.camy.transform.position.xy() - decoration.parallax.posCamAtStart.xy()) * decoration.parallax.multiplier;
			DoCameraJump(v.WithZ(-10f));
		}

		private void DoCameraJump(Vector3 targetPos)
		{
			camera.transform.DOKill(false);
			camera.transform.DOMove(targetPos, 0.4f, false).SetUpdate(true).SetEase(Ease.OutCubic);
		}
	}
}
