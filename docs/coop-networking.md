# Co-op lag and desync in Valheim

Researched 2026-09-29 for the Reddit thread "I love this game but it's outright dishonest to…" (r/valheim, post `1wsz914`). Reddit blocks automated access, so only the thread's title could be read. This covers Valheim's co-op networking problems in general, checked against the game's code and against the networking mods currently on Thunderstore. If the thread describes something different, this needs revisiting.

## Why co-op feels bad: how Valheim's networking works

Read from the game's decompiled code (build 0.221.4). The 1.0 reference assemblies only contain signatures, so the constants below may have changed in 1.0.

**1. Each object is simulated by one player's PC, not by the server.**
- Every creature, ship, chest, cooking station and so on has an *owner*. That player's game runs its AI and physics and decides its hits. Everyone else sees the owner's version, delayed by the owner's connection.
- Ownership goes to whoever is nearby when the object has no owner. The game only re-checks every 2 seconds (`ZDOMan.ReleaseNearbyZDOS`), and it only hands an object over once its current owner has left the area. There is no preference for the player with the best connection or fastest PC.
- **Result:** whoever arrives first with the worst connection or slowest PC makes an area laggy for everyone. Enemies hit you from where they were on *their* screen, arrows land late, and a chest opens slowly because the request goes through another player.
- Even a dedicated server doesn't fix this, because the server only relays. It doesn't simulate.

**2. World updates are rationed.**
- The server sends object updates in rounds of about 50 ms, to one player per frame (`ZDOMan.SendZDOToPeers2`).
- Each player gets at most about 10 KB queued per round (`SendZDOs`).
- Steam's send rate is fixed at 150 KB/s (`ZSteamSocket`: `SendRateMin` = `SendRateMax` = 153600).
- Large bases (thousands of building pieces) and busy areas (Ashlands) fill that budget, so updates arrive late and things rubber-band.

**3. Player-hosted games add the host's limits.** The host's upload speed and PC are also the server's, and the host is usually the default owner of everything near them.

## Options

### A. Without mods

1. **Run a dedicated server** (a spare PC or a rented one) instead of hosting from a player's PC. This takes the host's own game load off the server role. It doesn't fix ownership (point 1).
2. **Let the player with the best connection and PC enter shared areas first**, especially the main base, so they become the owner. If one player causes lag, they can leave through a portal and come back so ownership moves.
3. Use wired connections, and turn crossplay off if everyone is on Steam. Crossplay routes traffic through PlayFab, which can add latency.

### B. Networking mods (recommended)

Checked on Thunderstore on 2026-09-29. These networking mods rewrite the same game code, so **install only one**; the better ones refuse to load together.

| Mod | Updated | What it targets | Install on |
|---|---|---|---|
| **MidnightMods-NetworkPerformanceSystem** 1.11.0 | 2026-09-28 | Latency and ownership: measures each player's connection, moves object ownership away from poor connections, transfers ownership before you interact, plus per-player send rates | Server or host only works. Clients optionally add latency compensation. No version lock. |
| VerdantsAscent-FiresGhettoNetworking 1.4.31 | 2026-09-16 | Bandwidth plus server-side simulation (the server runs the world), compression, auto-tuning | Built for dedicated servers; server, plus clients for compression |
| sighsorry-SkadiNet 1.1.6 | 2026-09-18 | Bandwidth: per-player send scheduling, compression, ownership recovery on disconnect | Server and every client, same version |
| DeathMonger-SpreadTheLoad 0.1.4 | 2026-09-28 | Ownership only: objects follow whoever uses them, ships follow the helmsman | Dedicated server only |
| Searica-NetworkTweaks 0.2.0 | 2026-09-15 | Bandwidth only: send rate and players served per tick | Server and clients |

Avoid **CW_Jesse-BetterNetworking**. Many guides still recommend it, but it was last updated in November 2023.

**Recommendation: NetworkPerformanceSystem.**
- It's the one aimed at the actual co-op problem, which is objects being owned by the worst connection (point 1). The others mostly add bandwidth (point 2).
- It works with only the host or server having it, doesn't lock out players who don't have it, and works on both player-hosted games and dedicated servers.
- It lists Valheim Plus as compatible.
- It has a built-in diagnostic mode (`Enable Network Monitoring`, then `nps_stats` in the console), so any remaining lag can be measured rather than guessed at.

If you move to a dedicated server with a larger group and still have problems, FiresGhettoNetworking's server-side simulation is the next thing to try, instead of NetworkPerformanceSystem, not alongside it.

**Caveats:**
- None of these have been tested by us.
- NetworkPerformanceSystem is young (about 23k downloads) and changes often. Keep a backup of the world before trying it.

### C. Writing our own networking code: not recommended for now

- The mods above are active, updated for 1.0, and already cover this. Our own version would conflict with them, since they deliberately refuse to load alongside other networking mods.
- Networking changes can only be checked with several real players on real connections, which can't be done from this development setup.
- It would take time away from the crafting, loot and combat work, which nobody else is doing the way you want.

What CraftingOverhaul should do instead is **stay compatible**:
- The craft-from-chests feature takes ownership of a chest right before removing items from it. NetworkPerformanceSystem also moves chest ownership ahead of interactions.
- The two should cooperate, but that needs a multiplayer test: two players crafting from the same chests with NetworkPerformanceSystem installed.

## Suggested next steps

1. Paste the Reddit post's text, so this can be checked against what it actually describes.
2. Try NetworkPerformanceSystem on whoever hosts (or on the server), with everyone else unchanged.
3. If there's still lag, turn on its monitoring for a session and look at `nps_stats`. It shows whether a particular player's connection, or bandwidth, is the bottleneck.

Sources:
- Game code: `ZDOMan`, `ZSteamSocket`, `ZNetScene` in `assembly_valheim` (0.221.4 decompiled; 1.0 signatures)
- Thunderstore package listings and READMEs (thunderstore.io, 2026-09-29)
- [Supercraft: Valheim Better Networking Guide 2026](https://supercraft.host/wiki/valheim/better_networking/)
- [HostedGG: How to Fix Valheim Multiplayer Lag and Desync (2026)](https://hostedgg.com/blog/valheim-multiplayer-lag-fix)
- [HostMyGame: Valheim server lag, bases, latency and desynchronization](https://hostmygame.eu/en/guides/valheim/server-lag-desync)
