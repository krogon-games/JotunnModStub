# Play-tests: `claude/optimistic-ramanujan-acdyf8`

## Round: Gungnir's Throw, Lightning Bolt and Plague Javelin (2026-11-28)

**Build:** the three DLLs from this branch (Orlog and CraftingOverhaul changed). The log should read `Orlog 0.10.0 loaded (build: claude/optimistic-ramanujan-acdyf8 <commit>)` (versions are raised when the branch merges), and early on `Orlog: made the ghost javelins for N spear throws.`
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`. `orlog_class Amazon` if the character has no class, then `orlog_level 40` (eitr and points). `spawn SpearBronze`, equip it. `spawn TrainingDummy` about 10 m in front of you. `orlog_respec` if your points are spent.

**What's new:**
- **Gungnir's Throw:** a basic skill everyone with a class has, no points. A small "Basic" card above the skill tabs; put it in a slot, and right click with a spear throws a see-through gold ghost of the spear where you aim. Your spear stays in your hand. Same stamina and damage as the game's own throw; no eitr.
- **Lightning Bolt:** the ghost wrapped in lightning, twice as fast, stops at the first enemy. 75% of the throw's damage at level 1 (200% at 20), all lightning. 6 eitr plus the throw's stamina.
- **Plague Javelin:** the ghost drips green; it hits for the throw's damage, then a poison cloud spreads out to 4 m over 3 s (6.8 s at level 20), poisoning everything inside each second for 25% of the throw's damage (100% at 20). 7 eitr plus the throw's stamina.

If something looks wrong, tell me what you saw and send `BepInEx/LogOutput.log`: every skill throw writes a line with its numbers ("Lightning Bolt thrown (rank 1): 75% of the throw's damage …", and "Plague Javelin (level 1): …" when a cloud ends).

**My own choices, open to change:** holding right click keeps throwing, one after another, as holding the game's throw does; the cloud poisons once a second starting a second after landing (so at level 20 its last poisoning reaches about 3.5 m while the ring reaches 4 m); the Basic card's look (the same wood and medallion as the other cards). The cloud's ring shows on your game only, like Venom Strike's.

**Phone page:** the same steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: Gungnir's Throw and the javelin skills (#41)" (step ids `gt-1` … `gt-8`, `lb-1` … `lb-4`, `pj-1` … `pj-4`, `c6-1` … `c6-2`).

### Checklist

**Gungnir's Throw**

1. [x] (`gt-1`) Press `K` → above the three tabs is a small card **Gungnir's Throw**, "Basic · always learned", lit like a learned skill. Click it → the details say it throws a gold ghost copy of your spear with the throw's damage and stamina, no eitr; the button at the bottom says "No points needed". The three tabs still have nine cards each.
2. [x] (`gt-2`) Drag Gungnir's Throw onto slot F1 (no point needed), close the panel, press F1 with the spear in hand.
3. [ ] ✗ (`gt-3`) Right click at the dummy → your character throws, and a **see-through gold spear** with a soft glow and a faint trail flies where you aim. Your spear **stays in your hand** (nothing on the ground, still one spear in the inventory). Stamina drops as for a normal throw; eitr doesn't move.
4. [ ] ✗ (`gt-4`) The ghost **sticks** in the dummy (or the ground, if you miss) and **fades out in about a second**. Nothing to pick up.
5. [x] (`gt-5`) Damage: throw the real spear the game's way (middle mouse) at the dummy and note the number; pick the spear up again. Then Gungnir's Throw → about the same number (both vary a little from throw to throw).
6. [x] (`gt-6`) Hold right click → it throws again and again while stamina lasts; let go → it stops. Left click still pokes, and middle mouse still throws the real spear as before.
7. [x] (`gt-7`) Spears skill: open the game's own Skills list (in the inventory) and note Spears; hit the dummy a few times with Gungnir's Throw → Spears goes up, as it does with the game's throw.
8. [ ] ✗ (`gt-8`) Abyssal Harpoon: `spawn SpearChitin`, equip it, `spawn Boar` some distance away. Gungnir's Throw at the boar → a gold ghost harpoon, **no rope and no pull**. (The harpoon's own middle-mouse throw still pulls.)

**Lightning Bolt**

9. [x] (`lb-1`) Back to the bronze spear. In the javelin tab click **Lightning Bolt** → "75% of the throw's damage, all of it lightning … 2× a thrown spear's speed"; cost 6 eitr. Learn one rank (`orlog_skill LightningBolt 1` works too), drag it to F2, press F2.
10. [ ] ✗ (`lb-2`) Right click at the dummy → the gold spear **crackles with blue-white lightning**, flies clearly **faster and flatter** than Gungnir's Throw, and **bursts in lightning** on the dummy. Eitr drops by 6 and stamina by the throw's.
11. [x] (`lb-3`) Damage: the number is about **three quarters** of Gungnir's Throw's (step 5), shown in the lightning colour. Then `orlog_skill LightningBolt 20` → about **twice** Gungnir's Throw's.
12. [x] (`lb-4`) Stops at the first enemy: `spawn TrainingDummy` again so two stand one behind the other. Lightning Bolt at the front one → only the front one takes damage.

**Plague Javelin**

13. [x] (`pj-1`) Click **Plague Javelin** → "100% of the throw's damage; … a poison cloud spreads out to 4 m over 3 s, poisoning everything in it for 25% of the throw's damage each second"; cost 7 eitr. Learn one rank, drag it to F3, press F3.
14. [ ] ✗ (`pj-2`) Throw at the ground just in front of the dummy → **green drips** fall from the ghost in flight. Where it lands: a green burst and a **green ring that grows from nothing to 4 m over about 3 s**, then fades.
15. [x] (`pj-3`) Throw at the dummy itself → the hit is about Gungnir's Throw's number; then the dummy shows **poison numbers each second**, each about a quarter of the hit, while the cloud lasts.
16. [x] (`pj-4`) `orlog_skill PlagueJavelin 20` → the cloud now spreads over about **7 s** and each poisoning is about **as big as the hit**.

**Checks**

17. [x] (`c6-1`) Out of eitr: cast Lightning Bolt until eitr is empty → right click says "Not enough eitr" and nothing is thrown. Press F1 (Gungnir's Throw) → it still throws (no eitr needed).
18. [ ] ✗ (`c6-2`) Your verdict: the gold ghost, Lightning Bolt's and Plague Javelin's looks, and how throwing from right click feels. Anything to change?

### Your feedback (2026-11-28, phone page)
12 of 18 passed. Your notes, word for word:
- `gt-3` failed: "its definitely not see-through or gold, no glow. spear stays in hand. nothing to pick up. the projectile looks identical to the normal vanilla throw. stamina drops, eitr doesnt"
- `gt-4` failed: "you cant see the spear at all past the initial impact with a target."
- `gt-8` failed: "with abyssal harpoon, the vanilla interaction is left click to throw the harpoon. there is no middle mouse throw. Tested now - rightclick gungnirs throw does nothing."
- `lb-2` failed: "there is animation and vfx on the dummy of lightning on impact. but same as the throw - the projectile looks identical to basic throw. it does fly faster though."
- `pj-2` failed: "the spear in flight looks identical to the vanilla throw. i do see the green ring growing."
- `c6-2` (verdict) failed: "see other notes. Also the visual of the plaguejavelin green ring stands out too much. it would be better if the poison cloud grew in size instead. i know youre using the glowing ring for quite a few skills now. i may want to revisit the design of this ring for all relevant skills in the future. add it as a ticket to work on in the future. keep the ring for now, maybe it give it a slightly less intrusive and blended feel. fix the other outlined issues."

### What changed after the feedback
- `gt-3`, `gt-4`, `lb-2`, `pj-2`: **the ghost copies were never made.** They were built when the game first loads its prefabs, but at that moment the game's item list is still empty, so no spear was found and nothing was logged; every throw then flew as the game's own spear (only the speed, no-drop and damage changes, which don't need the copy, worked). They are now built from the game's loaded prefabs, again once the game's prefabs are registered, and on the first throw if one is still missing; the log says how many were made. The ghost also keeps its model after impact (the game hides a thrown spear's model on impact, because the spear to pick up replaces it). The look is now split in two (the gold tint; the light, trail, sparks and drips on a child object of their own), so one failing can't stop the other, and the first ghost thrown writes one line to the log saying what it got.
- `gt-8`: the Abyssal Harpoon's throw is its left-click attack. Gungnir's Throw (and the javelin skills) now use the main attack for a weapon whose secondary attack doesn't throw; the ghost harpoon has no rope and no pull.
- `c6-2`: Plague Javelin's cloud now grows: soft green puffs fill the cloud's reach as it spreads to 4 m. The ring stays, thinner and half see-through, with a dimmer light (your decision 17). Venom Strike's and Immolation Arrow's rings are unchanged. The ring's design for all area skills is a ticket for later: [#105](https://github.com/krogon-games/JotunnModStub/issues/105) (decision 18).

## Round 2: re-test of the ghost javelin's look, the harpoon and the cloud (2026-11-28)

**Build:** the three DLLs from this branch again (Orlog changed since round 1). **Setup:** as round 1 (bronze spear, the dummy, Gungnir's Throw on F1, Lightning Bolt on F2, Plague Javelin on F3).

**Phone page:** the first section, "This round: re-test, the ghost javelin's look (#41)" (step ids `r2-1` … `r2-8`). Round 1's steps are off the page.

1. [ ] (`r2-1`) After loading a world, `BepInEx/LogOutput.log` has `Orlog: made the ghost javelins for N more spear throws`, with N about 20 (0 on the second line is fine). After your first Gungnir's Throw: `Orlog: ghost javelin …: X model part(s) tinted, light on, trail on.` Copy both lines into the note.
2. [ ] (`r2-2`) Gungnir's Throw at the dummy → the spear in flight is **see-through gold** with a soft gold light and a faint trail; it doesn't look like the real spear.
3. [ ] (`r2-3`) The ghost **stays visible where it sticks** (in the dummy, or the ground if you miss) and **fades out in about a second**.
4. [ ] (`r2-4`) Lightning Bolt → the gold spear **crackles with blue-white lightning** in flight, faster than Gungnir's Throw.
5. [ ] (`r2-5`) Plague Javelin → **green drips** fall in flight; where it lands, **a soft green cloud spreads out** to about 4 m, with a **thin, faint ring** at its edge (much less bright than before).
6. [ ] (`r2-6`) Abyssal Harpoon (`spawn SpearChitin`, Gungnir's Throw on its slot): right click → a **gold ghost harpoon**, no rope and no pull. Left click is still the real harpoon throw, rope and pull included.
7. [ ] (`r2-7`) Your verdict on the looks now. Anything to change?
8. [ ] (`r2-8`) **The freeze test.** On 2026-11-29 the game froze during this re-test (`BepInEx/corruption_freeze.txt`: "the main thread is inside our mods' code: Orlog: \"set a ghost javelin's damage\""; you restarted, repeated the same thing and it didn't freeze). That code does nothing that can loop; its only slow part is writing a log line (one per skill throw). Claude's guess, not confirmed: on Windows, a click inside the BepInEx console window pauses it to select text, and the game then hangs at its next log line until a key is pressed there. You said this was "very possible but not sure". Test: with the console window open, click once inside it (its title starts with "Select"), go back to the game and throw Gungnir's Throw a few times. Does it freeze? If so, click the console and press `Esc`: does the game come back at once?
