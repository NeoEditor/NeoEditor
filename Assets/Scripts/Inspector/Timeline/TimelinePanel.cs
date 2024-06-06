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
       
        //private LinkedList<LevelEventData> currentlyShowingEvents = new LinkedList<LevelEventData>();

        private List<LevelEventData> levelEventsDataSortedByStartPos = new List<LevelEventData>();
        private List<LevelEventData> levelEventsDataSortedByEndPos = new List<LevelEventData>();

        // level events sorted by startPosX, index of last item showing on viewport (-1 = before first item showing on viewport)
        private int levelEventsSortedByStartPosListEndIdx = -1;
        // level events sorted by endPosX, index of first item showing on viewport
        private int levelEventsSortedByEndPosListStartIdx = 0;

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

            foreach (var eventData in levelEventsDataSortedByStartPos)
                if (eventData.obj != null)
                    eventPool.Release(eventData.obj);
            levelEventsDataSortedByStartPos.Clear();
            levelEventsDataSortedByEndPos.Clear();

            levelEventsSortedByStartPosListEndIdx = -1;
            levelEventsSortedByEndPosListStartIdx = 0;

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
                // get position
                Vector2 position = new Vector2(GetEventPosX(levelEvent), -25);

                if (ignoreEvents.Contains(levelEvent.eventType))
                    continue;

                float entryTime = (float)floors[levelEvent.floor].entryTime;
                float duration = GetEventDuration(levelEvent);
                float objWidth = GetEventObjWidth(duration);

                var eventData = new LevelEventData(entryTime, duration, levelEvent);
                levelEventsDataSortedByStartPos.Add(eventData);
                levelEventsDataSortedByEndPos.Add(eventData);

                if (position.x <= scrollWidth)
                {
                    //Main.Entry.Logger.Log("[d] (init phase) adding event | floor " + levelEvent.floor + " type: " + levelEvent.eventType);

                    var obj = CreateEventObject(levelEvent, position.x, objWidth);

                    eventData.obj = obj;
                    levelEventsSortedByStartPosListEndIdx++;
                }
            }
            //if (levelEventsSortedByStartPosListEndIdx >= 0)
            //    levelEventsSortedByStartPosListStartIdx = 0;

            levelEventsDataSortedByEndPos.Sort((a, b) => {
                // sort by event end time, smaller one goes first
                float diff = a.end - b.end;
                if (diff < 0) 
                    return -1;
                else if (diff > 0)
                    return 1;
                else
                    return 0;
            });
            //foreach (var data in levelEventsDataSortedByEndPos)
            //{
            //    float endPosX = GetEventPosX(data.evt) + GetEventObjWidth(data.evt);
            //    if (endPosX > scrollWidth)
            //        break;

            //    levelEventsSortedByEndPosListStartIdx++;
            //}

            scrConductor conductor = scrConductor.instance;
            float timelineWidth = TimeToBeat(floors.Last().entryTime + conductor.crotchet * 4) * width;
            
            content
                .GetComponent<RectTransform>()
                .SizeDeltaX(timelineWidth);
            content.GetComponent<RectTransform>().SizeDeltaY(1000f);

            floorNumBar
                .GetComponent<RectTransform>()
                .SizeDeltaX(timelineWidth);
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

            // prevScrollPos < (current) pos
            // scrolled to the right
            if (dir.x < 0)
            {
                int frontLinesToRemove = 0;
                foreach (var line in vLines)
                {
                    if (line.x < pos.x)
                    {
                        // ȭ�� �������� ���� line���� ��ε�
                        if (line.obj != null)
                            vPool.Release(line.obj);
                        if (line.num != null)
                            floorNumPool.Release(line.num);

                        frontLinesToRemove++;
                    }
                    else
                        break;
                }

                for (int i = 0; i < frontLinesToRemove; i++)
                {
                    // foreach �ȿ��� list item�� �տ������� �����ϴ� ���� �Ұ����ϹǷ�
                    // ������ line ������ �� ���� ���� ���������� ����
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
                        // �ʹ� �� ���̸� �ǳʶپ vLines list�� �ƹ��͵� ���� ��� (pos.x ���� ���� �� �������� ��ġ�� line�� ���� ���)
                        // ������ ���������� ���鼭 ó������ ǥ�õǾ�� �� line�� ã��
                        firstLineShowingOnScreenIdx = i + 1;
                        lastLineShowingOnScreenIdx = firstLineShowingOnScreenIdx;
                        continue;
                    }
                    if (posX > pos.x + scrollRT.rect.width)
                        break;

                    vLines.AddLast(CreateLineData(floor, i));

                    lastLineShowingOnScreenIdx++;
                }

                // ====

                // remove level events object which is completely hidden to the left of viewport
                for (int i = levelEventsSortedByEndPosListStartIdx; i < levelEventsDataSortedByEndPos.Count; i++)
                {
                    var data = levelEventsDataSortedByEndPos[i];
                    float endPosX = GetEventPosX(data.evt) + GetEventObjWidth(data.evt);

                    if (endPosX >= pos.x)
                        break;

                    //Main.Entry.Logger.Log("[d] removing event " + i + " from front side");

                    if (data.obj != null)
                    {
                        eventPool.Release(data.obj);
                        data.obj = null;
                    }

                    levelEventsSortedByEndPosListStartIdx++;
                }

                //foreach (var eventData in currentlyShowingEvents)
                //{
                //    float posX = GetEventPosX(eventData.evt);

                //    if (!eventData.objAvailable && posX < pos.x)
                //    {
                //        frontEventsToRemove++;
                //        continue;
                //    }

                //    float objWidth = GetEventObjWidth(eventData.evt);
                //    if (posX + objWidth < pos.x)
                //    {
                //        if (eventData.obj != null)
                //            eventPool.Release(eventData.obj);

                //        frontEventsToRemove++;
                //    }
                //    else
                //        break;
                //}

                //Main.Entry.Logger.Log("[d] remove events from front: " + frontEventsToRemove);

                // add level events object which is now shown to the right of viewport
                for (int i = levelEventsSortedByStartPosListEndIdx + 1; i < levelEventsDataSortedByStartPos.Count; i++)
                {
                    var data = levelEventsDataSortedByStartPos[i];
                    var startPosX = GetEventPosX(data.evt);
                    var endPosX = startPosX + GetEventObjWidth(data.evt);

                    if (endPosX < pos.x)
                    {
                        levelEventsSortedByStartPosListEndIdx = i;
                        continue;
                    }
                    else if (startPosX > pos.x + scrollRT.rect.width)
                        break;

                    //Main.Entry.Logger.Log("[d] adding event " + i);

                    var obj = CreateEventObject(data.evt, startPosX, endPosX - startPosX);
                    data.obj = obj;

                    levelEventsSortedByStartPosListEndIdx++;
                }

                //for (int i = 0; i < frontEventsToRemove; i++)
                //    currentlyShowingEvents.RemoveFirst();

                //firstEventShowingOnScreenIdx += frontEventsToRemove;
                //lastEventShowingOnScreenIdx = firstEventShowingOnScreenIdx + currentlyShowingEvents.Count - 1;

                //for (int i = lastEventShowingOnScreenIdx + 1; i < editor.events.Count; i++)
                //{
                //    var evt = editor.events[i];
                //    float posX = GetEventPosX(evt);
                //    float objWidth = GetEventObjWidth(evt);

                //    if (posX + objWidth < pos.x)
                //    {
                //        firstEventShowingOnScreenIdx = i + 1;
                //        lastEventShowingOnScreenIdx = firstEventShowingOnScreenIdx;
                //        continue;
                //    }
                //    if (posX > pos.x + scrollRT.rect.width)
                //        break;

                //    if (ignoreEvents.Contains(evt.eventType))
                //    {
                //        currentlyShowingEvents.AddLast(new LevelEventData(evt));
                //        lastEventShowingOnScreenIdx++;
                //        continue;
                //    }

                //    Main.Entry.Logger.Log("[d] adding event " + i);

                //    var obj = CreateEventObject(evt, posX, objWidth);
                //    currentlyShowingEvents.AddLast(new LevelEventData(evt, obj));
                //    lastEventShowingOnScreenIdx++;
                //}
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

                    vLines.AddFirst(CreateLineData(floor, i));

                    firstLineShowingOnScreenIdx--;
                }

                // ====

                for (int i = levelEventsSortedByStartPosListEndIdx; i >= 0; i--)
                {
                    var data = levelEventsDataSortedByStartPos[i];
                    float startPosX = GetEventPosX(data.evt);

                    if (startPosX <= pos.x + scrollRT.rect.width)
                        break;

                    //Main.Entry.Logger.Log("[d] removing event " + i + " from back side");

                    if (data.obj != null)
                    {
                        eventPool.Release(data.obj);
                        data.obj = null;
                    }

                    levelEventsSortedByStartPosListEndIdx--;
                }

                //int backEventsToRemove = 0;
                //foreach (var eventData in currentlyShowingEvents)
                //{
                //    float posX = GetEventPosX(eventData.evt);

                //    if (!eventData.objAvailable && posX > pos.x + scrollRT.rect.width)
                //    {
                //        backEventsToRemove++;
                //        continue;
                //    }

                //    float objWidth = GetEventObjWidth(eventData.evt);
                //    if (posX > pos.x + scrollRT.rect.width)
                //    {
                //        if (eventData.obj != null)
                //            eventPool.Release(eventData.obj);

                //        backEventsToRemove++;
                //    }
                //    else
                //        break;
                //}

                //Main.Entry.Logger.Log("[d] remove events from back: " + backEventsToRemove);

                for (int i = levelEventsSortedByEndPosListStartIdx - 1; i >= 0; i--)
                {
                    var data = levelEventsDataSortedByEndPos[i];
                    var startPosX = GetEventPosX(data.evt);
                    var endPosX = startPosX + GetEventObjWidth(data.evt);

                    if (startPosX > pos.x + scrollRT.rect.width)
                    {
                        levelEventsSortedByEndPosListStartIdx = i;
                        continue;
                    }
                    else if (endPosX < pos.x)
                        break;

                    //Main.Entry.Logger.Log("[d] adding event " + i);

                    var obj = CreateEventObject(data.evt, startPosX, endPosX - startPosX);
                    data.obj = obj;

                    levelEventsSortedByEndPosListStartIdx--;
                }

                //for (int i = 0; i < backEventsToRemove; i++)
                //    currentlyShowingEvents.RemoveLast();

                //lastEventShowingOnScreenIdx -= backEventsToRemove;
                //firstEventShowingOnScreenIdx = lastEventShowingOnScreenIdx - currentlyShowingEvents.Count + 1;

                //for (int i = firstEventShowingOnScreenIdx - 1; i >= 0; i--)
                //{
                //    var evt = editor.events[i];
                //    float posX = GetEventPosX(evt);
                //    float objWidth = GetEventObjWidth(evt);

                //    if (posX > pos.x + scrollRT.rect.width)
                //    {
                //        lastEventShowingOnScreenIdx = i - 1;
                //        firstEventShowingOnScreenIdx = lastEventShowingOnScreenIdx;
                //        continue;
                //    }
                //    if (posX + objWidth < pos.x)
                //        break;

                //    if (ignoreEvents.Contains(evt.eventType))
                //    {
                //        currentlyShowingEvents.AddFirst(new LevelEventData(evt));
                //        firstEventShowingOnScreenIdx--;
                //        continue;
                //    }

                //    Main.Entry.Logger.Log("[d] adding event " + i);

                //    var obj = CreateEventObject(evt, posX, objWidth);
                //    currentlyShowingEvents.AddFirst(new LevelEventData(evt, obj));
                //    firstEventShowingOnScreenIdx--;
                //}
            }

            //if (!changingScroll && dir.x != 0)
            //    followPlayhead = false;

            floorNumBar.LocalMoveX(-pos.x);

            prevScrollPos = pos;
            //Main.Entry.Logger.Log("Update Complete! cnt = " + vLines.Count + ", firstIdx = " + firstLineShowingOnScreenIdx + ", lastIdx = " + lastLineShowingOnScreenIdx);
            //Main.Entry.Logger.Log("events: startPosSortedIdx = " + levelEventsSortedByStartPosListEndIdx + ", endPosSortedIdx = " + levelEventsSortedByEndPosListStartIdx);
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
            num.transform.LocalMoveX(posX);
            num.text.text = floor.seqID.ToString();

            line.SetActive(true);
            num.gameObject.SetActive(true);
            return new VerticalLineData(floor.seqID, posX, line, num);
        }

        private GameObject CreateEventObject(LevelEvent levelEvent, float posX, float objWidth)
        {
            var obj = eventPool.Get();
            obj.transform.GetChild(0).GetComponent<Image>().sprite = GCS.levelEventIcons[
                levelEvent.eventType
            ];
            obj.GetComponent<TimelineEvent>().panel = this;

            obj.transform.LocalMoveX(posX);
            obj.GetComponent<RectTransform>().SizeDeltaX(objWidth);

            return obj;
        }

        private float GetLinePosX(scrFloor floor)
        {
            return TimeToBeat(floor.entryTime) * width * scale;
        }

        private float GetEventPosX(LevelEvent evt)
        {
            NeoEditor editor = NeoEditor.Instance;
            scrFloor floor = editor.floors[evt.floor];

            float position = TimeToBeat(floor.entryTime) * width * scale;

            object f;
            bool valueExist = evt.data.TryGetValue("angleOffset", out f);
            position += (valueExist ? (float)f : 0) / 180f * (1 / floor.speed) * width;

            return position;
        }

        private float GetEventDuration(LevelEvent evt)
        {
            NeoEditor editor = NeoEditor.Instance;
            scrFloor floor = editor.floors[evt.floor];

            object f;
            bool valueExist = evt.data.TryGetValue("duration", out f);
            float duration = valueExist ? (float)f * (1 / floor.speed) : 0;

            return duration;
        }

        private float GetEventObjWidth(LevelEvent evt)
        {
            float duration = GetEventDuration(evt);
            float objWidth = Mathf.Max(duration * width, height);

            return objWidth;
        }
        private float GetEventObjWidth(float duration)
        {
            float objWidth = Mathf.Max(duration * width, height);

            return objWidth;
        }
    }
}
