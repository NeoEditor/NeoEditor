using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ADOFAI;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NeoEditor.Inspector.Controls
{
    public class Control_File : ControlBase
    {
        private enum FileFormat
        {
            File,
            Audio,
            Image
        }

        public TMP_InputField inputField;
        public Button button;
        public Image buttonIcon;
        private string filename;
        public UnityEvent onFileChange;

        public override string text
        {
            get => inputField.text;
            set
            {
                inputField.text = value;
                filename = value;
            }
        }

        public override List<Selectable> selectables => new List<Selectable> { inputField, button };

        // Token: 0x06001FDB RID: 8155 RVA: 0x000E4653 File Offset: 0x000E2853
        private void Awake()
        {
            button.onClick.AddListener(
                delegate()
                {
                    BrowseFile();
                }
            );
            inputField.onEndEdit.AddListener(
                delegate(string s)
                {
                    if (CheckIfLevelIsSaved())
                    {
                        ProcessFile(s, propertyInfo.fileType);
                    }
                }
            );
        }

        // Token: 0x06001FDC RID: 8156 RVA: 0x000E4690 File Offset: 0x000E2890
        private bool CheckIfLevelIsSaved()
        {
            if (string.IsNullOrEmpty(ADOBase.levelPath))
            {
                string popupType = "SaveBeforeSongImport";
                switch (propertyInfo.fileType)
                {
                    case FileType.Audio:
                        popupType = "SaveBeforeSongImport";
                        break;
                    case FileType.Image:
                        popupType = "SaveBeforeImageImport";
                        break;
                    case FileType.Video:
                        popupType = "SaveBeforeVideoImport";
                        break;
                }
                NeoEditor.Instance.ShowPopup(true, popupType);
                return false;
            }
            return true;
        }

        // Token: 0x06001FDD RID: 8157 RVA: 0x000E46E8 File Offset: 0x000E28E8
        private void BrowseFile()
        {
            if (!CheckIfLevelIsSaved())
            {
                return;
            }
            string newFilename = null;
            FileType fileType = propertyInfo.fileType;
            if (fileType == FileType.Audio)
            {
                newFilename = RDEditorUtils.ShowFileSelectorForAudio(
                    RDString.Get("editor.dialog.selectSound", null, LangSection.Translations),
                    -1L
                );
            }
            else if (fileType == FileType.Image)
            {
                newFilename = RDEditorUtils.ShowFileSelectorForImage(
                    RDString.Get("editor.dialog.selectImage", null, LangSection.Translations),
                    -1L
                );
            }
            else if (fileType == FileType.Video)
            {
                newFilename = RDEditorUtils.ShowFileSelectorForVideo(
                    RDString.Get("editor.dialog.selectVideo", null, LangSection.Translations),
                    -1L
                );
            }
            ProcessFile(newFilename, fileType);
        }

        // Token: 0x06001FDE RID: 8158 RVA: 0x000E4760 File Offset: 0x000E2960
        private void ProcessFile(string newFilename, FileType fileType)
        {
            NeoEditor editor = NeoEditor.Instance;
            if (newFilename == null)
            {
                return;
            }
            if (filename == newFilename)
            {
                return;
            }
            if (newFilename != "")
            {
                File.Exists(Path.Combine(Path.GetDirectoryName(ADOBase.levelPath), newFilename));
            }
            if (fileType != FileType.Audio)
            {
                if (fileType == FileType.Image)
                {
                    using (new SaveStateScope(editor, false, true, false))
                    {
                        LevelEvent selectedEvent = inspectorPanel.selectedEvent;
                        filename = newFilename;
                        selectedEvent[propertyInfo.name] = filename;
                        inputField.text = filename;
                        base.ToggleOthersEnabled();
                        if (selectedEvent.eventType == LevelEventType.BackgroundSettings)
                        {
                            ADOBase.customLevel.SetBackground();
                            return;
                        }
                        if (selectedEvent.eventType == LevelEventType.AddDecoration)
                        {
                            editor.UpdateDecorationObject(selectedEvent);
                            return;
                        }
                        if (selectedEvent.eventType == LevelEventType.MoveDecorations)
                        {
                            object obj;
                            if (selectedEvent.data.TryGetValue("decorationImage", out obj))
                            {
                                string text = obj as string;
                                if (text != null && !string.IsNullOrEmpty(text))
                                {
                                    string filePath = Path.Combine(
                                        Path.GetDirectoryName(ADOBase.levelPath),
                                        text
                                    );
                                    LoadResult loadResult;
                                    ADOBase.customLevel.imgHolder.AddSprite(
                                        text,
                                        filePath,
                                        out loadResult
                                    );
                                    //editor.UpdateImageLoadResult(text, loadResult);
                                }
                            }
                        }
                        else if (
                            propertyInfo.affectsFloors
                            || selectedEvent.eventType == LevelEventType.ColorTrack
                        )
                        {
                            ADOBase.customLevel.UpdateFloorSprites();
                            //editor.ApplyEventsToFloors();
                        }
                        else
                        {
                            ADOBase.customLevel.UpdateBackgroundSprites();
                        }
                        return;
                    }
                }
                if (fileType == FileType.Video)
                {
                    LevelEvent selectedEvent2 = inspectorPanel.selectedEvent;
                    filename = newFilename;
                    selectedEvent2[propertyInfo.name] = filename;
                    inputField.text = filename;
                    VideoPlayer videoBG = ADOBase.customLevel.videoBG;
                    videoBG.gameObject.SetActive(true);
                    videoBG.url = Path.Combine(
                        Path.GetDirectoryName(ADOBase.levelPath),
                        editor.levelData.miscSettings.data["bgVideo"].ToString()
                    );
                    videoBG.Stop();
                    videoBG.Prepare();
                    base.ToggleOthersEnabled();
                }
                return;
            }
            LevelEvent selectedEvent3 = inspectorPanel.selectedEvent;
            filename = newFilename;
            base.ToggleOthersEnabled();
            if (Path.GetExtension(filename).Replace(".", string.Empty) == "mp3")
            {
                //editor.songToConvert = filename;
                //editor.ShowPopup(true, scnEditor.PopupType.OggEncode, false);
                return;
            }
            selectedEvent3[propertyInfo.name] = filename;
            inputField.text = filename;
            //editor.UpdateSongAndLevelSettings();
        }

        // Token: 0x06001FDF RID: 8159 RVA: 0x000E4A48 File Offset: 0x000E2C48
        private void Update()
        {
            button.interactable = inputField.interactable;
            Color color = Color.white.WithAlpha(inputField.interactable ? 1f : 0.4f);
            buttonIcon.color = color;
        }

        // Token: 0x06001FE0 RID: 8160 RVA: 0x000E4AB4 File Offset: 0x000E2CB4
        public override void OnRightClick()
        {
            NeoEditor editor = NeoEditor.Instance;
            LevelEvent selectedEvent = inspectorPanel.selectedEvent;
            if (string.IsNullOrEmpty(selectedEvent[propertyInfo.name] as string))
            {
                return;
            }
            using (new SaveStateScope(editor, false, true, false))
            {
                filename = "";
                selectedEvent[propertyInfo.name] = filename;
                inputField.text = filename;
                base.ToggleOthersEnabled();
                FileType fileType = propertyInfo.fileType;
                if (propertyInfo.fileType == FileType.Audio)
                {
                    editor.levelData.songFilename = filename;
                    //editor.UpdateSongAndLevelSettings();
                }
                else if (propertyInfo.fileType == FileType.Image)
                {
                    if (selectedEvent.eventType == LevelEventType.BackgroundSettings)
                    {
                        ADOBase.customLevel.SetBackground();
                    }
                    else if (selectedEvent.eventType == LevelEventType.AddDecoration)
                    {
                        editor.UpdateDecorationObject(selectedEvent);
                    }
                    else
                    {
                        ADOBase.customLevel.UpdateBackgroundSprites();
                    }
                }
                else if (propertyInfo.fileType == FileType.Video)
                {
                    VideoPlayer videoBG = ADOBase.customLevel.videoBG;
                    videoBG.Stop();
                    videoBG.gameObject.SetActive(false);
                }
            }
        }
    }
}
