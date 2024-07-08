using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private List<MenuContent> contents = new List<MenuContent>();

        public void AddMenu(MenuItem root)
        {
            root.parent = null;
            MenuButton menu = Instantiate(menuBarButtonTemplate, menubar);
            menu.text.text = root.text;
            menu.info = root;
            menu.gameObject.SetActive(true);

            var content = Instantiate(menuTemplate, menu.rect);
            foreach (MenuItem sub in root.subMenus)
            {
                AddSubMenu(sub, content, 1);
            }

            menu.button.onClick.AddListener(() => ShowMenu(content));

            contents.Add(content);
        }

        public void AddSubMenu(MenuItem menu, MenuContent content, int depth)
        {
            if (menu is SeparatorMenuItem)
            {
                Instantiate(separatorTemplate, content.rect).gameObject.SetActive(true);
            }
            else
            {
                MenuButton button = Instantiate(menuButtonTemplate, content.rect);
                button.text.text = menu.text;
                button.shortcut.text = menu.shortcut;
                button.checkbox.gameObject.SetActive(menu is ToggleMenuItem);
                button.gameObject.SetActive(true);
                if (menu is EntryMenuItem)
                {
                    var subContent = Instantiate(menuTemplate, button.rect);
                    subContent.rect.anchoredPosition = new Vector2(298f, 2);
                    subContent.parents = content.parents.ToList();
                    subContent.parents.Add(content);
                    subContent.canvas.sortingOrder += depth;
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
                        () => (menu as ToggleMenuItem).action(!button.isChecked)
                    );
                }
            }
        }

        public void ShowMenu(MenuContent content)
        {
            foreach (var c in contents)
            {
                if (!content.parents.Contains(c))
                    c.gameObject.SetActive(false);
            }
            content.gameObject.SetActive(true);
        }

        void Start()
        {
            var file = new MenuItem("File", "");
            file.AddSubMenu(new ActionMenuItem("New", "Ctrl + N", () => { }));
            file.AddSubMenu(new SeparatorMenuItem());
			file.AddSubMenu(new ActionMenuItem("Open", "Ctrl + O", () =>
			{
				NeoEditor.Instance.OpenLevel();
			}));
			file.AddSubMenu(new ActionMenuItem("Open Recent", "Ctrl + Shift + O", () => { }));
            file.AddSubMenu(new SeparatorMenuItem());
			file.AddSubMenu(new ActionMenuItem("Save", "Ctrl + S", () =>
			{
				NeoEditor.Instance.SaveLevel();
			}));
			file.AddSubMenu(new ActionMenuItem("Save As", "Ctrl + Shift + S", () =>
			{
				NeoEditor.Instance.SaveLevelAs();
			}));
			file.AddSubMenu(new SeparatorMenuItem());
            file.AddSubMenu(new ActionMenuItem("Help", "Ctrl + H", () => { }));
            file.AddSubMenu(new ActionMenuItem("Preference", "Ctrl + Shift + I", () => { }));
            file.AddSubMenu(new ActionMenuItem("Exit", "", controller.QuitToMainMenu));
            AddMenu(file);

            var edit = new MenuItem("Edit", "");
            edit.AddSubMenu(new ActionMenuItem("Undo", "Ctrl + Z", () => { }));
            edit.AddSubMenu(new ActionMenuItem("Redo", "Ctrl + Shift + Z", () => { }));
            edit.AddSubMenu(new SeparatorMenuItem());
            edit.AddSubMenu(new ActionMenuItem("Cut", "Ctrl + X", () => { }));
            edit.AddSubMenu(new ActionMenuItem("Copy", "Ctrl + C", () => { }));
            edit.AddSubMenu(new ActionMenuItem("Paste", "Ctrl + V", () => { }));
            edit.AddSubMenu(new ActionMenuItem("Delete", "Del", () => { }));
            edit.AddSubMenu(new SeparatorMenuItem());
            var find = edit.AddSubMenu(new EntryMenuItem("Find"));
            find.AddSubMenu(new ActionMenuItem("Floor", "Ctrl + F", () => { }));
            find.AddSubMenu(new ActionMenuItem("Event", "Ctrl + Shift + F", () => { }));
            find.AddSubMenu(new ActionMenuItem("Decoration", "Ctrl + Alt + F", () => { }));
            var replace = edit.AddSubMenu(new EntryMenuItem("Replace"));
            replace.AddSubMenu(new ActionMenuItem("Event", "Ctrl + Shift + H", () => { }));
            replace.AddSubMenu(new ActionMenuItem("Decoration", "Ctrl + Alt + H", () => { }));
            edit.AddSubMenu(new SeparatorMenuItem());
            edit.AddSubMenu(new ActionMenuItem("Select All", "Ctrl + A", () => { }));
            AddMenu(edit);

            var view = new MenuItem("View", "");
            var zoom = view.AddSubMenu(new EntryMenuItem("Zoom"));
            zoom.AddSubMenu(new ActionMenuItem("Zoom In", "Ctrl + Plus", () => { }));
            zoom.AddSubMenu(new ActionMenuItem("Zoom Out", "Ctrl + Minus", () => { }));
            zoom.AddSubMenu(new ActionMenuItem("Restore Default Zoom", "Ctrl + 0", () => { }));
            view.AddSubMenu(new SeparatorMenuItem());

            AddMenu(view);
        }

        void Update() { }

        public void GenerateUI() { }
    }
}
