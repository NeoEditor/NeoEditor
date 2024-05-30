using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor
{
    public class EditorControls : MonoBehaviour
    {
        public TextMeshProUGUI time;

        public Button prev;
        public Button next;
        public Button stop;
        public Button play;

        public GameObject speed;
        public Button speedDown;
        public Button speedUp;
        public TextMeshProUGUI speedText;

        public Button fullscreen;

        public Slider slider;

        private bool changingSlider;

        //Slider

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
#if UNITY_EDITOR
            return;
#endif
            NeoEditor editor = NeoEditor.Instance;

            speed.SetActive(RDInput.holdingControl && editor.paused);
            if (RDInput.holdingControl && editor.paused)
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
            //TODO: Update time and slider.
            scrConductor conductor = scrConductor.instance;
            if (conductor.song.clip)
            {
                TimeSpan now = TimeSpan.FromSeconds(conductor.song.time);

                time.text =
                    $"{now.Minutes}:{now.Seconds.ToString("00")}.{now.Milliseconds.ToString("000")}\t({scrController.instance.currentSeqID}/{NeoEditor.Instance.floors.Count - 1})";

                changingSlider = true;
                slider.value = conductor.song.time / conductor.song.clip.length;
                changingSlider = false;
            }
            else
            {
                time.text = "";
            }
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
