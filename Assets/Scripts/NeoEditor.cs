using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ADOFAI;
using DG.Tweening;
using HarmonyLib;
using NeoEditor.Patches;
using NeoEditor.PopupWindows;
using NeoEditor.Tabs;
using SA.GoogleDoc;
using SFB;
using UnityEngine;
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

        public GameObject prefab_inspector;
        public GameObject prefab_property;
        public GameObject prefab_controlBool;
        public GameObject prefab_controlText;
        public GameObject prefab_controlLongText;
        public GameObject prefab_controlColor;
        public GameObject prefab_controlFile;
        public GameObject prefab_controlVector2;

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

        private bool shouldScrub = true;
        private int scrubTo = 0;

        public Camera BGcamCopy;
        public Camera BGcamstaticCopy;

        public float camUserSizeTarget = 1f;
        public float camUserSize = 1f;
        public float scrollSpeed;
        public Tween anchorZoomTween;

        private readonly Color grayColor = new Color(0.42352942f, 0.42352942f, 0.42352942f);
        private readonly Color lineGreen = new Color(0.4f, 1f, 0.4f, 1f);
        private readonly Color lineYellow = new Color(1f, 1f, 0.4f, 1f);
        private readonly Color linePurple = new Color(0.75f, 0.5f, 1f, 1f);
        private readonly Color lineBlue = new Color(0.4f, 0.4f, 1f, 1f);

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
            LoadGameScene();
        }

        private void Start()
        {
            customLevel = scnGame.instance;
            Application.wantsToQuit += TryApplicationQuit;

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

            var dictionary = GCS
                .settingsInfo.Concat(GCS.levelEventsInfo)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            for (int i = 0; i < 7; i++)
            {
                int tab = i;
                tabs[i].InitTab(dictionary);
                tabButtons[i].onClick.AddListener(() => SelectTab((EditorTab)tab));
            }

            SelectTab(EditorTab.Project);

            customLevel.RemakePath();
            customLevel.ResetScene();

            FloorMesh.UpdateAllRequired();

            OpenLevel();

            scrController.instance.paused = true;
            GameObject.Find("Error Meter(Clone)")?.SetActive(false);
            //customLevel.Play(0, false);
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

        private IEnumerator OpenLevelCo(string definedLevelPath = null)
        {
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
    }
}
