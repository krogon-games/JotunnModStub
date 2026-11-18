# Play-tests: `claude/amazon-snipe-shackle-shot-m67buh`

## Round: Snipe and Shackle Shot (2026-11-18)

**Build:** the three DLLs from this branch (Orlog and Corruption changed). The log lines should read `Orlog 0.7.0 loaded (build: claude/amazon-snipe-shackle-shot-m67buh <commit>)` and the same for Corruption (versions are raised when the branch merges).
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`. `orlog_class Amazon` if the character has no class, then `orlog_level 40` (for eitr and points). `raiseskill Bows 100`, so the bow draws fast. `spawn Bow`, `spawn ArrowWood 100`. `orlog_respec` if your points are spent. Best in the Black Forest (trees and greydwarfs).

**What's new:**
- **Snipe:** hold right click past full draw to charge. The charge fills in 3.5 s at skill level 1 (1.5 s at 20). The longer you charge, the harder and faster the arrow: from a normal arrow up to 150% of a normal shot's damage at full charge (300% at level 20), slow at first and fastest near the end. The view zooms in to 3× at full charge, and stamina drains faster than with a normal drawn bow. Costs 9 eitr.
- **Shackle Shot:** the arrow hits for a normal shot's damage (150% at level 20). If another enemy or a tree is up to 8 m behind the enemy you hit, a rope ties them and both are stunned for 1.75 s (4 s at level 20): no moving, no attacking, gold swirls above their heads. With nothing behind, the enemy is stunned for only 0.6 s. Bosses are only slowed. Costs 9 eitr.
- **Both work with crossbows.**
- **Your answer on area hits:** the burning ground's and the poison cloud's ticks no longer trigger on-hit affixes (life steal and the rest); Immolation Arrow's blast still does.

If something looks wrong, tell me what you saw and send `BepInEx/LogOutput.log`: every Snipe shot and every shackle writes a line with its numbers.

**My own choices, open to change:** the arrow's speed rises evenly with the charge (the damage follows your "slow start, big finish" curve); the extra stamina stops once the charge is full; mouse sensitivity stays the same while zoomed; Snipe's charge circle looks like Wind Lance's (pale blue), but with a ring sound and no wind at full charge. The rope and swirls are my first look, made from the game's own pieces (the harpoon's rope if the game has it). Only you see them: another player sees the creatures stand still, without the rope or swirls.

**Phone page:** the same steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: Snipe and Shackle Shot" (step ids `sn-1` … `sn-10`, `sh-1` … `sh-10`, `dot-1` … `dot-2`).

### Checklist

**Snipe**

1. [ ] (`sn-1`) Press `K`, Bow and Crossbow tab: no card says "Coming later" any more. Click **Snipe** → its details read "Hold past full draw to charge for up to 3.5 s … up to 150% damage at 2× an arrow's speed. The view zooms in up to 3× while charging …". The cost shown is 9 eitr. Learn one rank (`orlog_skill Snipe 1` works too), drag Snipe onto slot F1, press F1.
2. [ ] (`sn-2`) Hold right click → the bow draws as usual. Once it's fully drawn, keep holding: the yellow circle keeps narrowing and turns pale blue, and the view slowly zooms in. After about 3.5 s the circle pulses, a ring sounds, and things look about three times as close.
3. [ ] (`sn-3`) Let go → the arrow flies noticeably faster and flatter than a normal one, and the view springs back to normal. Your eitr drops by 9.
4. [ ] (`sn-4`) Damage: `spawn Troll 1` and `god`. Shoot one normal arrow with the left mouse button at full draw and note the number (about 40). Then a fully charged Snipe → about one and a half times that (about 60). Then one let go at about half the charge (half the circle's narrowing) → only a little more than a normal arrow (about 45).
5. [ ] (`sn-5`) Let go before the bow is fully drawn → a normal, weaker arrow; no zoom, and your eitr doesn't drop.
6. [ ] (`sn-6`) Stamina: type `god` again to turn it off. Hold Snipe and watch the stamina bar → it drains faster while the charge builds than when you just hold a drawn bow with the left mouse button, and slows to the normal rate once the charge is full. If you run out while charging, the bow lowers and nothing is shot (no eitr).
7. [ ] (`sn-7`) Long shot: find something 50+ m away (a tree trunk or a creature) and shoot a full Snipe at it, then a normal arrow aimed the same way → Snipe's arrow drops much less.
8. [ ] (`sn-8`) Level 20: `orlog_skill Snipe 20` → the full charge takes about 1.5 s, and a full Snipe on the troll deals about three times a normal arrow (about 120).
9. [ ] (`sn-9`) Crossbow: `spawn CrossbowArbalest`, `spawn BoltBone 50`, equip it with Snipe on F1 and wait until it's loaded. Hold right click → no bolt leaves; the circle runs and the view zooms in; your stamina drains. Let go → the bolt flies, harder than a normal bolt (shoot one with the left mouse button first to compare); the crossbow reloads.
10. [ ] (`sn-10`) The older charged skill: Wind Lance on F2 still charges as before (wind at the bow, no zoom) and flies as a gust.

**Shackle Shot**

11. [ ] (`sh-1`) Click **Shackle Shot** on the bow tab → its details read "The arrow deals 100% damage and shackles the enemy it hits to another enemy or a tree up to 8 m behind it: both are stunned for 1.75 s (with nothing behind it, 0.6 s). Bosses are only slowed". The cost shown is 9 eitr. Learn one rank, drag it onto slot F3, press F3. Back to the bow.
12. [ ] (`sh-2`) Two enemies: `god`, `spawn Greydwarf 2`, and back off so they follow you in a line. Shoot the front one → a rope ties it to the one behind it, gold swirls circle above both heads, and both stand still, without attacking or turning, for about 2 seconds; then they come at you again.
13. [ ] (`sh-3`) A tree: shoot an enemy that stands just in front of a tree (the tree behind it, as seen from you) → the rope goes from the enemy to the tree, and the enemy is stunned for about 2 seconds.
14. [ ] (`sh-4`) Nothing behind: shoot an enemy in the open, with nothing behind it → no rope, short swirls, and it stops for only a blink (0.6 s).
15. [ ] (`sh-5`) No attacking: let a greydwarf come right up to you, step back a little and shackle it (with another one or a tree behind it) → it doesn't swing at you while the swirls show.
16. [ ] (`sh-6`) Damage: on the troll, a Shackle Shot's number is about the same as a normal arrow's at level 1 (`orlog_skill ShackleShot 20`: about one and a half times, and the stun lasts about 4 s).
17. [ ] (`sh-7`) Boss: `spawn Eikthyr` (`god` on) and Shackle him → he isn't stopped, only slowed (about half speed), and he has no swirls.
18. [ ] (`sh-8`) A flier: `spawn Deathsquito` (with `god`) and Shackle it with nothing behind → it stops in the air for a blink. Two of them in a line (`spawn Deathsquito 2`): both hang still for the full time.
19. [ ] (`sh-9`) Crossbow: Shackle Shot on the crossbow works the same.
20. [ ] (`sh-10`) Your verdict on both skills: the feel of Snipe's charge and zoom, and the rope and swirls' look.

**Area hits and on-hit affixes (your answer)**

21. [ ] (`dot-1`) Bow in hand: `co_affix life_steal 10` (the tooltip shows the life steal). `orlog_skill VenomStrike 20`, put Venom Strike on F4. Lose some health (jump off something high, `god` off). Shoot the **ground** in front of a troll so it walks into the cloud (the arrow mustn't hit it) → your health doesn't jump up while the troll is poisoned (it only creeps up with normal regeneration).
22. [ ] (`dot-2`) Same with Immolation Arrow (`orlog_skill ImmolationArrow 20`) on the ground beside the troll → one small jump of health from the blast, then nothing more while it stands in the burning ground.
