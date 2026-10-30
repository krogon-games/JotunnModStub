using System;
using System.Collections.Generic;
using CraftingOverhaul.CharacterSheet;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     The class choice window (brief §2.6): opens when the player uses the Orlog stone (the classes plugin's
    ///     <c>GetClassStoneRequest</c> goes up) while they have no class. Lists the classes with icon, name and short
    ///     description; Confirm calls <c>ChooseClass</c> and closes on success, or shows the reason.
    /// </summary>
    internal static class ClassChoiceWindow
    {
        private const float Width = 520f;
        private const float RowHeight = 96f;
        private const float Margin = 22f;

        private static RectTransform _root;
        private static RectTransform _list;
        private static Text _message;
        private static GameObject _confirm;
        private static readonly List<GameObject> Rows = new List<GameObject>();
        private static readonly List<Image> RowBacks = new List<Image>();
        private static string[] _classIds = new string[0];
        private static string _selected = "";
        private static int _lastRequest = int.MinValue;

        public static bool IsOpen => _root && _root.gameObject.activeSelf;

        public static void Open()
        {
            if (!Player.m_localPlayer || !GUIManager.CustomGUIFront)
            {
                return;
            }
            _classIds = FlatJson.Strings(ClassesSource.ClassIds());
            if (_classIds.Length == 0)
            {
                return;
            }
            SkillsPanel.Close();
            Build();
            _root.gameObject.SetActive(true);
            _root.SetAsLastSibling();
            ClassWindows.SetOpen(ClassWindows.Choice, true);
        }

        public static void Close()
        {
            if (_root)
            {
                _root.gameObject.SetActive(false);
            }
            ClassWindows.SetOpen(ClassWindows.Choice, false);
        }

        private static void Build()
        {
            float height = 70f + _classIds.Length * (RowHeight + 8f) + 110f;
            if (!_root)
            {
                _root = UiKit.Panel("CraftingOverhaul_ClassChoice", GUIManager.CustomGUIFront.transform, new Vector2(Width, height));
                var title = UiKit.Text("Title", _root, 26, UiKit.Gold, TextAnchor.MiddleCenter);
                UiKit.SetText(title, "Choose your class");
                UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(Width, 40f));
                var close = UiKit.Button("X", _root, new Vector2(36f, 36f), Close);
                UiKit.Place((RectTransform)close.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-Margin, -14f), new Vector2(36f, 36f));

                _list = UiKit.Rect("Classes", _root);
                UiKit.Place(_list, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(Width - 2f * Margin, 0f));

                _message = UiKit.Text("Message", _root, 15, new Color(1f, 0.6f, 0.5f), TextAnchor.MiddleCenter);
                UiKit.Place(_message.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(Width, 24f));
                _confirm = UiKit.Button("Confirm", _root, new Vector2(160f, 40f), Confirm).gameObject;
                UiKit.Place((RectTransform)_confirm.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, Margin), new Vector2(160f, 40f));
            }
            _root.sizeDelta = new Vector2(Width, height);
            UiKit.Place(_root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Width, height));
            UiKit.SetText(_message, "");

            foreach (var row in Rows)
            {
                UnityEngine.Object.Destroy(row);
            }
            Rows.Clear();
            RowBacks.Clear();
            for (int i = 0; i < _classIds.Length; i++)
            {
                string id = _classIds[i];
                string info = ClassesSource.ClassInfo(id);
                var back = UiKit.Image("Class_" + id, _list, SheetArt.Parchment, Image.Type.Simple, true);
                if (!back.sprite)
                {
                    back.color = new Color(0.84f, 0.75f, 0.59f);
                }
                UiKit.Place(back.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -i * (RowHeight + 8f)),
                    new Vector2(Width - 2f * Margin, RowHeight));
                back.gameObject.AddComponent<Button>().onClick.AddListener(() => Select(id));

                var icon = UiKit.Image("Icon", back.rectTransform, ClassesSource.ClassIcon(id));
                icon.preserveAspect = true;
                icon.enabled = icon.sprite;
                UiKit.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(68f, 68f));
                var name = UiKit.Text("Name", back.rectTransform, 20, UiKit.Ink, TextAnchor.UpperLeft, false);
                UiKit.SetText(name, FlatJson.Get(info, "name") ?? id);
                UiKit.Place(name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -10f), new Vector2(Width - 2f * Margin - 110f, 26f));
                var description = UiKit.Text("Description", back.rectTransform, 14, UiKit.Ink, TextAnchor.UpperLeft, false);
                description.horizontalOverflow = HorizontalWrapMode.Wrap;
                UiKit.SetText(description, FlatJson.Get(info, "description") ?? "");
                UiKit.Place(description.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -38f), new Vector2(Width - 2f * Margin - 110f, RowHeight - 44f));
                Rows.Add(back.gameObject);
                RowBacks.Add(back);
            }
            Select(_classIds.Length == 1 ? _classIds[0] : "");
        }

        private static void Select(string id)
        {
            _selected = id;
            for (int i = 0; i < RowBacks.Count; i++)
            {
                RowBacks[i].color = _classIds[i] == id ? Color.white : new Color(0.65f, 0.6f, 0.55f);
            }
            _confirm.GetComponent<Button>().interactable = id.Length > 0;
        }

        private static void Confirm()
        {
            if (_selected.Length == 0)
            {
                return;
            }
            string refused = ClassesSource.ChooseClass(_selected);
            if (string.IsNullOrEmpty(refused))
            {
                Close();
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "You are now " + Article(_selected) + " " + _selected);
            }
            else
            {
                UiKit.SetText(_message, refused);
            }
        }

        private static string Article(string word)
        {
            return word.Length > 0 && "AEIOUaeiou".IndexOf(word[0]) >= 0 ? "an" : "a";
        }

        private static void Update()
        {
            if (!ClassesSource.Available || !Player.m_localPlayer)
            {
                if (IsOpen)
                {
                    Close();
                }
                return;
            }
            int request = ClassesSource.StoneRequest;
            if (_lastRequest == int.MinValue)
            {
                // First look: a request from before we started watching doesn't open the window.
                _lastRequest = request;
                return;
            }
            if (request == _lastRequest)
            {
                return;
            }
            bool used = request > _lastRequest;
            _lastRequest = request;
            if (used && !IsOpen && string.IsNullOrEmpty(FlatJson.Get(ClassesSource.Character(), "class")))
            {
                Open();
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.Update))]
        private static class Update_Patch
        {
            private static void Postfix()
            {
                try
                {
                    Update();
                }
                catch (Exception ex)
                {
                    Safe.Report("open the class choice window", ex);
                }
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.OnDestroy))]
        private static class OnDestroy_Patch
        {
            private static void Postfix()
            {
                Close();
                if (_root)
                {
                    UnityEngine.Object.Destroy(_root.gameObject);
                }
                _root = null;
                Rows.Clear();
                RowBacks.Clear();
                _lastRequest = int.MinValue;
            }
        }
    }
}
