# Co-op lag and desync in Valheim

Researched 2026-09-29 for the Reddit thread ["I love this game, but it's outright dishonest to advertise 10-player crossplay servers"](https://www.reddit.com/r/valheim/comments/1wsz914/) (r/valheim, post and comments supplied by the user). Checked against the game's code and the networking mods on Thunderstore.

## The situation in the post

- A **dedicated server**, **10 players**, **one on PS5** through crossplay. Problems start with as few as 4–5 players.
- Symptoms:
  - enemies invisible to some players;
  - positions not updating for up to a minute;
  - loot and doors not responding;
  - Black Forest fights barely possible with 9 players.
- The commenters agree the cause is "first player into an area becomes its host". Suggested fixes:
  - send the best connection in first;
  - a "network traffic" mod (BetterNetworking named);
  - a mod that "makes the server handle mobs";
  - NetworkPerformanceSystem.
- Constraints raised in the thread:
  - the PS5 player can't install mods;
  - one player doesn't want everyone to need mods (achievements);
  - hosts prefer their own hardware to rented servers.

## What's actually going on (from the game code)

Read from the decompiled game code (build 0.221.4). The 1.0 reference assemblies only contain signatures, so these constants may have changed in 1.0.

**1. The first player in an area simulates it, for as long as they stay.**
- Every creature, chest, door, smelter and ship has one *owner*, whose game runs its AI and physics and decides its hits. Everyone else sees the owner's version, delayed by the owner's connection and frame rate.
- Ownership goes to a nearby player only when the object has no owner, or its owner has left the area. The game re-checks every 2 seconds (`ZDOMan.ReleaseNearbyZDOS`).
- Nothing ever moves ownership to the player with the better connection. The dedicated server only relays and stores; it doesn't simulate. That's why commenters are right that a dedicated server alone doesn't fix this.
- Loot and doors are objects with owners too, so opening them waits on the owner's round trip. That explains the "can't loot / can't open doors" reports.

**2. World updates get slower the more players there are.**
- About every 50 ms the server starts a round of updates, but it serves **one player per server frame** (`ZDOMan.SendZDOToPeers2`), and each player gets at most about 10 KB per turn (`SendZDOs`).
- With 10 players, a round takes 10 server frames, so each player gets updates roughly ten times less often than a solo player would. Busy areas then run into the 10 KB per-turn cap.
- That fits "positions don't update for a minute" and "enemies invisible" getting much worse as the player count grows.

**3. Crossplay changes the transport.**
- A crossplay server runs everyone's connection over PlayFab (`ZPlayFabSocket`) instead of Steam's networking.
- Steam's fixed 150 KB/s send rate (`ZSteamSocket`) and the mods that raise it (e.g. NetworkTweaks, "increased data transfer rate (only works for steam)") don't apply there.
- The PlayFab socket reports only a quarter of its in-flight data to the 10 KB cap, and adds relay latency.
- So on this server, **raising the Steam send rate, the classic "network traffic mod", does little or nothing.** The fix has to be ownership and scheduling.

Some claims in the thread are out of date:
- "Bandwidth hardcoded at ~30 kbps" was the early-access figure. The current code uses 150 KB/s on Steam.
- The BetterNetworking mod named in the thread was last updated in November 2023.

## What to do for this setup

The fix has to work **server-side only**, because the PS5 player can't run mods. That also means no player needs mods, so achievements aren't affected.

### 1. Install NetworkPerformanceSystem on the dedicated server (recommended)

[MidnightMods-NetworkPerformanceSystem](https://thunderstore.io/c/valheim/p/MidnightMods/NetworkPerformanceSystem/), 1.11.0, updated 2026-09-28. It addresses both causes above:
- It measures each player's round trip and moves object ownership away from poor connections. This is the fix for "whoever enters first owns it".
- It moves ownership of things you're about to use (chests, doors, stations) to you ahead of time.
- It replaces the one-player-per-frame update scheduler, and sends settled or distant creatures less often.

**Server-only works with vanilla clients**, including the PS5 player, and there's no version lock, so nobody is kicked for not having it. PC players can optionally install it for latency compensation and ship-helm handover. It lists Valheim Plus as compatible.

It has built-in diagnostics:
1. Set `Enable Network Monitoring = true` on the server.
2. Play a session.
3. Run `nps_stats` in the console. It shows per-player ping, packet loss and ownership changes, so the next problem can be measured rather than argued about.

**Caveats:**
- We haven't tested it.
- It's young (about 23k downloads) and changes often, so back up the world first.
- Its Steam-specific parts (loss backoff on the Steam transport) won't apply on a crossplay server. The ownership and scheduling fixes don't depend on the transport, but this is my reading of its README, not something its author states.

**Install only one networking mod.** They rewrite the same code, and NetworkPerformanceSystem refuses to load alongside FiresGhettoNetworking, SkadiNet, BetterNetworking, NetworkTweaks and others.

### Alternatives, and why they rank lower here

| Mod | Why not first choice for this server |
|---|---|
| [DeathMonger-SpreadTheLoad](https://thunderstore.io/c/valheim/p/DeathMonger/SpreadTheLoad/) | Server-only and fine for vanilla clients. It moves ownership to whoever is chopping, mining or steering, and away from players you name as slow. It doesn't fix the update scheduler (cause 2). A reasonable fallback if NetworkPerformanceSystem misbehaves. |
| [VerdantsAscent-FiresGhettoNetworking](https://thunderstore.io/c/valheim/p/VerdantsAscent/FiresGhettoNetworking/) | Its "server-side simulation" is what the thread's "server handles mobs" comment refers to. But by its own README, creatures are still simulated by the nearest client, only players with the mod can hand a creature over (so not the PS5 player), and the switch that truly moves creatures to the server is "not recommended" and least tested. Its compression also needs the mod on both sides. |
| SkadiNet | Requires the same version on the server and every client, so the PS5 player can't join. |
| NetworkTweaks, BetterNetworking (and forks), Smoothbrain Network | Mainly raise the Steam send rate, which a crossplay server doesn't use. |

A mod that makes the server run everything, like a conventional game server, doesn't reliably exist. The mods that try describe it as experimental, and ships and carts break first.

### 2. Habits that help, with or without a mod

- **Let the most stable connection enter first.** For bosses and raids, have one of the players with low ping to the server go through the portal first. The thread's Jeraz does this with good results. Ping to the server and connection stability matter more than CPU.
- **Reset a bad owner.** If one player makes an area lag, they portal out and back in, so ownership passes to someone else.
- **Ships:** the ship is simulated by its owner. Have the helmsman open the cargo hold before taking the helm, which makes them the owner (from the thread; SpreadTheLoad and NetworkPerformanceSystem automate this).
- **Smelting lag:** smelters and kilns are owned objects too. Whoever feeds a big smelting run should be the player with the steadiest connection.
- **Crossplay:** if the PS5 player isn't playing, run the server without `-crossplay` so everyone uses Steam's transport.

### 3. Writing our own networking code: still not recommended

- The problem in the thread is real and structural. Fully fixing it means moving simulation to the server, which even dedicated mods treat as experimental.
- The practical fixes (ownership rebalancing, better scheduling) already exist in maintained 1.0 mods that work with vanilla and console clients.
- Our own version would conflict with them, since networking mods refuse to load together.
- It can only be tested with many real players on real connections.

What CraftingOverhaul must do is **stay compatible**. The craft-from-chests feature takes ownership of a chest right before taking items from it. NetworkPerformanceSystem also moves chest ownership ahead of interactions. They should cooperate, but that needs a multiplayer test: two players crafting from the same chests, with NetworkPerformanceSystem on the server.

## Sources

- Game code: `ZDOMan`, `ZSteamSocket`, `ZPlayFabSocket`, `ZNet` in `assembly_valheim` (0.221.4 decompiled; 1.0 signatures)
- Thunderstore package listings and READMEs (thunderstore.io, 2026-09-29)
- The Reddit thread above, as supplied by the user
- [Supercraft: Valheim Better Networking Guide 2026](https://supercraft.host/wiki/valheim/better_networking/)
- [HostedGG: How to Fix Valheim Multiplayer Lag and Desync (2026)](https://hostedgg.com/blog/valheim-multiplayer-lag-fix)
- [HostMyGame: Valheim server lag, bases, latency and desynchronization](https://hostmygame.eu/en/guides/valheim/server-lag-desync)
