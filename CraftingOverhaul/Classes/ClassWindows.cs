using System;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     Which class windows are open, so the game's input is blocked (and the cursor shown) while any is, and Escape
    ///     closes them instead of opening the game menu.
    /// </summary>
    internal static class ClassWindows
    {
        public const int Skills = 1;
        public const int Choice = 2;

        private static int _open;

        public static bool AnyOpen => _open != 0;

        public static void SetOpen(int window, bool open)
        {
            bool before = AnyOpen;
            _open = open ? _open | window : _open & ~window;
            if (before != AnyOpen)
            {
                GUIManager.BlockInput(AnyOpen);
            }
        }

        public static void CloseAll()
        {
            SkillsPanel.Close();
            ClassChoiceWindow.Close();
        }

        /// <summary>The menu opens on Escape in <c>Menu.Update</c>; while a class window is open, Escape closes it instead.</summary>
        [HarmonyPatch(typeof(Menu), nameof(Menu.Update))]
        private static class MenuUpdate_Patch
        {
            private static bool Prefix()
            {
                try
                {
                    if (AnyOpen && ZInput.GetKeyDown(KeyCode.Escape))
                    {
                        CloseAll();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Safe.Report("close the class windows", ex);
                }
                return true;
            }
        }
    }
}
