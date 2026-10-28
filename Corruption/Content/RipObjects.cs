using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     A rip portal. Its ZDO holds where it leads (<see cref="Rips.KeyTarget" />) and when it closes. The entry rip
    ///     at home leads into the rip; the exit rip, opened by destroying the heart, leads back home.
    /// </summary>
    internal class RipPortal : MonoBehaviour, Hoverable, Interactable
    {
        public EffectFade m_targetFound;
        public MeshRenderer m_model;

        private ZNetView m_nview;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            if (!m_nview || m_nview.GetZDO() == null)
            {
                enabled = false;
                return;
            }
            if (m_model)
            {
                m_model.material.SetColor("_EmissionColor", new Color(1.4f, 0.12f, 0.06f));
            }
            InvokeRepeating(nameof(Tick), 0.5f, 1f);
        }

        private void Tick()
        {
            Safe.Run("update a rip portal", () =>
            {
                if (!m_nview.IsValid())
                {
                    return;
                }
                if (m_targetFound)
                {
                    m_targetFound.SetActive(Player.GetClosestPlayer(transform.position, 8f));
                }
                if (m_nview.IsOwner() && Rips.IsExpired(m_nview.GetZDO()))
                {
                    m_nview.Destroy();
                }
            });
        }

        public void Enter(Player player)
        {
            if (m_nview && m_nview.IsValid())
            {
                Rips.Enter(m_nview.GetZDO(), player);
            }
        }

        public string GetHoverText()
        {
            return m_nview && m_nview.IsValid() ? Rips.PortalHoverText(m_nview.GetZDO()) : "";
        }

        public string GetHoverName()
        {
            return Localization.instance.Localize("$corruption_rip");
        }

        public float GetHoverOffset()
        {
            return 0f;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold || !(user is Player player) || player != Player.m_localPlayer)
            {
                return false;
            }
            Enter(player);
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }

    /// <summary>Walking into the portal's swirl enters it, like a vanilla portal.</summary>
    internal class RipTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<Player>();
            if (player && player == Player.m_localPlayer)
            {
                GetComponentInParent<RipPortal>()?.Enter(player);
            }
        }
    }

    /// <summary>
    ///     The rip's heart at the landing spot. Destroying it opens the exit rip home; it collapses with the rip when
    ///     the rip's time runs out. Settles onto the ground once its area has loaded (it was placed from world
    ///     generation heights, before anyone had been there).
    /// </summary>
    internal class RipHeart : MonoBehaviour, Hoverable
    {
        private ZNetView m_nview;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            if (!m_nview || m_nview.GetZDO() == null)
            {
                enabled = false;
                return;
            }
            var destructible = GetComponent<Destructible>();
            if (destructible)
            {
                destructible.m_health = CorruptionConfig.RipHeartHealth.Value;
                destructible.m_onDestroyed += () => Safe.Run("open the way home", () => Rips.OpenExit(m_nview.GetZDO(), transform.position));
            }
            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        private void Tick()
        {
            Safe.Run("update a rip heart", () =>
            {
                if (!m_nview.IsValid() || !m_nview.IsOwner())
                {
                    return;
                }
                var zdo = m_nview.GetZDO();
                if (Rips.IsExpired(zdo))
                {
                    m_nview.Destroy();
                    return;
                }
                if (!zdo.GetBool(Rips.KeyGrounded) && ZNetScene.instance.IsAreaReady(transform.position) &&
                    Rips.TryFindGround(transform.position, out var ground))
                {
                    transform.position = ground;
                    zdo.SetPosition(ground);
                    zdo.Set(Rips.KeyGrounded, true);
                }
            });
        }

        public string GetHoverText()
        {
            return m_nview && m_nview.IsValid() ? Rips.HeartHoverText(m_nview.GetZDO()) : "";
        }

        public string GetHoverName()
        {
            return Localization.instance.Localize("$corruption_rip_heart");
        }

        public float GetHoverOffset()
        {
            return 0f;
        }
    }
}
