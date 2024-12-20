using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoEditor
{
    public class EditorControls : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public TextMeshProUGUI time;

        public Button prev;
        public Button next;
        public Button stop;
        public Button play;

        public Sprite playSprite;
        public Sprite pauseSprite;

        public GameObject speed;
        public Button speedDown;
        public Button speedUp;
        public TextMeshProUGUI speedText;

        public Button fullscreen;

        public Slider slider;

        private bool changingSlider;
        private bool isPointerHover;

        void Start()
        {
            NeoEditor editor = NeoEditor.Instance;
            prev.onClick.AddListener(() => editor.Skip(-5));
            next.onClick.AddListener(() => editor.Skip(5));
            stop.onClick.AddListener(() => editor.Rewind());
            play.onClick.AddListener(() => editor.PlayPause());

            speedDown.onClick.AddListener(() => ShiftSpeed(-1));
            speedUp.onClick.AddListener(() => ShiftSpeed(1));

            UpdatePercentText(Persistence.shortcutPlaySpeed);
            editor.SetPlaybackSpeed(Persistence.shortcutPlaySpeed);

            slider.onValueChanged.AddListener(SliderChangedByUser);
        }

        void Update()
        {
            NeoEditor editor = NeoEditor.Instance;

            play.image.sprite = editor.paused ? playSprite : pauseSprite;
            speed.SetActive(RDInput.holdingControl && editor.paused && isPointerHover);
            if (RDInput.holdingControl && editor.paused && isPointerHover)
            {
                Vector2 mouseScrollDelta = RDInput.mouseScrollDelta;
                if (Mathf.Abs(mouseScrollDelta.y) > 0.05f)
                {
                    if (mouseScrollDelta.y > 0f)
                    {
                        ShiftSpeed(1);
                    }
                    else if (mouseScrollDelta.y < 0f)
                    {
                        ShiftSpeed(-1);
                    }
                }
            }

            scrConductor conductor = scrConductor.instance;
            if (conductor.song.clip)
            {
                TimeSpan now = TimeSpan.FromSeconds(conductor.song.time);

                time.text =
                    $"<b>{now.Minutes}:{now.Seconds.ToString("00")}.{now.Milliseconds.ToString("000")}</b> <color=#ffffff64>{scrController.instance.currentSeqID}/{NeoEditor.Instance.floors.Count - 1}</color>";

                changingSlider = true;
                slider.value = conductor.song.time / conductor.song.clip.length;
                changingSlider = false;
            }
            else
            {
                time.text = "";
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerHover = false;
        }

        void ShiftSpeed(int direction)
        {
            int playSpeed = Persistence.shortcutPlaySpeed;
            int mul =
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 1 : 10;
            mul *= direction;
            playSpeed = Mathf.Clamp(playSpeed + mul, 1, 1000);
            Persistence.shortcutPlaySpeed = playSpeed;
            UpdatePercentText(playSpeed);
            NeoEditor.Instance.SetPlaybackSpeed(playSpeed);
        }

        private void UpdatePercentText(int speedPercent)
        {
            speedText.text = speedPercent.ToString() + "%";
        }

        private void SliderChangedByUser(float value)
        {
            if (changingSlider)
                return;
            scrConductor conductor = scrConductor.instance;
            NeoEditor editor = NeoEditor.Instance;
            if (conductor.song.clip)
            {
                float time = conductor.song.clip.length * value;
                int i = 0;
                foreach (var floor in editor.floors)
                {
                    if (floor.entryTime > time)
                        break;
                    i++;
                }
                editor.SkipTo(i - 1);
            }
        }
    }
}
