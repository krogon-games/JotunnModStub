# Play-tests: `claude/elegant-maxwell-e7ys9o`

## Round: Strafe as rapid fire (2026-11-17)

**Build:** the three DLLs from this branch (only Orlog changed). The log line should read `Orlog 0.6.0 loaded (build: claude/elegant-maxwell-e7ys9o <commit>)` (the version is raised when it merges).
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`. `orlog_class Amazon` if the character has no class, then `orlog_level 30` (for the eitr and the points). `raiseskill Bows 100`, so the bow draws fast and the numbers are easy to compare. The starter bow: `spawn Bow`, `spawn ArrowWood 100`. `orlog_respec` if your points are spent.

**What changed:** Strafe is no longer a volley at several enemies. It's Diablo 3's Rapid Fire: hold right click, the bow draws once as usual, and once it's fully drawn arrows stream out where you aim, 6 a second, for as long as you hold. Each arrow deals 25% of a normal full shot (50% at skill level 20). It costs 10 eitr to start and 3 eitr a second while firing (15 and 5 at level 20), and takes one arrow a second from the quiver. You walk slowly and lose stamina as with a drawn bow. **Crossbows too:** with a loaded crossbow the stream starts as soon as you hold the button, 3 bolts a second at the same share; you can't run and lose stamina as with a drawn bow; when it stops the crossbow is empty and reloads.

This time I read the game's own bow code first, so it's built on how the game really draws and shoots. If something looks wrong, tell me what you saw and send `BepInEx/LogOutput.log`: each stream writes a line when it starts and one when it stops, with how many arrows flew and how much eitr it used.

**My own choices, open to change:** a stream's arrows push enemies back less and train your Bows skill less, in step with their smaller damage (otherwise 6 arrows a second would push six times as hard and train six times as fast as single shots). The bow wears once a second, with the arrow taken from the quiver, not with every arrow. Let go before full draw and it's one normal arrow with no eitr, as with Wind Lance. A crossbow's stream doesn't push you back with each bolt, as a single crossbow shot does.

**Phone page:** the same steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: Strafe as rapid fire" (step ids `rf-1` … `rf-15`).

### Checklist

**The card**

1. [ ] (`rf-1`) Press `K`, Bow and Crossbow tab, click **Strafe** → its details read "Hold to rapid-fire: after one full draw, fires 6 arrows a second where you aim for as long as you hold (a loaded crossbow: 3 bolts a second), each at 25% damage. Costs 10 eitr to start and 3 eitr a second while firing; uses 1 arrow or bolt a second; you walk and lose stamina as with a drawn bow". The cost shown is 10 eitr.
2. [ ] (`rf-2`) Learn one rank (`orlog_skill Strafe 1` works too), drag Strafe onto slot F1, press F1.

**Firing**

3. [ ] (`rf-3`) Hold right click → the bow draws as usual. As soon as it's fully drawn, arrows stream out where you aim, about 6 a second, each with the bow's release sound, flying and dropping like normal arrows. Aim around while holding: the stream follows your aim. Let go after 2–3 seconds → the stream stops at once, and **no extra arrow** leaves when you let go.
4. [ ] (`rf-4`) Damage: `spawn Troll 1` (lots of health) and `god`. First shoot one normal arrow at it with the left mouse button at full draw and note the number (about 40). Then Strafe at it → each arrow's number is about a quarter of that (about 10).
5. [ ] (`rf-5`) Let go early: tap right click so the bow isn't fully drawn → one normal, weaker arrow; your eitr doesn't drop.
6. [ ] (`rf-6`) Fire arrows: `spawn ArrowFire 20`, put them in your quiver, Strafe at the troll → the streamed arrows set it burning (the arrow's own fire carries).

**What it costs**

7. [ ] (`rf-7`) Eitr: watch the eitr bar. It drops by 10 when the stream starts, then keeps sinking steadily (about 3 a second) while you hold. Hold until it runs out → "Not enough eitr", the stream stops and the bow lowers. Nothing more fires until you let go and press again.
8. [ ] (`rf-8`) Arrows: note how many wood arrows you have, hold for about 5 seconds, let go → about 30 arrows flew, but the stack went down by only about 5 or 6.
9. [ ] (`rf-9`) Running out of arrows: drop all but 3 wood arrows (and the fire arrows), then hold → the stream runs for about 3 seconds, then the game says you're out of arrows and it stops. Pick the arrows back up afterwards.
10. [ ] (`rf-10`) Stamina and walking: type `god` again to turn it off. Hold Strafe and walk around → you walk at the slow pace of holding a drawn bow, and stamina drains as it does when you hold a drawn bow. If stamina runs out, the stream stops.

**Stopping it**

11. [ ] (`rf-11`) While the stream runs, press your block key (the mouse's back side button) → the stream stops. Opening the inventory (Tab) while firing stops it too.
12. [ ] (`rf-12`) Crossbow: `spawn CrossbowArbalest`, `spawn BoltBone 50`, equip it with Strafe on F1 and wait until it's loaded. Hold right click → bolts stream out at once where you aim, about 3 a second, with the crossbow's sound; it doesn't fire its own single shot. Each bolt's number is about a quarter of a normal bolt's (shoot one with the left mouse button first to compare). Hold for about 3 seconds, let go → about 9 bolts flew, the stack went down by about 3, and the crossbow reloads.
13. [ ] (`rf-13`) Crossbow, moving: with `god` off, hold Strafe and try to run → you only walk while it fires, and stamina drains as with a drawn bow. Hold it right after a shot, while it's still reloading → the stream starts as soon as it's loaded.

**Rank 20 and your verdict**

14. [ ] (`rf-14`) `orlog_skill Strafe 19` → the details say "each at 50% damage", "Costs 15 eitr to start and 5 eitr a second while firing", and the card's cost is 15. Each arrow hits the troll about half as hard as a full normal shot.
15. [ ] (`rf-15`) Your verdict: is this the machine gun you had in mind? Are 6 a second, the damage and the cost right? Is anything about the look or sound off (the bow stays in its drawn pose while the arrows leave it)?

### Your feedback

(none yet)
