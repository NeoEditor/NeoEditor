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

        private int firstLineShowingOnScreenIdx = -1;
        private int lastLineShowingOnScreenIdx = -1;

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

            for (int i = 0; i < floors.Count; i++)
            {
                var floor = floors[i];

                if (floor.midSpin)
                    continue;

                float posX = GetLinePosX(floor);
                if (posX < scrollWidth)
                {
                    var lineData = CreateLineData(floor, i);
                    vLines.AddLast(lineData);

                    //nextLineIdxToShow++;
                    lastLineShowingOnScreenIdx++;
                }
                //if (nextLineIdxToShow < 0)
                //    nextLineIdxToShow = 0;
                if (lastLineShowingOnScreenIdx >= 0)
                    firstLineShowingOnScreenIdx = 0;
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

            Vector2 pos = position * (content.sizeDelta - scrollRT.rect.size);
            Vector2 dir = prevScrollPos - pos;

            Main.Entry.Logger.Log("dir: " + dir);

            // prevScrollPos < (current) pos
            // scrolled to the right
            if (dir.x < 0)
            {
                int frontLinesToRemove = 0;
                foreach(var line in vLines)
                {
                    if (line.x < pos.x)
                    {
                        // 화면 왼쪽으로 나간 line들을 언로드
                        if (line.obj != null)
                            vPool.Release(line.obj);
                        if (line.num != null)
                            floorNumPool.Release(line.num);

                        frontLinesToRemove++;
                    }
                    else break;
                }

                Main.Entry.Logger.Log("[TimelinePanel.OnValueChanged] lines to remove from start: " + frontLinesToRemove);
                for (int i = 0; i < frontLinesToRemove; i++)
                {
                    // foreach 안에서 list item을 앞에서부터 삭제하는 것이 불가능하므로
                    // 삭제할 line 개수를 센 다음 따로 루프돌려서 삭제
                    vLines.RemoveFirst();
                }

                firstLineShowingOnScreenIdx += frontLinesToRemove;

                lastLineShowingOnScreenIdx = firstLineShowingOnScreenIdx + vLines.Count - 1;
                for (int i = lastLineShowingOnScreenIdx + 1; i < editor.floors.Count; i++)
                {
                    var floor = editor.floors[i];
                    float posX = GetLinePosX(floor);
                    if (posX < pos.x)
                    {
                        // 너무 긴 길이를 건너뛰어서 vLines list에 아무것도 없을 경우 (pos.x 변경 전과 후 지점에서 겹치는 line이 없을 경우)
                        // 루프를 순차적으로 돌면서 처음으로 표시되어야 할 line을 찾기
                        firstLineShowingOnScreenIdx = i + 1;
                        lastLineShowingOnScreenIdx = firstLineShowingOnScreenIdx;
                        continue;
                    }
                    if (posX > pos.x + scrollRT.rect.width)
                        break;

                    Main.Entry.Logger.Log("[TimelinePanel.OnValueChanged] adding floor idx " + i);
                    vLines.AddLast(CreateLineData(floor, i));

                    lastLineShowingOnScreenIdx++;
                }
            }

            // prevScrollPos > (current) pos
            // scrolled to the left
            else if (dir.x > 0)
            {
                int backLinesToRemove = 0;
                foreach (var line in vLines.Reverse())
                {
                    if (line.x > pos.x + scrollRT.rect.width)
                    {
                        if (line.obj != null)
                            vPool.Release(line.obj);
                        if (line.num != null)
                            floorNumPool.Release(line.num);

                        backLinesToRemove++;
                    }
                }

                Main.Entry.Logger.Log("[TimelinePanel.OnValueChanged] lines to remove from end: " + backLinesToRemove);
                for (int i = 0; i < backLinesToRemove; i++)
                {
                    vLines.RemoveLast();
                }

                lastLineShowingOnScreenIdx -= backLinesToRemove;

                firstLineShowingOnScreenIdx = lastLineShowingOnScreenIdx - vLines.Count + 1;
                for (int i = firstLineShowingOnScreenIdx - 1; i >= 0; i--)
                {
                    var floor = editor.floors[i];
                    float posX = GetLinePosX(floor);
                    if (posX > pos.x + scrollRT.rect.width)
                    {
                        lastLineShowingOnScreenIdx = i - 1;
                        firstLineShowingOnScreenIdx = lastLineShowingOnScreenIdx;
                        continue;
                    }
                    if (posX < pos.x)
                        break;

                    Main.Entry.Logger.Log("[TimelinePanel.OnValueChanged] adding floor idx " + i);
                    vLines.AddFirst(CreateLineData(floor, i));

                    firstLineShowingOnScreenIdx--;
                }
            }
            prevScrollPos = pos;

            //if (!changingScroll && dir.x != 0)
            //    followPlayhead = false;

            Main.Entry.Logger.Log("Update Complete! cnt = " + vLines.Count + ", firstIdx = " + firstLineShowingOnScreenIdx + ", lastIdx = " + lastLineShowingOnScreenIdx);
        }

        float TimeToBeat(double time)
        {
            return (float)(time / scrConductor.instance.crotchet);
        }

        private VerticalLineData CreateLineData(scrFloor floor, int floorIdx)
        {
            NeoEditor editor = NeoEditor.Instance;

            if (floor.midSpin)
            {
                // find first previous floor which is not a midspin
                // NOTE: can throw exception when no non-midspin floor exist before floorIdx
                // But that situation should not occur since valid chart data always starts with non-midspin floor
                scrFloor prevNormalFloor = null;
                for (int i = floorIdx - 1; i >= 0; i--)
                {
                    if (!editor.floors[i].midSpin)
                    {
                        prevNormalFloor = editor.floors[i];
                        break;
                    }
                }
                var prevLineX = GetLinePosX(prevNormalFloor);

                return new VerticalLineData(floor.seqID, prevLineX, null, null);
            }
                
            var line = vPool.Get();
            var num = floorNumPool.Get();
            float posX = GetLinePosX(floor);
            line.transform.LocalMoveX(posX);
            num.transform.LocalMoveX(posX + scroll.content.anchoredPosition.x);
            num.text.text = floor.seqID.ToString();

            line.SetActive(true);
            num.gameObject.SetActive(true);
            return new VerticalLineData(floor.seqID, posX, line, num);
        }

        private float GetLinePosX(scrFloor floor)
        {
            return TimeToBeat(floor.entryTime) * width * scale;
        }
    }
}
