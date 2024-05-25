using ADOFAI;
using HarmonyLib;
using SA.GoogleDoc;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public LevelData levelData => this.customLevel.levelData;
		public EventsArray<LevelEvent> events => this.levelData.levelEvents;
		public List<scrFloor> floors => this.customLevel.levelMaker.listFloors;

		public new scnGame customLevel;

		public GameObject[] tabContainers;
		public Button[] tabButtons;
		public RawImage gameView;
		public Camera uiCamera;

		public Texture2D floorConnectorTex;

		private bool gamePaused => scrController.instance.paused;
		private bool playerPaused => !scrController.instance.audioPaused;

		private List<GameObject> floorConnectorGOs = new List<GameObject>();

		private Material lineMaterial;
		private GameObject floorConnectors;

		private bool shouldScrub = true;

		private readonly Color grayColor = new Color(0.42352942f, 0.42352942f, 0.42352942f);
		private readonly Color lineGreen = new Color(0.4f, 1f, 0.4f, 1f);
		private readonly Color lineYellow = new Color(1f, 1f, 0.4f, 1f);
		private readonly Color linePurple = new Color(0.75f, 0.5f, 1f, 1f);
		private readonly Color lineBlue = new Color(0.4f, 0.4f, 1f, 1f);

		public NeoEditor() : base() { }

		private void Awake()
		{
			NeoEditor.Instance = this;
			LoadGameScene();
		}

		private void Start()
        {
			customLevel = scnGame.instance;
			Application.wantsToQuit += this.TryApplicationQuit;

			lineMaterial = new Material(Shader.Find("ADOFAI/ScrollingSprite"));
			lineMaterial.SetTexture("_MainTex", floorConnectorTex);
			lineMaterial.SetVector("_ScrollSpeed", new Vector2(-0.4f, 0f));
			lineMaterial.SetFloat("_Time0", 0f);

			scrCamera camera = scrCamera.instance;
			camera.camobj.targetTexture = (RenderTexture)gameView.texture;
			camera.BGcam.targetTexture = (RenderTexture)gameView.texture;
			camera.Bgcamstatic.targetTexture = (RenderTexture)gameView.texture;
			camera.Overlaycam.targetTexture = (RenderTexture)gameView.texture;

			scrUIController uIController = scrUIController.instance;
			uiController.canvas.renderMode = RenderMode.ScreenSpaceCamera;
			uiController.canvas.worldCamera = uiCamera;

			//floorConnectors = GameObject.Find("Floor Connector Lines");
			floorConnectors = new GameObject("Floor Connector Lines");

			for (int i = 0; i < 7; i++)
			{
				int tab = i;
				tabButtons[i].onClick.AddListener(() => SelectTab((EditorTab)tab));
			}

			SelectTab(EditorTab.Project);

			customLevel.RemakePath();
			customLevel.ResetScene();

			FloorMesh.UpdateAllRequired();

			OpenLevel();
			//customLevel.Play(0, false);
		}

		private void Update()
        {
        
        }

		private void LateUpdate()
		{
			
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
			}
			//
		}



		public void Play()
		{
			RDC.auto = true;
			customLevel.ResetScene(true);
			scrController.instance.paused = false;

			if (shouldScrub)
			{
				customLevel.Play(0, true);
				shouldScrub = false;
			}
			else
			{
				scrController.instance.TogglePauseGame();
			}
		
			GameObject.Find("Error Meter(Clone)").SetActive(false);
		}

		public void Pause()
		{
			scrController.instance.TogglePauseGame();
		}

		public void OpenLevel()
		{
			StartCoroutine(OpenLevelCo());
			customLevel.ReloadAssets();

			//DrawFloorOffsetLines();
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

		private string SanitizeLevelPath(string path)
		{
			return Uri.UnescapeDataString(path.Replace("file:", ""));
		}

		private void ClearAllFloorOffsets()
		{
			foreach (GameObject obj in this.floorConnectorGOs)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.floorConnectorGOs.Clear();
		}

		private void DrawFloorOffsetLines()
		{
			foreach (GameObject obj in this.floorConnectorGOs)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.floorConnectorGOs.Clear();
			int num = -2;
			Vector3 vector = Vector3.zero;
			foreach (LevelEvent levelEvent in this.events)
			{
				if (levelEvent.eventType == LevelEventType.PositionTrack && levelEvent.floor > 0)
				{
					int floor = levelEvent.floor;
					if ((!(this.floors[floor].prevfloor != null) || this.floors[floor].prevfloor.holdLength <= -1) && (!levelEvent.data.Keys.Contains("justThisTile") || !levelEvent.GetBool("justThisTile")))
					{
						if (floor != num)
						{
							vector = new Vector2(0f, 0f);
						}
						Vector3 vector2 = (Vector2)levelEvent.data["positionOffset"] * this.customLevel.GetTileSize();
						Vector2 vector3 = new Vector2(-0.75f * Mathf.Cos((float)this.floors[floor - 1].exitangle + 1.5707964f), 0.75f * Mathf.Sin((float)this.floors[floor - 1].exitangle + 1.5707964f));
						Vector3 vector4 = new Vector3(vector.x + this.floors[floor - 1].transform.position.x + vector3.x, vector.y + this.floors[floor - 1].transform.position.y + vector3.y, this.floors[floor - 1].transform.position.z);
						Vector3 vector5 = new Vector3(vector4.x + vector2.x, vector4.y + vector2.y, this.floors[floor].transform.position.z);
						if (Vector3.Distance(vector4, vector5) >= 0.05f)
						{
							GameObject gameObject = new GameObject();
							LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
							lineRenderer.positionCount = 2;
							lineRenderer.material = this.lineMaterial;
							lineRenderer.textureMode = LineTextureMode.Tile;
							if (levelEvent.GetBool("editorOnly"))
							{
								lineRenderer.startColor = this.lineGreen;
								lineRenderer.endColor = this.lineYellow;
							}
							else
							{
								lineRenderer.startColor = this.linePurple;
								lineRenderer.endColor = this.lineBlue;
							}
							lineRenderer.SetPosition(0, vector4);
							lineRenderer.SetPosition(1, vector5);
							lineRenderer.startWidth = 0.1f;
							lineRenderer.endWidth = 0.1f;
							lineRenderer.name = "Floor connector";
							lineRenderer.transform.parent = this.floorConnectors.transform;
							this.floorConnectorGOs.Add(gameObject);
							Vector2 vector6 = (Vector2)levelEvent.data["positionOffset"] * this.customLevel.GetTileSize();
							vector += new Vector3(vector6.x, vector6.y, 0f);
							num = floor;
						}
					}
				}
			}
		}

		private IEnumerator OpenLevelCo(string definedLevelPath = null)
		{
			this.ClearAllFloorOffsets();
			//this.redoStates.Clear();
			//this.undoStates.Clear();
			bool flag = definedLevelPath == null;
			string lastLevelPath = this.customLevel.levelPath;
			if (flag)
			{
				string[] levelPaths = StandaloneFileBrowser.OpenFilePanel(RDString.Get("editor.dialog.openFile", null, LangSection.Translations), Persistence.GetLastUsedFolder(), new ExtensionFilter[]
				{
				new ExtensionFilter(RDString.Get("editor.dialog.adofaiLevelDescription", null, LangSection.Translations), GCS.levelExtensions)
				}, false);
				yield return null;
				if (levelPaths.Length == 0 || string.IsNullOrEmpty(levelPaths[0]))
				{
					yield break;
				}
				string text = this.SanitizeLevelPath(levelPaths[0]);
				string text2 = Path.GetExtension(text).ToLower();
				string value = text2.Substring(1, text2.Length - 1);
				if (GCS.levelZipExtensions.Contains(value))
				{
					string availableDirectoryName = RDUtils.GetAvailableDirectoryName(Path.Combine(Path.GetDirectoryName(text), Path.GetFileNameWithoutExtension(text)));
					RDDirectory.CreateDirectory(availableDirectoryName);
					try
					{
						ZipUtils.Unzip(text, availableDirectoryName);
					}
					catch (Exception ex)
					{
						//this.ShowNotificationPopup(RDString.Get("editor.notification.unzipFailed", null, LangSection.Translations), null, null);
						string str = "Unzip failed: ";
						Exception ex2 = ex;
						Debug.LogError(str + ((ex2 != null) ? ex2.ToString() : null));
						Directory.Delete(availableDirectoryName, true);
						yield break;
					}
					//string text3 = this.FindAdofaiLevelOnDirectory(availableDirectoryName);
					string text3 = null;
					if (text3 == null)
					{
						//this.ShowNotificationPopup(RDString.Get("editor.notification.levelNotFound", null, LangSection.Translations), null, null);
						Directory.Delete(availableDirectoryName, true);
						yield break;
					}
					this.customLevel.levelPath = text3;
				}
				else
				{
					this.customLevel.levelPath = text;
				}
				levelPaths = null;
			}
			else
			{
				this.customLevel.levelPath = definedLevelPath;
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
				flag2 = this.customLevel.LoadLevel(ADOBase.levelPath, out loadResult);
			}
			catch (Exception ex3)
			{
				text4 = string.Concat(new string[]
				{
				"Error loading level file at ",
				ADOBase.levelPath,
				": ",
				ex3.Message,
				", Stacktrace:\n",
				ex3.StackTrace
				});
				Debug.Log(text4);
			}
			if (flag2)
			{
				customLevel.RemakePath(true, true);
				//this.errorImageResult.Clear();
				//this.isUnauthorizedAccess = false;
				//this.RemakePath(true, true);
				//this.lastSelectedFloor = null;
				//this.SelectFirstFloor();
				//this.UpdateSongAndLevelSettings();
				//this.customLevel.ReloadAssets(true, true);
				//this.UpdateDecorationObjects();
				//DiscordController discordController = DiscordController.instance;
				//if (discordController != null)
				//{
				//	discordController.UpdatePresence();
				//}
				//this.ShowNotification(RDString.Get("editor.notification.levelLoaded", null, LangSection.Translations), null, 1.25f);
				//this.unsavedChanges = false;
			}
			else
			{
				this.customLevel.levelPath = lastLevelPath;
				GCS.customLevelId = customLevelId;
				//this.ShowNotificationPopup(text4, new scnEditor.NotificationAction[]
				//{
				//new scnEditor.NotificationAction(RDString.Get("editor.notification.copyText", null, LangSection.Translations), delegate()
				//{
				//	this.notificationPopupContent.text.CopyToClipboard();
				//	this.ShowNotification(RDString.Get("editor.notification.copiedText", null, LangSection.Translations), null, 1.25f);
				//}),
				//new scnEditor.NotificationAction(RDString.Get("editor.ok", null, LangSection.Translations), delegate()
				//{
				//	this.CloseNotificationPopup();
				//})
				//}, RDString.Get(string.Format("editor.notification.loadingFailed.{0}", loadResult), null, LangSection.Translations));
			}
			//this.isLoading = false;
			//this.CloseAllPanels(null);
			//yield return null;
			//this.ShowImageLoadResult();
			yield break;
		}
	}
}
