using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using NeoEditor.Inspector;
using NeoEditor.Inspector.Media;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Tabs
{
    public class ProjectTab : TabBase
    {
        public ProjectPanel project;
        public MediaPanel media;
        public RawImage gameView;

        public override void InitTab(Dictionary<string, LevelEventInfo> levelEventsInfo)
        {
            project.Init(
                levelEventsInfo["SongSettings"],
                levelEventsInfo["LevelSettings"],
                levelEventsInfo["MiscSettings"]
            );
            gameView.texture = Assets.GameRenderer;
            OnOpenLevel();
        }

        public override void OnOpenLevel()
        {
            NeoEditor editor = NeoEditor.Instance;
            project.SetProperties(
                editor.levelData.songSettings,
                editor.levelData.levelSettings,
                editor.levelData.miscSettings
            );
            project.SelectTab(0);

            media.SetupItems(editor);
        }
    }
}
