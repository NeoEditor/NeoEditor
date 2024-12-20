using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI.Editor;
using DynamicPanels;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace NeoEditor.Menu
{
    public class MenuBar : ADOBase
    {
        public MenuButton menuBarButtonTemplate;
        public MenuButton menuButtonTemplate;
        public MenuButton separatorTemplate;
        public MenuContent menuTemplate;

        public RectTransform menubar;

        private List<MenuButton> menus = new();
        private List<MenuContent> contents = new List<MenuContent>();

        public void AddMenu(MenuItem root)
        {
            root.parent = null;
            MenuButton menu = Instantiate(menuBarButtonTemplate, menubar);
            menu.text.text = root.text;
            menu.info = root;
            menu.gameObject.SetActive(true);

            var content = Instantiate(menuTemplate, menu.rect);
            content.item = root;
            foreach (MenuItem sub in root.subMenus)
            {
                AddSubMenu(sub, content, 1);
            }

            menu.button.onClick.AddListener(() => ShowMenu(content));

            menus.Add(menu);
            contents.Add(content);
        }

        public void AddSubMenu(MenuItem menu, MenuContent content, int depth)
        {
            MenuButton button;
            if (menu is SeparatorMenuItem)
            {
                button = Instantiate(separatorTemplate, content.rect);
                button.gameObject.SetActive(true);
            }
            else
            {
                button = Instantiate(menuButtonTemplate, content.rect);
                button.text.text = menu.text;
                button.shortcut.text = menu.shortcutText;
                button.checkbox.gameObject.SetActive(menu is ToggleMenuItem);
                button.gameObject.SetActive(true);
                if (menu is EntryMenuItem)
                {
                    var subContent = Instantiate(menuTemplate, button.rect);
                    subContent.rect.anchoredPosition = new Vector2(298f, 2);
                    subContent.parents = content.parents.ToList();
                    subContent.parents.Add(content);
                    subContent.canvas.sortingOrder += depth;
                    subContent.item = menu;
                    button.button.onClick.AddListener(() => ShowMenu(subContent));
                    contents.Add(subContent);
                    foreach (MenuItem sub in menu.subMenus)
                    {
                        AddSubMenu(sub, subContent, depth + 1);
                    }
                }
                else if (menu is ActionMenuItem)
                {
                    button.button.onClick.AddListener(() => (menu as ActionMenuItem).action());
                }
                else if (menu is ToggleMenuItem)
                {
                    button.button.onClick.AddListener(
                        () =>
                        {
                            (menu as ToggleMenuItem).action(!button.isChecked);
                            button.isChecked = !button.isChecked;
                        }
                    );
                }
            }

            content.childs.Add(button);
        }

        public void ShowMenu(MenuContent content)
        {
            foreach (var c in contents)
            {
                if (!content.parents.Contains(c))
                    c.gameObject.SetActive(false);
            }
            content.gameObject.SetActive(true);

			for (int i = 0; i < content.item.subMenus.Count; i++)
            {
				MenuItem menu = content.item.subMenus[i];
                var menuButton = content.childs[i];
                menuButton.SetEnabled(menu.onActive == null || menu.onActive(menuButton));
            }
        }

        public void CloseMenu()
        {
			foreach (var content in contents)
			{
                content.gameObject.SetActive(false);
			}
		}

        void Start()
        {
            NeoEditor editor = NeoEditor.Instance;

            var file = new MenuItem("File");
            file.AddSubMenu(new ActionMenuItem("New", new EditorKeybind(KeyModifier.Control, KeyCode.N), () => { }));
            file.AddSubMenu(new SeparatorMenuItem());
			file.AddSubMenu(new ActionMenuItem("Open", new EditorKeybind(KeyModifier.Control, KeyCode.O), () =>
			{
				editor.OpenLevel();
			}));
            file.AddSubMenu(new ActionMenuItem("Open Recent",
                new EditorKeybind(KeyModifier.Control | KeyModifier.Shift, KeyCode.O), () => { }));
            file.AddSubMenu(new SeparatorMenuItem());
			file.AddSubMenu(new ActionMenuItem("Save", new EditorKeybind(KeyModifier.Control, KeyCode.S), () =>
			{
				editor.SaveLevel();
			}));
			file.AddSubMenu(new ActionMenuItem("Save As",
                new EditorKeybind(KeyModifier.Control | KeyModifier.Shift, KeyCode.S), () =>
			{
				editor.SaveLevelAs();
			}));
			file.AddSubMenu(new SeparatorMenuItem());
            file.AddSubMenu(new ActionMenuItem("Help", new EditorKeybind(KeyModifier.Control, KeyCode.H), () => { }));
            file.AddSubMenu(new ActionMenuItem("Preference", 
                new EditorKeybind(KeyModifier.Control | KeyModifier.Shift, KeyCode.I), () => { }));
            file.AddSubMenu(new ActionMenuItem("Exit", new EditorKeybind(KeyModifier.Control, KeyCode.Q), editor.TryQuit));
            AddMenu(file);

            var edit = new MenuItem("Edit");
            edit.AddSubMenu(new ActionMenuItem("Undo", new EditorKeybind(KeyModifier.Control, KeyCode.Z), () => { editor.Undo(); }));
            edit.AddSubMenu(new ActionMenuItem("Redo", 
                new EditorKeybind(KeyModifier.Control | KeyModifier.Shift, KeyCode.Z), () => { editor.Redo(); }));
            edit.AddSubMenu(new SeparatorMenuItem());
            edit.AddSubMenu(new ActionMenuItem("Cut", new EditorKeybind(KeyModifier.Control, KeyCode.X), () => { }));
            edit.AddSubMenu(new ActionMenuItem("Copy", new EditorKeybind(KeyModifier.Control, KeyCode.C), () => { }));
            edit.AddSubMenu(new ActionMenuItem("Paste", new EditorKeybind(KeyModifier.Control, KeyCode.V), () => { }));
            edit.AddSubMenu(new ActionMenuItem("Delete", new EditorKeybind(KeyModifier.None, KeyCode.Delete), () => { }));
            edit.AddSubMenu(new SeparatorMenuItem());
            var find = edit.AddSubMenu(new EntryMenuItem("Find"));
            find.AddSubMenu(new ActionMenuItem("Floor", new EditorKeybind(KeyModifier.Control, KeyCode.F), () => { }));
            find.AddSubMenu(new ActionMenuItem("Event", new EditorKeybind(KeyModifier.Control | KeyModifier.Shift, KeyCode.F), () => { }));
            find.AddSubMenu(new ActionMenuItem("Decoration", new EditorKeybind(KeyModifier.Control | KeyModifier.Alt, KeyCode.F), () => { }));
            var replace = edit.AddSubMenu(new EntryMenuItem("Replace"));
            replace.AddSubMenu(new ActionMenuItem("Event", new EditorKeybind(KeyModifier.Control | KeyModifier.Shift, KeyCode.H), () => { }));
            replace.AddSubMenu(new ActionMenuItem("Decoration", new EditorKeybind(KeyModifier.Control | KeyModifier.Alt, KeyCode.H), () => { }));
            edit.AddSubMenu(new SeparatorMenuItem());
            edit.AddSubMenu(new ActionMenuItem("Select All", new EditorKeybind(KeyModifier.Control, KeyCode.A), () => { }));
            AddMenu(edit);

            var panel = new MenuItem("Panel");
			panel.AddSubMenu(new ToggleMenuItem("Preview Panel", new EditorKeybind(), (on) =>
			{
				if (on)
				{
					editor.panelTabs["Game"].Panel.gameObject.SetActive(true);
				}
				else
				{
					editor.panelTabs["Game"].Detach()?.gameObject.SetActive(false);
				}
			}, (obj) =>
			{
				obj.isChecked = editor.panelTabs["Game"].Panel.gameObject.activeInHierarchy;
				return true;
			}
			));
			panel.AddSubMenu(new ToggleMenuItem("Scene Panel", new EditorKeybind(), (on) =>
			{
				if (on)
				{
					editor.panelTabs["Scene"].Panel.gameObject.SetActive(true);
				}
				else
				{
					editor.panelTabs["Scene"].Detach()?.gameObject.SetActive(false);
				}
			}, (obj) =>
			{
				obj.isChecked = editor.panelTabs["Scene"].Panel.gameObject.activeInHierarchy;
				return true;
			}
			));
			panel.AddSubMenu(new ToggleMenuItem("Inspector Panel", new EditorKeybind(), (on) =>
			{
				if (on)
				{
					editor.panelTabs["Inspector"].Panel.gameObject.SetActive(true);
				}
				else
				{
					editor.panelTabs["Inspector"].Detach()?.gameObject.SetActive(false);
				}
			}, (obj) =>
			{
				obj.isChecked = editor.panelTabs["Inspector"].Panel.gameObject.activeInHierarchy;
				return true;
			}
			));
			panel.AddSubMenu(new ToggleMenuItem("Decorations Panel", new EditorKeybind(), (on) =>
			{
				if (on)
				{
					editor.panelTabs["Decorations"].Panel.gameObject.SetActive(true);
				}
				else
				{
					editor.panelTabs["Decorations"].Detach()?.gameObject.SetActive(false);
				}
			}, (obj) =>
			{
				obj.isChecked = editor.panelTabs["Decorations"].Panel.gameObject.activeInHierarchy;
				return true;
			}
			));
			panel.AddSubMenu(new ToggleMenuItem("Project Panel", new EditorKeybind(), (on) =>
            {
                if (on)
                {
                    editor.panelTabs["Project"].Panel.gameObject.SetActive(true);
                }
                else
                {
                    editor.panelTabs["Project"].Detach()?.gameObject.SetActive(false);
                }
            }, (obj) =>
            {
                obj.isChecked = editor.panelTabs["Project"].Panel.gameObject.activeInHierarchy;
                return true;
            }
            ));
			panel.AddSubMenu(new ToggleMenuItem("Media Panel", new EditorKeybind(), (on) =>
			{
				if (on)
				{
					editor.panelTabs["Media"].Panel.gameObject.SetActive(true);
				}
				else
				{
					editor.panelTabs["Media"].Detach()?.gameObject.SetActive(false);
				}
			}, (obj) =>
			{
				obj.isChecked = editor.panelTabs["Media"].Panel.gameObject.activeInHierarchy;
				return true;
			}
			));
			
			panel.AddSubMenu(new ToggleMenuItem("Timeline Panel", new EditorKeybind(), (on) =>
			{
				if (on)
				{
					editor.panelTabs["Timeline"].Panel.gameObject.SetActive(true);
				}
				else
				{
					editor.panelTabs["Timeline"].Detach()?.gameObject.SetActive(false);
				}
			}, (obj) =>
			{
				obj.isChecked = editor.panelTabs["Timeline"].Panel.gameObject.activeInHierarchy;
				return true;
			}
			));
			panel.AddSubMenu(new ToggleMenuItem("Play Panel", new EditorKeybind(), (on) =>
			{
				if (on)
				{
					editor.panelTabs["Play"].Panel.gameObject.SetActive(true);
				}
				else
				{
					editor.panelTabs["Play"].Detach()?.gameObject.SetActive(false);
				}
			}, (obj) =>
			{
				obj.isChecked = editor.panelTabs["Play"].Panel.gameObject.activeInHierarchy;
				return true;
			}
			));
            panel.AddSubMenu(new SeparatorMenuItem());
#if DEBUG
            panel.AddSubMenu(new ActionMenuItem("Serialize Layout", () =>
            {
				NeoLogger.Debug(Convert.ToBase64String(PanelSerialization.SerializeCanvasToArray(editor.panelCanvas)));
			}));
#endif
            panel.AddSubMenu(new ActionMenuItem("Reset Layout", () => editor.ResetLayout()));
			AddMenu(panel);

            var view = new MenuItem("View");
            var zoom = view.AddSubMenu(new EntryMenuItem("Zoom"));
            zoom.AddSubMenu(new ActionMenuItem("Zoom In", new EditorKeybind(KeyModifier.Control, KeyCode.Plus), () => { }));
            zoom.AddSubMenu(new ActionMenuItem("Zoom Out", new EditorKeybind(KeyModifier.Control, KeyCode.Minus), () => { }));
            zoom.AddSubMenu(new ActionMenuItem("Restore Default Zoom", new EditorKeybind(KeyModifier.Control, KeyCode.Alpha0), () => { }));
            view.AddSubMenu(new SeparatorMenuItem());

            AddMenu(view);
        }

        void Update()
        {
            foreach(var menu in menus)
            foreach (var sub in menu.info.subMenus)
            {
                if (!sub.shortcut.IsPressed()) continue;

                if (sub is ActionMenuItem item)
                {
                    item.action();
                }
            }
        }

        public void GenerateUI() { }
    }
}
