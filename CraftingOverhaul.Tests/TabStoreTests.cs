using System;
using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     The inventory tabs' save and load (<see cref="TabStore" />), driven the way <c>InventoryTabs</c> drives it:
    ///     a character file (custom data with one base64 blob per tab), a loader that fills a scratch inventory and
    ///     reports what it found, pickups that go to a tab only while it's usable, and saves. The fake blob format: one
    ///     byte per item, 0 = an item whose prefab is missing (the game skips it), 255 = garbage that makes it throw.
    /// </summary>
    public class TabStoreTests
    {
        private const string Currency = "corruption_tab_currency";
        private const string Maps = "corruption_tab_maps";

        /// <summary>A stand-in for the player: the bag, the two tabs and the character file, saved and reloaded like the game does.</summary>
        private sealed class Character
        {
            public readonly Dictionary<string, string> CustomData = new Dictionary<string, string>();
            public List<byte> SavedBag = new List<byte>();
            public List<byte> Bag = new List<byte>();
            public readonly Dictionary<string, List<byte>> Tabs = new Dictionary<string, List<byte>>
            {
                { Currency, new List<byte>() }, { Maps, new List<byte>() }
            };
            public readonly TabStore Store = new TabStore(Currency, Maps);

            public static string Blob(params byte[] items)
            {
                var bytes = new List<byte> { (byte)items.Length };
                bytes.AddRange(items);
                return Convert.ToBase64String(bytes.ToArray());
            }

            /// <summary>Player.Load: prefix locks, postfix loads each tab into scratch and shows it only when complete.</summary>
            public void Load(bool gameLoadThrows = false)
            {
                Store.BeginLoad();
                if (gameLoadThrows)
                {
                    return;
                }
                Bag = new List<byte>(SavedBag);
                foreach (var key in new[] { Currency, Maps })
                {
                    List<byte> scratch = null;
                    bool complete = Store.Load(CustomData, key, bytes => Read(bytes, out scratch));
                    Tabs[key] = complete && scratch != null ? scratch : new List<byte>();
                }
            }

            private static TabLoadReport Read(byte[] bytes, out List<byte> scratch)
            {
                scratch = new List<byte>();
                int expected = bytes[0];
                int pos = 1;
                for (int i = 0; i < expected && pos < bytes.Length; i++, pos++)
                {
                    if (bytes[pos] == 255)
                    {
                        throw new InvalidOperationException("garbled item");
                    }
                    if (bytes[pos] != 0)
                    {
                        scratch.Add(bytes[pos]);
                    }
                }
                return new TabLoadReport { Expected = expected, Loaded = scratch.Count, ReadToEnd = pos == bytes.Length };
            }

            /// <summary>InventoryTabs.Get: null for a locked tab.</summary>
            public List<byte> Tab(string key) => Store.IsUsable(key) ? Tabs[key] : null;

            /// <summary>Inventory.AddItem prefix: into the tab when usable, else the bag.</summary>
            public void PickUp(string key, byte item) => (Tab(key) ?? Bag).Add(item);

            /// <summary>Inventory.RemoveItem prefix (tabs pay first) then the bag; true if paid.</summary>
            public bool Spend(byte item)
            {
                foreach (var key in new[] { Currency, Maps })
                {
                    var tab = Tab(key);
                    if (tab != null && tab.Remove(item))
                    {
                        return true;
                    }
                }
                return Bag.Remove(item);
            }

            /// <summary>Player.Save prefix and the game's own bag save.</summary>
            public void Save()
            {
                SavedBag = new List<byte>(Bag);
                foreach (var key in new[] { Currency, Maps })
                {
                    Store.Save(CustomData, key, () => Blob(Tabs[key].ToArray()));
                }
            }

            public int Owned(byte item) => Bag.Count(i => i == item) + Tabs.Values.Sum(t => t.Count(i => i == item));
        }

        [Fact]
        public void ValidCurrencyAndCorruptMaps_CurrencyCanNotBeDuplicated()
        {
            var c = new Character();
            c.CustomData[Currency] = Character.Blob(7, 7, 7);
            string corruptMaps = Character.Blob(1, 255, 2);
            c.CustomData[Maps] = corruptMaps;

            c.Load();
            Assert.True(c.Store.IsUsable(Currency));
            Assert.False(c.Store.IsUsable(Maps));
            Assert.Contains("couldn't be read", c.Store.Problem(Maps));

            // Spend two shards, save, quit, come back: one shard left, not three.
            Assert.True(c.Spend(7));
            Assert.True(c.Spend(7));
            c.Save();
            c.Load();
            Assert.Equal(1, c.Owned(7));
            Assert.Equal(corruptMaps, c.CustomData[Maps]);
        }

        [Fact]
        public void LockedTab_NewPickupsGoToTheBagAndSurviveAReload()
        {
            var c = new Character();
            c.CustomData[Maps] = Character.Blob(1, 255);
            c.Load();

            c.PickUp(Maps, 9);
            Assert.Contains((byte)9, c.Bag);
            c.Save();
            c.Load();
            Assert.Equal(1, c.Owned(9));
        }

        [Fact]
        public void MissingPrefab_LocksTheTabAndKeepsItsBlob()
        {
            var c = new Character();
            string maps = Character.Blob(4, 0, 5); // the middle map's prefab is gone (its mod isn't loaded)
            c.CustomData[Maps] = maps;
            c.CustomData[Currency] = Character.Blob(7);

            c.Load();
            Assert.False(c.Store.IsUsable(Maps));
            Assert.Contains("2 of its 3", c.Store.Problem(Maps));
            Assert.Null(c.Tab(Maps));
            Assert.Empty(c.Tabs[Maps]); // the two maps that did load aren't shown either

            c.PickUp(Maps, 6);
            c.Save();
            Assert.Equal(maps, c.CustomData[Maps]);

            // The mod is back next session: the blob still holds all three maps.
            c.CustomData[Maps] = Character.Blob(4, 3, 5);
            c.Load();
            Assert.True(c.Store.IsUsable(Maps));
            Assert.Equal(new byte[] { 4, 3, 5 }, c.Tabs[Maps]);
            Assert.Equal(1, c.Owned(6));
        }

        [Fact]
        public void GarbledBase64_LocksAndKeepsTheText()
        {
            var c = new Character();
            c.CustomData[Currency] = "not base64 !!";
            c.Load();
            Assert.False(c.Store.IsUsable(Currency));
            c.Save();
            Assert.Equal("not base64 !!", c.CustomData[Currency]);
        }

        [Fact]
        public void ExtraBytesAfterTheLastItem_Locks()
        {
            var c = new Character();
            var bytes = Convert.FromBase64String(Character.Blob(7)).Concat(new byte[] { 1, 2 }).ToArray();
            c.CustomData[Currency] = Convert.ToBase64String(bytes);
            c.Load();
            Assert.False(c.Store.IsUsable(Currency));
            Assert.Contains("extra bytes", c.Store.Problem(Currency));
        }

        [Fact]
        public void GameLoadFails_NothingIsSavedOver()
        {
            var c = new Character();
            string currency = Character.Blob(7, 7);
            c.CustomData[Currency] = currency;
            c.CustomData[Maps] = Character.Blob(3);
            c.Load(gameLoadThrows: true);

            Assert.False(c.Store.IsUsable(Currency));
            Assert.False(c.Store.IsUsable(Maps));
            c.Save();
            Assert.Equal(currency, c.CustomData[Currency]);
        }

        [Fact]
        public void NewCharacterWithoutSavedTabs_TabsWorkAndSave()
        {
            var c = new Character();
            c.Load();
            Assert.True(c.Store.IsUsable(Currency));
            c.PickUp(Currency, 7);
            c.Save();
            c.Load();
            Assert.Equal(new byte[] { 7 }, c.Tabs[Currency]);
        }

        [Fact]
        public void Lock_MidSession_KeepsWhatWasWrittenThroughLaterSaves()
        {
            // co_tabs_damage: writes a damaged blob, locks the tab, and the logout save must not overwrite it.
            var c = new Character();
            c.CustomData[Maps] = Character.Blob(3);
            c.Load();
            string damaged = Character.Blob(3, 0);
            c.CustomData[Maps] = damaged;
            c.Store.Lock(Maps, "test");
            c.Save();
            Assert.Equal(damaged, c.CustomData[Maps]);
            c.Load();
            Assert.False(c.Store.IsUsable(Maps));
        }

        [Fact]
        public void Reset_UnlocksForTheNextCharacter()
        {
            var store = new TabStore(Currency, Maps);
            store.BeginLoad();
            Assert.False(store.IsUsable(Maps));
            store.Reset();
            Assert.True(store.IsUsable(Maps));
            Assert.Null(store.Problem(Maps));
        }
    }
}
