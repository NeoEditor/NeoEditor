using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using NeoEditor.Tabs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Timeline
{
    public class TimelinePanel : MonoBehaviour
    {
        public EffectTabBase tab;

        public GameObject horizontalGrid;
        public GameObject verticalGrid;
        public GameObject EventObj;

        public RectTransform root;
        public GameObject content;
        public GameObject grid;
        public GameObject events;
        public ScrollRect scroll;

        public GameObject playhead;

        private List<GameObject> horizontalLines = new List<GameObject>();
        private List<GameObject> verticalLines = new List<GameObject>();

        private float scale = 200f;
        private float height = 25f;

        private LevelEventType[] ignoreEvents = new LevelEventType[]
        {
            LevelEventType.None,
            LevelEventType.SetSpeed,
            LevelEventType.Twirl,
            LevelEventType.Checkpoint,
            LevelEventType.LevelSettings,
            LevelEventType.SongSettings,
            LevelEventType.TrackSettings,
            LevelEventType.BackgroundSettings,
            LevelEventType.CameraSettings,
            LevelEventType.MiscSettings,
            LevelEventType.EventSettings,
            LevelEventType.DecorationSettings,
            LevelEventType.AddDecoration,
            LevelEventType.AddText,
            LevelEventType.Hold,
            LevelEventType.CallMethod,
            LevelEventType.AddComponent,
            LevelEventType.MultiPlanet,
            LevelEventType.FreeRoam,
            LevelEventType.FreeRoamTwirl,
            LevelEventType.FreeRoamRemove,
            LevelEventType.FreeRoamWarning,
            LevelEventType.Pause,
            LevelEventType.AutoPlayTiles,
            LevelEventType.ScaleMargin,
            LevelEventType.ScaleRadius,
            LevelEventType.Multitap,
            LevelEventType.TileDimensions,
            LevelEventType.KillPlayer,
            LevelEventType.SetFloorIcon,
            LevelEventType.AddObject
        };

        private TimelineEvent selectedEvent;

        public void Init()
        {
            foreach (var gameObject in horizontalLines)
                Destroy(gameObject);
            foreach (var gameObject in verticalLines)
                Destroy(gameObject);

            horizontalLines.Clear();
            verticalLines.Clear();

            NeoEditor editor = NeoEditor.Instance;

            var floors = editor.floors;

            foreach (var floor in floors)
            {
                if (floor.midSpin)
                    continue;

                GameObject line = Instantiate(verticalGrid, grid.transform);
                line.transform.LocalMoveX(TimeToBeat(floor.entryTime) * scale);

                line.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    floor.seqID.ToString();
                line.SetActive(true);

                verticalLines.Add(line);
            }

            foreach (var levelEvent in editor.events)
            {
                if (ignoreEvents.Contains(levelEvent.eventType))
                    continue;
                GameObject eventObj = Instantiate(EventObj, events.transform);
                scrFloor floor = floors[levelEvent.floor];

                eventObj.transform.GetChild(0).GetComponent<Image>().sprite = GCS.levelEventIcons[
                    levelEvent.eventType
                ];
                eventObj.GetComponent<TimelineEvent>().panel = this;

                Main.Entry.Logger.Log(levelEvent.ToString());

                Vector2 position = new Vector2(TimeToBeat(floor.entryTime) * scale, -25);
                float bpm = editor.customLevel.levelData.bpm;
                object f;
                bool b = levelEvent.data.TryGetValue("angleOffset", out f);

                position += new Vector2((b ? (float)f : 0) / 180f * (1 / floor.speed) * scale, 0);
                eventObj.transform.LocalMoveX(position.x);

                b = levelEvent.data.TryGetValue("duration", out f);
                eventObj
                    .GetComponent<RectTransform>()
                    .SizeDeltaX(
                        b ? Mathf.Max((float)f * scale * (1 / floor.speed), height) : height
                    );
            }

            scrConductor conductor = scrConductor.instance;
            content
                .GetComponent<RectTransform>()
                .SizeDeltaX(
                    TimeToBeat(floors[floors.Count - 1].entryTime + conductor.crotchet * 4) * scale
                );
            content.GetComponent<RectTransform>().SizeDeltaY(1000f);
        }

        public void SelectEvent(TimelineEvent timelineEvent)
        {
            if (selectedEvent != null)
                selectedEvent.UnselectEvent();
            selectedEvent = timelineEvent;
            tab.SelectEvent(selectedEvent.targetEvent);
        }

        public void SetParent(RectTransform transform)
        {
            transform.SetParent(transform, false);
        }

        void Update()
        {
            if (NeoEditor.Instance.paused)
                return;
            scrConductor conductor = scrConductor.instance;
            if (conductor.song.clip)
            {
                playhead.transform.LocalMoveX(TimeToBeat(conductor.songposition_minusi) * scale);
                scroll.content.anchoredPosition = new Vector2(
                    -playhead.transform.localPosition.x + root.rect.width / 2,
                    scroll.content.anchoredPosition.y
                );
            }
        }

        float TimeToBeat(double time)
        {
            return (float)(time / scrConductor.instance.crotchet);
        }
    }
}
