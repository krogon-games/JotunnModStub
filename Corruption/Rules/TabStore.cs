using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>What a tab's loader found in one saved blob.</summary>
    internal struct TabLoadReport
    {
        /// <summary>How many items the blob says it holds (its own header).</summary>
        public int Expected;

        /// <summary>How many items made it into the tab (the game skips items whose prefab is missing).</summary>
        public int Loaded;

        /// <summary>Whether the loader read the blob to its last byte (a short or garbled blob stops early or throws).</summary>
        public bool ReadToEnd;
    }

    /// <summary>
    ///     Saving and loading the inventory tabs with the character, transactionally (no game types, so it's tested).
    ///     Each tab is saved under its own key in the character's custom data. A tab whose saved blob can't be read
    ///     completely (garbled, an item whose prefab is missing, the loader failing) is <b>locked</b> for the session:
    ///     <list type="bullet">
    ///         <item>nothing is exposed from it (no item from it can be spent, so a reload can't hand it back a second time),</item>
    ///         <item>nothing new goes into it (pickups go to the bag, which saves normally, so nothing vanishes),</item>
    ///         <item>its saved blob is never written over, so it's still there, unchanged, for a later fix.</item>
    ///     </list>
    ///     A tab that loads completely works normally even when the other one is locked.
    /// </summary>
    internal sealed class TabStore
    {
        private readonly string[] _keys;
        private readonly bool[] _usable;
        private readonly string[] _problems;

        public TabStore(params string[] keys)
        {
            _keys = keys;
            _usable = new bool[keys.Length];
            _problems = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                // A new character never loads: its tabs start empty and usable.
                _usable[i] = true;
            }
        }

        /// <summary>Another character: every tab usable and empty until its own load says otherwise.</summary>
        public void Reset()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                _usable[i] = true;
                _problems[i] = null;
            }
        }

        /// <summary>Before a character's tabs are read: every tab is locked until its own blob has loaded completely.</summary>
        public void BeginLoad()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                _usable[i] = false;
                _problems[i] = null;
            }
        }

        /// <summary>
        ///     Reads one tab's blob from <paramref name="customData" /> with <paramref name="load" /> (which fills a scratch
        ///     inventory and reports what it found). Returns true when the tab loaded completely: only then may the caller
        ///     show the scratch inventory's items. An absent or empty blob is an empty tab.
        /// </summary>
        public bool Load(IDictionary<string, string> customData, string key, Func<byte[], TabLoadReport> load)
        {
            int i = Index(key);
            _usable[i] = false;
            _problems[i] = null;
            if (customData == null || !customData.TryGetValue(key, out string text) || string.IsNullOrEmpty(text))
            {
                _usable[i] = true;
                return true;
            }
            TabLoadReport report;
            try
            {
                report = load(Convert.FromBase64String(text));
            }
            catch (Exception ex)
            {
                _problems[i] = "its saved data couldn't be read (" + ex.GetType().Name + ": " + ex.Message + ")";
                return false;
            }
            if (report.Loaded != report.Expected)
            {
                _problems[i] = $"only {report.Loaded} of its {report.Expected} saved items could be loaded (an item's prefab is missing: a mod that adds it isn't loaded?)";
                return false;
            }
            if (!report.ReadToEnd)
            {
                _problems[i] = "its saved data has extra bytes after the last item (damaged or from an unknown version)";
                return false;
            }
            _usable[i] = true;
            return true;
        }

        public bool IsUsable(string key) => _usable[Index(key)];

        /// <summary>Why a tab is locked, or null.</summary>
        public string Problem(string key) => _problems[Index(key)];

        /// <summary>Saves a usable tab under its key (<paramref name="write" /> gives its blob); a locked tab's saved blob stays as it was.</summary>
        public void Save(IDictionary<string, string> customData, string key, Func<string> write)
        {
            if (customData == null || !_usable[Index(key)])
            {
                return;
            }
            customData[key] = write();
        }

        private int Index(string key)
        {
            int i = Array.IndexOf(_keys, key);
            if (i < 0)
            {
                throw new ArgumentException("Unknown tab key " + key, nameof(key));
            }
            return i;
        }
    }
}
