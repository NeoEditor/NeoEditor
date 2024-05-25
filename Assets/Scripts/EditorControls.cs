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

        public Button speedDown;
        public Button speedUp;
        public TextMeshProUGUI speed;

        public Button fullscreen;

        //Slider

        void Start()
        {
            NeoEditor editor = NeoEditor.Instance;
            //prev.onClick.AddListener(() => editor.Skip(-5));
            //next.onClick.AddListener(() => editor.Skip(5));
            stop.onClick.AddListener(() => editor.Pause());
            play.onClick.AddListener(() => editor.Play());
        }

        void Update()
        {
            NeoEditor editor = NeoEditor.Instance;
            //TODO: Update time and slider.
        }
    }
}
