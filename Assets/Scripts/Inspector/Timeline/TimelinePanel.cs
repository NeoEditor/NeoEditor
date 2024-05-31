using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using NeoEditor.Tabs;
using OggVorbisEncoder.Setup;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Timeline
{
    public class TimelinePanel : MonoBehaviour
    {
        public EffectTabBase tab;

        public GameObject horizontalLine;
        public GameObject verticalLine;
        public GameObject eventObj;
        public FloorNumberText floorNum;

        public RectTransform root;
        public RectTransform content;
        public RectTransform grid;
        public RectTransform events;
        public ScrollRect scroll;
        public RectTransform scrollRT;
        public RectTransform floorNumBar;

        public GameObject playhead;

        private float width = 200f;
        private float height = 25f;
        private float scale = 1f;
        private int vIndex;
        private int magnetNum = 4;
        private bool followPlayhead = true;
        private bool changingScroll = false;
        private Vector2 prevScrollPos = Vector2.zero;

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

        private ObjectPool<GameObject> hPool;
        private ObjectPool<GameObject> vPool;
        private ObjectPool<GameObject> eventPool;
        private ObjectPool<FloorNumberText> floorNumPool;

        private LinkedList<VerticalLineData> vLines = new LinkedList<VerticalLineData>();

        void CreatePool()
        {
            hPool = new ObjectPool<GameObject>(
                () => Instantiate(horizontalLine, grid),
                (obj) => obj.SetActive(true),
                (obj) => obj.SetActive(false),
                (obj) => Destroy(obj),
                true,
                20,
                100
            );
            vPool = new ObjectPool<GameObject>(
                () => Instantiate(verticalLine, grid),
                (obj) => obj.SetActive(true),
                (obj) => obj.SetActive(false),
                (obj) => Destroy(obj),
                true,
                20,
                1000
            );
            eventPool = new ObjectPool<GameObject>(
                () => Instantiate(eventObj, events),
                (obj) => obj.SetActive(true),
                (obj) => obj.SetActive(false),
                (obj) => Destroy(obj),
                true,
                20,
                10000
            );
            floorNumPool = new ObjectPool<FloorNumberText>(
                () => Instantiate(floorNum, floorNumBar),
                (obj) => obj.gameObject.SetActive(true),
                (obj) => obj.gameObject.SetActive(false),
                (obj) => Destroy(obj),
                true,
                20,
                1000
            );

            scroll.onValueChanged.AddListener(OnValueChanged);
        }

        public void Init()
        {
            NeoEditor editor = NeoEditor.Instance;
            if (vPool == null)
                CreatePool();

            var floors = editor.floors;

            scroll.content.anchoredPosition = Vector2.zero;
            float scrollWidth = scrollRT.rect.width;
            float scrollHeight = scrollRT.rect.height;

            foreach (var line in vLines)
                vPool.Release(line.obj);
            vLines.Clear();

            foreach (var floor in floors)
            {
                if (floor.midSpin)
                    continue;

                float posX = TimeToBeat(floor.entryTime) * width * scale;
                if (posX < scrollWidth)
                {
                    var line = vPool.Get();
                    var num = floorNumPool.Get();
                    line.transform.LocalMoveX(posX);
                    num.transform.LocalMoveX(posX + scroll.content.anchoredPosition.x);
                    num.text.text = floor.seqID.ToString();

                    line.SetActive(true);
                    num.gameObject.SetActive(true);
                    vLines.AddLast(new VerticalLineData(floor.seqID, posX, line, num));
                }
            }

            foreach (var levelEvent in editor.events)
            {
                if (ignoreEvents.Contains(levelEvent.eventType))
                    continue;
                GameObject obj = Instantiate(eventObj, events.transform);
                scrFloor floor = floors[levelEvent.floor];

                obj.transform.GetChild(0).GetComponent<Image>().sprite = GCS.levelEventIcons[
                    levelEvent.eventType
                ];
                obj.GetComponent<TimelineEvent>().panel = this;

                Vector2 position = new Vector2(TimeToBeat(floor.entryTime) * width, -25);
                float bpm = editor.customLevel.levelData.bpm;
                object f;
                bool b = levelEvent.data.TryGetValue("angleOffset", out f);

                position += new Vector2((b ? (float)f : 0) / 180f * (1 / floor.speed) * width, 0);
                obj.transform.LocalMoveX(position.x);

                b = levelEvent.data.TryGetValue("duration", out f);
                obj.GetComponent<RectTransform>()
                    .SizeDeltaX(
                        b ? Mathf.Max((float)f * width * (1 / floor.speed), height) : height
                    );
            }

            scrConductor conductor = scrConductor.instance;
            content
                .GetComponent<RectTransform>()
                .SizeDeltaX(
                    TimeToBeat(floors[floors.Count - 1].entryTime + conductor.crotchet * 4) * width
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
                playhead.transform.LocalMoveX(TimeToBeat(conductor.songposition_minusi) * width);

                if (followPlayhead)
                {
                    changingScroll = true;
                    scroll.content.anchoredPosition = new Vector2(
                        -playhead.transform.localPosition.x + root.rect.width / 2,
                        scroll.content.anchoredPosition.y
                    );
                    changingScroll = false;
                }
            }
        }

        public void OnValueChanged(Vector2 position)
        {
            NeoEditor editor = NeoEditor.Instance;

            VerticalLineData CreateLine(scrFloor floor, VerticalLineData prev)
            {
                if (floor.midSpin)
                    return new VerticalLineData(floor.seqID, prev.x, null, null);
                var line = vPool.Get();
                var num = floorNumPool.Get();
                float posX = TimeToBeat(floor.entryTime) * width * scale;
                line.transform.LocalMoveX(posX);
                num.transform.LocalMoveX(posX + scroll.content.anchoredPosition.x);
                num.text.text = floor.seqID.ToString();

                line.SetActive(true);
                num.gameObject.SetActive(true);
                return new VerticalLineData(floor.seqID, posX, line, num);
            }

            Vector2 pos = position * (content.sizeDelta - scrollRT.rect.size);
            Vector2 dir = prevScrollPos - pos;

            if (dir.x < 0)
            {
                var first = vLines.First.Value;
                while (first.x < pos.x)
                {
                    if (first.obj != null)
                        vPool.Release(first.obj);
                    if (first.num != null)
                        floorNumPool.Release(first.num);
                    vLines.RemoveFirst();
                    first = vLines.First.Value;
                }
                var last = vLines.Last.Value;
                while (last.x < pos.x + scrollRT.rect.width)
                {
                    if (last.id + 1 >= editor.floors.Count)
                        break;

                    var floor = editor.floors[last.id + 1];
                    vLines.AddLast(CreateLine(floor, last));
                    last = vLines.Last.Value;
                }
            }
            else if (dir.x > 0)
            {
                var last = vLines.Last.Value;
                while (last.x > pos.x + scrollRT.rect.width)
                {
                    if (last.obj != null)
                        vPool.Release(last.obj);
                    if (last.num != null)
                        floorNumPool.Release(last.num);
                    vLines.RemoveLast();
                    last = vLines.Last.Value;
                }
                var first = vLines.First.Value;
                while (first.x > pos.x)
                {
                    if (first.id + -1 < 0)
                        break;

                    var floor = editor.floors[first.id - 1];
                    vLines.AddFirst(CreateLine(floor, last));
                    first = vLines.First.Value;
                }
            }
            prevScrollPos = pos;

            //if (!changingScroll && dir.x != 0)
            //    followPlayhead = false;
        }

        float TimeToBeat(double time)
        {
            return (float)(time / scrConductor.instance.crotchet);
        }
    }
}
