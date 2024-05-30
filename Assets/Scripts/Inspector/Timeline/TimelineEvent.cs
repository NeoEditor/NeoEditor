using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Timeline
{
    public class TimelineEvent : MonoBehaviour
    {
        public TimelinePanel panel;
        public Button button;

        public LevelEvent targetEvent;

        void Start()
        {
            button.onClick.AddListener(() =>
            {
                button.interactable = false;
                panel.SelectEvent(this);
            });
        }

        void Update() { }

        public void UnselectEvent()
        {
            button.interactable = true;
        }
    }
}
