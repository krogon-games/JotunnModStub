using System;
using System.IO;
using BepInEx.Configuration;

namespace KrogonGames.Shared
{
    /// <summary>
    ///     Development convenience: reloads a plugin's .cfg file when it is saved while the game runs, so settings can
    ///     be tuned without a restart. Compiled into both plugins (linked file).
    ///
    ///     Cost: the operating system reports the change (no polling); each frame only checks one flag.
    ///     Before release: switch it off by default or remove it (see docs/CURRENT_STATE.md → Before release).
    ///
    ///     Not applied while connected to someone else's server: there the server's values for admin-only settings
    ///     apply, and reloading the local file would replace them.
    /// </summary>
    internal sealed class ConfigWatcher : IDisposable
    {
        private const float Debounce = 0.5f;

        private readonly ConfigFile _config;
        private readonly ConfigEntry<bool> _enabled;
        private readonly Action<string> _log;
        private FileSystemWatcher _watcher;
        private volatile bool _changed;
        private float _reloadAt;

        public ConfigWatcher(ConfigFile config, Action<string> log)
        {
            _config = config;
            _log = log;
            _enabled = config.Bind("0 - Development", "ReloadConfigOnFileChange", true,
                "Reload this file when it is saved while the game runs (development aid; off for release). " +
                "Ignored while connected to another player's or a dedicated server: change server settings on the server.");

            try
            {
                string path = config.ConfigFilePath;
                _watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path))
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
                };
                // Raised on a background thread: only set a flag here; Unity and BepInEx work happens in Tick.
                _watcher.Changed += (_, __) => _changed = true;
                _watcher.Created += (_, __) => _changed = true;
                _watcher.Renamed += (_, __) => _changed = true;
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                _log("Config file watching is unavailable: " + ex.Message);
                _watcher = null;
            }
        }

        /// <summary>Call from the plugin's Update (main thread).</summary>
        public void Tick()
        {
            if (_watcher == null)
            {
                return;
            }
            float now = UnityEngine.Time.realtimeSinceStartup;
            if (_changed)
            {
                // Editors save in several writes; wait until the file has been quiet for a moment.
                _changed = false;
                _reloadAt = now + Debounce;
                return;
            }
            if (_reloadAt <= 0f || now < _reloadAt)
            {
                return;
            }
            _reloadAt = 0f;

            if (!_enabled.Value)
            {
                return;
            }
            if (ZNet.instance && !ZNet.instance.IsServer())
            {
                _log($"{Path.GetFileName(_config.ConfigFilePath)} changed, not reloaded: connected to a server, whose settings apply.");
                return;
            }
            try
            {
                _config.Reload();
                _log($"Reloaded {Path.GetFileName(_config.ConfigFilePath)}.");
            }
            catch (Exception ex)
            {
                _log($"Could not reload {Path.GetFileName(_config.ConfigFilePath)}: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _watcher = null;
        }
    }
}
