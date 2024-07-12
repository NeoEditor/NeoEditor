using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using NeoEditor.Tabs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Timeline
{
    public class TimelinePanel : MonoBehaviour, IPointerClickHandler
    {
        public EffectTabBase parentTab;

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

        private TimelineEvent selectedEvent;

        private ObjectPool<GameObject> hPool;
        private ObjectPool<GameObject> vPool;
        private ObjectPool<GameObject> eventPool;
        private ObjectPool<FloorNumberText> floorNumPool;

        private LinkedList<VerticalLineData> vLines = new LinkedList<VerticalLineData>();

        private List<LevelEventData> levelEventsDataSortedByStartPos = new List<LevelEventData>();
        private List<LevelEventData> levelEventsDataSortedByEndPos = new List<LevelEventData>();

        // level events sorted by startPosX, index of last item showing on viewport (-1 = before first item showing on viewport)
        private int levelEventsSortedByStartPosListEndIdx = -1;

        // level events sorted by endPosX, index of first item showing on viewport
        private int levelEventsSortedByEndPosListStartIdx = 0;

        private int selectingEventFloor = -1;
        private LevelEventData selectingTargetEvent;

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

            float scrollWidth = scrollRT.rect.width;
            float scrollHeight = scrollRT.rect.height;

            // release all loaded objects

            Main.Entry.Logger.Log("[d] TimelinePanel#Init() called. Releasing objects to pool...");

            foreach (var line in vLines)
            {
                // verticalLine obj and floorNum obj can be null
                // when floor is midspin
                if (line.obj != null)
                {
                    vPool.Release(line.obj);
                }
                if (line.num != null)
                {
                    floorNumPool.Release(line.num);
                }
            }
            vLines.Clear();

            foreach (var eventData in levelEventsDataSortedByStartPos)
            {
                if (eventData.obj != null)
                {
                    eventData.obj.GetComponent<TimelineEvent>().isRendering = false;
                    eventPool.Release(eventData.obj);

                    eventData.obj = null;
                }
            }

            //Main.Entry.Logger.Log(string.Format("[d] eventPool CountAll: {0} active: {1} inactive: {2}", eventPool.CountAll, eventPool.CountActive, eventPool.CountInactive));

            levelEventsDataSortedByStartPos.Clear();
            levelEventsDataSortedByEndPos.Clear();

            levelEventsSortedByStartPosListEndIdx = -1;
            levelEventsSortedByEndPosListStartIdx = 0;

            // release temporary levelevent object
            if (selectingTargetEvent != null)
            {
                eventPool.Release(selectingTargetEvent.obj);
                selectingTargetEvent.obj = null;
            }

            selectingTargetEvent = null;
            selectingEventFloor = -1;

            // loop level floors(tiles) and register verticalLine

            firstLineShowingOnScreenIdx = -1;
            lastLineShowingOnScreenIdx = -1;

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

                    lastLineShowingOnScreenIdx++;
                }
                if (lastLineShowingOnScreenIdx >= 0)
                    firstLineShowingOnScreenIdx = 0;
            }

            // last LevelEvent's end time on each timeline row
            // used to calculate the optimal row for next LevelEvent
            List<float> timelineRowEndPosX = new List<float>();

            // loop level floors(tiles) and register level events

            foreach (var levelEvent in editor.events)
            {
                // get position
                Vector2 position = new Vector2(GetEventPosX(levelEvent), -25);

                if (NeoConstants.TimelineIgnoreEvents.Contains(levelEvent.eventType))
                    continue;

                float entryTime = (float)floors[levelEvent.floor].entryTime;
                float duration = GetEventDuration(levelEvent);
                float objWidth = GetEventObjWidth(duration);

                // get the optimal timeline row
                int optimalRow = -1;
                float eventEndPosX = position.x + objWidth;
                for (int i = 0; i < timelineRowEndPosX.Count; i++)
                {
                    // Main.Entry.Logger.Log(string.Format("i: {0} endPosX: {1} curPosX: {2}", i, timelineRowEndPosX[i], position.x));
                    if (timelineRowEndPosX[i] < position.x)
                    {
                        optimalRow = i;
                        timelineRowEndPosX[i] = eventEndPosX;
                        break;
                    }
                }
                if (optimalRow < 0)
                {
                    // no optimal row found, create a new row and place it
                    optimalRow = timelineRowEndPosX.Count;
                    timelineRowEndPosX.Add(eventEndPosX);
                }

                Main.Entry.Logger.Log(string.Format("floor {0}: {1} {2} {3} // row: {4}", levelEvent.floor, levelEvent.eventType, position.x, eventEndPosX, optimalRow));

                var eventData = new LevelEventData(entryTime, duration, optimalRow, levelEvent);
                levelEventsDataSortedByStartPos.Add(eventData);
                levelEventsDataSortedByEndPos.Add(eventData);

                if (position.x <= scrollWidth)
                {
                    //Main.Entry.Logger.Log("[d] (init phase) adding event | floor " + levelEvent.floor + " type: " + levelEvent.eventType);

                    var obj = CreateEventObject(levelEvent, position.x, eventData.timelineRow, objWidth);

                    eventData.obj = obj;
                    levelEventsSortedByStartPosListEndIdx++;
                }
            }

            //Main.Entry.Logger.Log("[d] [init] events: startPosSortedIdx = " + levelEventsSortedByStartPosListEndIdx + ", endPosSortedIdx = " + levelEventsSortedByEndPosListStartIdx);

            levelEventsDataSortedByStartPos.Sort(
                (a, b) =>
                {
                    // sort by event start position x, smaller one goes first
                    var diff = GetEventPosX(a.evt) - GetEventPosX(b.evt);
                    if (diff < 0)
                        return -1;
                    else if (diff > 0)
                        return 1;
                    else
                        return 0;
                }
            );

            levelEventsDataSortedByEndPos.Sort(
                (a, b) =>
                {
                    var aStartPosX = GetEventPosX(a.evt);
                    var aObjWidth = GetEventObjWidth(a.evt);
                    var aEndPosX = aStartPosX + aObjWidth;

                    var bStartPosX = GetEventPosX(b.evt);
                    var bObjWidth = GetEventObjWidth(b.evt);
                    var bEndPosX = bStartPosX + bObjWidth;

                    // sort by event end position x, smaller one goes first
                    float diff = aEndPosX - bEndPosX;
                    if (diff < 0)
                        return -1;
                    else if (diff > 0)
                        return 1;
                    else
                        return 0;
                }
            );

            scrConductor conductor = scrConductor.instance;
            float timelineWidth =
                TimeToBeat(floors.Last().entryTime + conductor.crotchet * 4) * width;

            content.GetComponent<RectTransform>().SizeDeltaX(timelineWidth);
            content.GetComponent<RectTransform>().SizeDeltaY((timelineRowEndPosX.Count + 2) * height);

            floorNumBar.GetComponent<RectTransform>().SizeDeltaX(timelineWidth);

            prevScrollPos = Vector2.zero;
            // move scroll position after initialization completed ( Init() then OnValueChanged() )
            scroll.content.anchoredPosition = Vector2.zero;
        }

        public void SelectEvent(TimelineEvent timelineEvent)
        {
            if (selectedEvent != null)
            {
                selectedEvent.UnselectEvent();
                parentTab.UnselectEvent();
            }
            selectedEvent = timelineEvent;
            parentTab.SelectEvent(selectedEvent.targetEvent);
        }

        public void SetParent(RectTransform transform)
        {
            this.transform.SetParent(transform, false);
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

            //Main.Entry.Logger.Log("[d] [OnValueChanged] (before) events: startPosSortedIdx = " + levelEventsSortedByStartPosListEndIdx + ", endPosSortedIdx = " + levelEventsSortedByEndPosListStartIdx);

            // prevScrollPos < (current) pos
            // scrolled to the right
            if (dir.x < 0)
            {
                int frontLinesToRemove = 0;
                foreach (var line in vLines)
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
                    else
                        break;
                }

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

                    vLines.AddLast(CreateLineData(floor, i));

                    lastLineShowingOnScreenIdx++;
                }

                // ====

                // remove level events object which is completely hidden to the left of viewport
                for (
                    int i = levelEventsSortedByEndPosListStartIdx;
                    i < levelEventsDataSortedByEndPos.Count;
                    i++
                )
                {
                    var data = levelEventsDataSortedByEndPos[i];
                    float startPosX = GetEventPosX(data.evt);
                    float endPosX = startPosX + GetEventObjWidth(data.evt);

                    if (startPosX > prevScrollPos.x + scrollRT.rect.width)
                    {
                        levelEventsSortedByEndPosListStartIdx++;
                        continue;
                    }
                    if (endPosX >= pos.x)
                        break;

                    //Main.Entry.Logger.Log("[d] removing event " + i + " from front side");
                    data.obj.GetComponent<TimelineEvent>().isRendering = false;
                    if (data.obj != null)
                    {
                        eventPool.Release(data.obj);
                        data.obj = null;
                    }

                    levelEventsSortedByEndPosListStartIdx++;
                }

                // add level events object which is now shown to the right of viewport
                for (
                    int i = levelEventsSortedByStartPosListEndIdx + 1;
                    i < levelEventsDataSortedByStartPos.Count;
                    i++
                )
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

                    //Main.Entry.Logger.Log("[d] adding event " + i + " to the back side");

                    var obj = CreateEventObject(data.evt, startPosX, data.timelineRow, endPosX - startPosX);
                    data.obj = obj;

                    levelEventsSortedByStartPosListEndIdx++;
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
                    float endPosX = startPosX + GetEventObjWidth(data.evt);

                    if (endPosX < prevScrollPos.x)
                    {
                        levelEventsSortedByStartPosListEndIdx--;
                        continue;
                    }
                    if (startPosX <= pos.x + scrollRT.rect.width)
                        break;

                    //Main.Entry.Logger.Log("[d] removing event " + i + " from back side");
                    data.obj.GetComponent<TimelineEvent>().isRendering = false;
                    if (data.obj != null)
                    {
                        eventPool.Release(data.obj);
                        data.obj = null;
                    }

                    levelEventsSortedByStartPosListEndIdx--;
                }

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

                    //Main.Entry.Logger.Log("[d] adding event " + i + " to the front side");

                    var obj = CreateEventObject(data.evt, startPosX, data.timelineRow, endPosX - startPosX);
                    data.obj = obj;

                    levelEventsSortedByEndPosListStartIdx--;
                }
            }

            //if (!changingScroll && dir.x != 0)
            //    followPlayhead = false;

            floorNumBar.LocalMoveX(-pos.x);

            prevScrollPos = pos;
            //Main.Entry.Logger.Log("Update Complete! cnt = " + vLines.Count + ", firstIdx = " + firstLineShowingOnScreenIdx + ", lastIdx = " + lastLineShowingOnScreenIdx);
            //Main.Entry.Logger.Log("[d] [OnValueChanged] (after) events: startPosSortedIdx = " + levelEventsSortedByStartPosListEndIdx + ", endPosSortedIdx = " + levelEventsSortedByEndPosListStartIdx);
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

        private GameObject CreateEventObject(LevelEvent levelEvent, float posX, int timelineRow, float objWidth)
        {
            var obj = eventPool.Get();
            if (levelEvent == null)
                obj.transform.GetChild(0).gameObject.SetActive(false);
            else
                obj.transform.GetChild(0).GetComponent<Image>().sprite = GCS.levelEventIcons[levelEvent.eventType];
            var timelineEvent = obj.GetComponent<TimelineEvent>();
            timelineEvent.panel = this;
            timelineEvent.targetEvent = levelEvent;
            timelineEvent.isRendering = true;
            timelineEvent.button.interactable = levelEvent == null ? false : (selectedEvent?.targetEvent) != levelEvent;

            obj.transform.LocalMoveX(posX);
            obj.transform.LocalMoveY(-timelineRow * height);
            obj.GetComponent<RectTransform>().SizeDeltaX(objWidth);

            //Main.Entry.Logger.Log(string.Format("[d] CreateEventObject: floor {0} {1}, posX: {2}", levelEvent.floor, levelEvent.eventType, posX));
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

        public void UpdateSelectedEventPos(int seqID)
        {
            if (selectedEvent == null || !selectedEvent.isRendering)
                return;
            NeoEditor editor = NeoEditor.Instance;
            scrFloor floor = editor.floors[seqID];

            float position = TimeToBeat(floor.entryTime) * width * scale;

            object f;
            bool valueExist = selectedEvent.targetEvent.data.TryGetValue("angleOffset", out f);
            position += (valueExist ? (float)f : 0) / 180f * (1 / floor.speed) * width;
            selectedEvent.transform.LocalMoveX(position);
        }

        public void UpdateSelectedEventPos(float angleOffset)
        {
            if (selectedEvent == null || !selectedEvent.isRendering)
                return;
            NeoEditor editor = NeoEditor.Instance;
            scrFloor floor = editor.floors[selectedEvent.targetEvent.floor];

            float position = TimeToBeat(floor.entryTime) * width * scale;

            position += angleOffset / 180f * (1 / floor.speed) * width;
            selectedEvent.transform.LocalMoveX(position);
        }

        public void AddNewEventObject(int floor, float x, int row, float entryTime)
        {
            float objWidth = height * scale;

			var eventData = new LevelEventData(entryTime, 0f, row, null);

			var obj = CreateEventObject(null, x, eventData.timelineRow, objWidth);

            eventData.obj = obj;

            if (selectingTargetEvent != null)
            {
                eventPool.Release(selectingTargetEvent.obj);
                selectingTargetEvent.obj = null;
            }

            selectingEventFloor = floor;
            selectingTargetEvent = eventData;
            parentTab.SetEventSelector();
		}

		public void ApplySelector(LevelEventType type)
		{
            if (selectingEventFloor < 0) return;
            NeoEditor editor = NeoEditor.Instance;
            var levelEvent = editor.AddEvent(selectingEventFloor, type);

            // craete new LevelEventData and copy gameobject
            var eventData = new LevelEventData(selectingTargetEvent.start, 0f, selectingTargetEvent.timelineRow, levelEvent);
            eventData.obj = selectingTargetEvent.obj;

			levelEventsDataSortedByStartPos.Add(eventData);
			levelEventsDataSortedByEndPos.Add(eventData);

			levelEventsDataSortedByStartPos.InsertionSort(
				(a, b) =>
				{
					// sort by event start position x, smaller one goes first
					var diff = GetEventPosX(a.evt) - GetEventPosX(b.evt);
					if (diff < 0)
						return -1;
					else if (diff > 0)
						return 1;
					else
						return 0;
				}
			);

			levelEventsDataSortedByEndPos.InsertionSort(
				(a, b) =>
				{
					var aStartPosX = GetEventPosX(a.evt);
					var aObjWidth = GetEventObjWidth(a.evt);
					var aEndPosX = aStartPosX + aObjWidth;

					var bStartPosX = GetEventPosX(b.evt);
					var bObjWidth = GetEventObjWidth(b.evt);
					var bEndPosX = bStartPosX + bObjWidth;

					// sort by event end position x, smaller one goes first
					float diff = aEndPosX - bEndPosX;
					if (diff < 0)
						return -1;
					else if (diff > 0)
						return 1;
					else
						return 0;
				}
			);
			levelEventsSortedByStartPosListEndIdx++;

            selectingEventFloor = -1;
            selectingTargetEvent = null;

            parentTab.SelectEvent(levelEvent);
		}

		public void OnPointerClick(PointerEventData eventData)
        {
            NeoEditor editor = NeoEditor.Instance;
            var floors = editor.floors;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                content,
                eventData.position,
                null,
                out localPos
            );

            //find floor
            float posX = -1f;
            int floor = -1;
            VerticalLineData prevLine = vLines.First.Value;
            foreach (VerticalLineData line in vLines)
            {
                if (localPos.x < line.x)
                {
                    posX = prevLine.x;
                    floor = prevLine.id;
                    break;
                }
                prevLine = line;
            }

            if (floor == -1)
                return;

            int posY = Mathf.FloorToInt(-localPos.y / (height * scale));

            AddNewEventObject(floor, posX, posY, (float)floors[floor].entryTime);
        }
	}
}
