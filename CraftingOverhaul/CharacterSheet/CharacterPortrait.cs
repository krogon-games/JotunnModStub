using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CraftingOverhaul.CharacterSheet
{
    /// <summary>
    ///     The live picture of the local player in the character sheet's arch, like WoW's character model: a camera of
    ///     our own films the player from the front into a texture shown in the sheet, so it shows your real gear and
    ///     idle animation. Drag it sideways to turn the view round the character.
    ///
    ///     Only the player may appear, so for just the length of our camera's render the player's renderers move to a
    ///     layer nothing else uses, which is the only layer our camera sees; a light of our own (also on only for that
    ///     render) lights the player at night. Everything goes back before any other camera renders. Runs only while
    ///     the inventory is open.
    /// </summary>
    internal class CharacterPortrait : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        /// <summary>Of the picture, how many metres of the scene fill the frame's height (at the art's base height).</summary>
        private const float ViewHeight = 2.7f;
        /// <summary>The feet stand on the pedestal drawn in the frame art, this far above the frame's bottom (pixels).</summary>
        private const float FeetAboveBottom = 46f;
        private const float BaseHeight = 504f;
        private const float FieldOfView = 30f;
        private const float RendererRefreshInterval = 0.5f;
        private const float TurnPerPixel = 0.6f;
        private const int MaxTextureHeight = 1024;
        private static readonly Color Backdrop = new Color(0.10f, 0.14f, 0.19f, 1f);

        private RawImage _image;
        private Camera _camera;
        private Light _light;
        private RenderTexture _texture;
        private int _layer = -1;
        private float _yaw;
        private float _nextRendererRefresh;
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private readonly List<int> _savedLayers = new List<int>();
        private bool _swapped;
        private static bool _subscribed;
        private static CharacterPortrait _active;

        public static CharacterPortrait Attach(RawImage image)
        {
            var portrait = image.gameObject.AddComponent<CharacterPortrait>();
            portrait._image = image;
            image.enabled = false;
            return portrait;
        }

        private void Update()
        {
            try
            {
                Restore();
                var player = Player.m_localPlayer;
                bool show = ModConfig.SheetPortrait.Value && InventoryGui.IsVisible() && player && !player.IsDead() && isActiveAndEnabled;
                if (!show)
                {
                    Stop();
                    // Each time the inventory opens, the picture starts from the front again.
                    _yaw = 0f;
                    return;
                }
                if (!EnsureCamera())
                {
                    _image.enabled = false;
                    return;
                }
                Frame(player);
                _camera.enabled = true;
                _image.enabled = true;
                _active = this;
            }
            catch (Exception ex)
            {
                Safe.Report("show the character picture", ex);
                Stop();
            }
        }

        /// <summary>Places the camera in front of the player so the feet land on the frame's pedestal.</summary>
        private void Frame(Player player)
        {
            var rect = ((RectTransform)transform).rect;
            if (rect.height <= 1f || rect.width <= 1f)
            {
                return;
            }
            float viewHeight = ViewHeight * rect.height / BaseHeight;
            float feet = FeetAboveBottom / rect.height;
            float lookHeight = (0.5f - feet) * viewHeight;
            float distance = viewHeight / 2f / Mathf.Tan(FieldOfView * Mathf.Deg2Rad / 2f);

            Vector3 forward = player.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            forward = Quaternion.Euler(0f, _yaw, 0f) * forward.normalized;
            Vector3 target = player.transform.position + Vector3.up * lookHeight;
            _camera.transform.position = target + forward * distance;
            _camera.transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
            _camera.fieldOfView = FieldOfView;
            _camera.nearClipPlane = Mathf.Max(0.1f, distance - 3f);
            _camera.farClipPlane = distance + 3f;
            // In front and above, a little to one side, so the face and gear get some shape.
            _light.transform.position = _camera.transform.position + Vector3.up * 1.5f + _camera.transform.right * 1.2f;
            _light.range = distance + 6f;

            // The texture follows the frame's size on screen (sharp at any UI scale), capped.
            var canvas = GetComponentInParent<Canvas>();
            float scale = canvas ? Mathf.Max(1f, canvas.scaleFactor) : 1f;
            int height = Mathf.Clamp(Mathf.RoundToInt(rect.height * scale), 64, MaxTextureHeight);
            int width = Mathf.Max(32, Mathf.RoundToInt(height * rect.width / rect.height));
            if (!_texture || _texture.width != width || _texture.height != height)
            {
                if (_texture)
                {
                    _camera.targetTexture = null;
                    _texture.Release();
                    Destroy(_texture);
                }
                _texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { name = "CraftingOverhaul_Portrait", antiAliasing = 2 };
                _camera.targetTexture = _texture;
                _image.texture = _texture;
            }
            _camera.aspect = rect.width / rect.height;
        }

        private bool EnsureCamera()
        {
            if (_camera)
            {
                return true;
            }
            _layer = FreeLayer();
            if (_layer < 0)
            {
                Jotunn.Logger.LogWarning("No free layer for the character picture; the sheet shows an empty frame.");
                ModConfig.SheetPortrait.Value = false;
                return false;
            }
            var go = new GameObject("CraftingOverhaul_PortraitCamera");
            DontDestroyOnLoad(go);
            _camera = go.AddComponent<Camera>();
            _camera.enabled = false;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Backdrop;
            _camera.cullingMask = 1 << _layer;
            _camera.depth = -50f;
            _camera.useOcclusionCulling = false;
            _camera.allowHDR = false;
            _camera.allowMSAA = true;

            var lightGo = new GameObject("CraftingOverhaul_PortraitLight");
            lightGo.transform.SetParent(go.transform, false);
            _light = lightGo.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = new Color(1f, 0.93f, 0.82f);
            _light.intensity = 1.6f;
            _light.shadows = LightShadows.None;
            _light.cullingMask = 1 << _layer;
            _light.enabled = false;

            if (!_subscribed)
            {
                _subscribed = true;
                Camera.onPreCull += OnAnyPreCull;
                Camera.onPostRender += OnAnyPostRender;
            }
            Jotunn.Logger.LogInfo($"Character picture camera created (layer {_layer}).");
            return true;
        }

        /// <summary>A layer with no name, so nothing in the game uses it; searched from the top.</summary>
        private static int FreeLayer()
        {
            for (int i = 31; i >= 8; i--)
            {
                if (string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    return i;
                }
            }
            return -1;
        }

        private static void OnAnyPreCull(Camera camera)
        {
            var portrait = _active;
            if (!portrait || camera != portrait._camera)
            {
                return;
            }
            try
            {
                portrait.MovePlayerToLayer();
                portrait._light.enabled = true;
            }
            catch (Exception ex)
            {
                Safe.Report("film the character picture", ex);
                portrait.Restore();
            }
        }

        private static void OnAnyPostRender(Camera camera)
        {
            var portrait = _active;
            if (portrait && camera == portrait._camera)
            {
                portrait.Restore();
            }
        }

        private void MovePlayerToLayer()
        {
            var player = Player.m_localPlayer;
            if (!player)
            {
                return;
            }
            // Gear changes add and remove renderers; look them up again now and then.
            if (Time.unscaledTime >= _nextRendererRefresh)
            {
                _nextRendererRefresh = Time.unscaledTime + RendererRefreshInterval;
                _renderers.Clear();
                player.GetComponentsInChildren(false, _renderers);
            }
            _savedLayers.Clear();
            for (int i = 0; i < _renderers.Count; i++)
            {
                var r = _renderers[i];
                if (!r)
                {
                    _savedLayers.Add(-1);
                    continue;
                }
                _savedLayers.Add(r.gameObject.layer);
                r.gameObject.layer = _layer;
            }
            _swapped = true;
        }

        /// <summary>Puts every renderer back on its own layer. Also called each frame as a safety net.</summary>
        private void Restore()
        {
            if (_light)
            {
                _light.enabled = false;
            }
            if (!_swapped)
            {
                return;
            }
            _swapped = false;
            for (int i = 0; i < _renderers.Count && i < _savedLayers.Count; i++)
            {
                var r = _renderers[i];
                if (r && _savedLayers[i] >= 0)
                {
                    r.gameObject.layer = _savedLayers[i];
                }
            }
        }

        private void Stop()
        {
            Restore();
            if (_camera)
            {
                _camera.enabled = false;
            }
            if (_image)
            {
                _image.enabled = false;
            }
            if (_active == this)
            {
                _active = null;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Handling the drag here keeps it from moving the whole sheet.
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _yaw = Mathf.Repeat(_yaw + eventData.delta.x * TurnPerPixel, 360f);
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
            if (_camera)
            {
                _camera.targetTexture = null;
                Destroy(_camera.gameObject);
            }
            if (_texture)
            {
                _texture.Release();
                Destroy(_texture);
            }
        }
    }
}
