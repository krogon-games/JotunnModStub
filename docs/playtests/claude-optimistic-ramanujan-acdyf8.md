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

1. [ ] (`gt-1`) Press `K` → above the three tabs is a small card **Gungnir's Throw**, "Basic · always learned", lit like a learned skill. Click it → the details say it throws a gold ghost copy of your spear with the throw's damage and stamina, no eitr; the button at the bottom says "No points needed". The three tabs still have nine cards each.
2. [ ] (`gt-2`) Drag Gungnir's Throw onto slot F1 (no point needed), close the panel, press F1 with the spear in hand.
3. [ ] (`gt-3`) Right click at the dummy → your character throws, and a **see-through gold spear** with a soft glow and a faint trail flies where you aim. Your spear **stays in your hand** (nothing on the ground, still one spear in the inventory). Stamina drops as for a normal throw; eitr doesn't move.
4. [ ] (`gt-4`) The ghost **sticks** in the dummy (or the ground, if you miss) and **fades out in about a second**. Nothing to pick up.
5. [ ] (`gt-5`) Damage: throw the real spear the game's way (middle mouse) at the dummy and note the number; pick the spear up again. Then Gungnir's Throw → about the same number (both vary a little from throw to throw).
6. [ ] (`gt-6`) Hold right click → it throws again and again while stamina lasts; let go → it stops. Left click still pokes, and middle mouse still throws the real spear as before.
7. [ ] (`gt-7`) Spears skill: open the game's own Skills list (in the inventory) and note Spears; hit the dummy a few times with Gungnir's Throw → Spears goes up, as it does with the game's throw.
8. [ ] (`gt-8`) Abyssal Harpoon: `spawn SpearChitin`, equip it, `spawn Boar` some distance away. Gungnir's Throw at the boar → a gold ghost harpoon, **no rope and no pull**. (The harpoon's own middle-mouse throw still pulls.)

**Lightning Bolt**

9. [ ] (`lb-1`) Back to the bronze spear. In the javelin tab click **Lightning Bolt** → "75% of the throw's damage, all of it lightning … 2× a thrown spear's speed"; cost 6 eitr. Learn one rank (`orlog_skill LightningBolt 1` works too), drag it to F2, press F2.
10. [ ] (`lb-2`) Right click at the dummy → the gold spear **crackles with blue-white lightning**, flies clearly **faster and flatter** than Gungnir's Throw, and **bursts in lightning** on the dummy. Eitr drops by 6 and stamina by the throw's.
11. [ ] (`lb-3`) Damage: the number is about **three quarters** of Gungnir's Throw's (step 5), shown in the lightning colour. Then `orlog_skill LightningBolt 20` → about **twice** Gungnir's Throw's.
12. [ ] (`lb-4`) Stops at the first enemy: `spawn TrainingDummy` again so two stand one behind the other. Lightning Bolt at the front one → only the front one takes damage.

**Plague Javelin**

13. [ ] (`pj-1`) Click **Plague Javelin** → "100% of the throw's damage; … a poison cloud spreads out to 4 m over 3 s, poisoning everything in it for 25% of the throw's damage each second"; cost 7 eitr. Learn one rank, drag it to F3, press F3.
14. [ ] (`pj-2`) Throw at the ground just in front of the dummy → **green drips** fall from the ghost in flight. Where it lands: a green burst and a **green ring that grows from nothing to 4 m over about 3 s**, then fades.
15. [ ] (`pj-3`) Throw at the dummy itself → the hit is about Gungnir's Throw's number; then the dummy shows **poison numbers each second**, each about a quarter of the hit, while the cloud lasts.
16. [ ] (`pj-4`) `orlog_skill PlagueJavelin 20` → the cloud now spreads over about **7 s** and each poisoning is about **as big as the hit**.

**Checks**

17. [ ] (`c6-1`) Out of eitr: cast Lightning Bolt until eitr is empty → right click says "Not enough eitr" and nothing is thrown. Press F1 (Gungnir's Throw) → it still throws (no eitr needed).
18. [ ] (`c6-2`) Your verdict: the gold ghost, Lightning Bolt's and Plague Javelin's looks, and how throwing from right click feels. Anything to change?
