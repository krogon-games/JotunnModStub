"""
Mock-ups of the Riftforge workspace as built in round claude/lucid-carson-hs5g89 (issue #30, Krogon's brief of
2026-11-09): the six inventory tabs with Currencies' fixed places, and the Riftforge's third tab ("Forge") in the
game's own crafting-panel layout (Krogon's feedback the same day: match the Craft and Upgrade tabs): the bench with the
forge slot where the recipe list is, a copy of the game's description panel where the description is. A second picture
shows the Shatter confirmation in the game's Yes/No box (which waits for #29's rules). Replaces the two-slot, operation-list mock-ups of tools/mockup_riftforge.py.

    python tools/mockup_riftforge_workspace.py      (writes into docs/images/)

Item names, lines and counts are samples. The in-game layout uses the game's own panel, cells and fonts; this is a
picture of the arrangement, not of the exact art.
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import preview_skills_panel as P  # noqa: E402
import mockup_riftforge as M  # noqa: E402

OUT = M.OUT
S = P.S
GOLD, LIGHT, DIM, RED, GREEN = P.GOLD, P.LIGHT, P.LIGHT_DIM, P.RED, P.CAN_SPEND
MAGIC = M.MAGIC
RARE = M.RARE
ORANGE = (255, 161, 60)
TABS = ["Bag", "Currencies", "Splinters", "Maps", "Runes", "Essences"]  # no Skills button (removed 2026-11-09)
# Currencies' fixed places: the shard, then the ten orbs in the design's order (FixedLayout / InventoryTabs.HomesOf).
HOMES = ["shard", "transmutation", "augmentation", "alchemy", "regal", "exalted", "divine", "annulment",
         "chance", "jeweller", "mirror"]
COUNTS = {"shard": 3, "transmutation": 12, "augmentation": 0, "alchemy": 4, "regal": 2, "exalted": 0, "divine": 1,
          "annulment": 5, "chance": 3, "jeweller": 1, "mirror": 0}
# With a Magic spear (2 affixes) in the slot: what can't be used on it is tinted red.
INCOMPATIBLE = {"transmutation", "alchemy", "chance", "jeweller", "shard"}


def tint_red(icon):
    return P.tinted(icon, (255, 110, 110), 0.55)


def inventory(canvas, x, y, icons):
    w, h = 600, 380
    P.paste(canvas, P.sliced(P.art("panel"), w, h, 20), x, y)
    d = ImageDraw.Draw(canvas)
    bw = (w - 28 - 5 * 4) / 6
    for i, name in enumerate(TABS):
        bx = x + 14 + i * (bw + 4)
        P.button(canvas, (bx, y - 34, bw, 30), name, enabled=name != "Currencies")
        d = ImageDraw.Draw(canvas)
        if name == "Currencies":
            P.text(d, (bx + bw / 2, y - 19), name, 15, ORANGE, "mm")
    cell, gap = 62, 8
    gx, gy = x + (w - (8 * cell + 7 * gap)) / 2, y + 40
    for row in range(4):
        for col in range(8):
            i = row * 8 + col
            cx, cy = gx + col * (cell + gap), gy + row * (cell + gap)
            key = HOMES[i] if i < len(HOMES) else None
            icon = icons.get(key) if key else None
            if key == "mirror":
                icon = icons.get("divine")  # placeholder: the Mirror's art isn't made yet
            count = COUNTS.get(key) if key else None
            dim = key is not None and count == 0
            if icon is not None and key in INCOMPATIBLE and not dim:
                icon = tint_red(icon)
            glow = (255, 240, 200) if key == "regal" else None
            M.slot(canvas, cx, cy, cell, icon, glow=glow, count=count if key else None, dim=dim)
    d = ImageDraw.Draw(canvas)
    P.text(d, (x + 20, y + h - 52), "Fixed places: an orb you have none of stays dimmed with a 0.", 14, DIM, "la")
    P.text(d, (x + 20, y + h - 32), "Red: can't be used on the item in the forge. Gold glow: your pick.", 14, DIM, "la")


def workspace(canvas, x, y, icons, confirm=False):
    """The Riftforge panel with the Forge tab open: the game's header and tabs; the bench where the recipe list was;
    a copy of the game's description panel (icon, name, tooltip, cost slots, main button) where the description was."""
    w, h = 640, 540
    P.paste(canvas, P.sliced(P.art("panel"), w, h, 20), x, y)
    d = ImageDraw.Draw(canvas)
    P.text(d, (x + w / 2, y + 34), "RIFTFORGE", 26, ORANGE, "mm", title=True)
    for i, name in enumerate(["CRAFT", "UPGRADE", "FORGE"]):
        P.button(canvas, (x + 40 + i * 118, y + 62, 112, 30), name, enabled=name != "FORGE")
    d = ImageDraw.Draw(canvas)
    P.text(d, (x + 40 + 2 * 118 + 56, y + 77), "FORGE", 15, ORANGE, "mm")
    top = y + 104
    # Bench: in the recipe list's own inset: the forge slot, the gear's name, its affixes (Krogon, 2026-11-10).
    bw, bh = 236, h - 124
    P.paste(canvas, P.sliced(P.art("inset"), bw, bh, 10), x + 16, top)
    d = ImageDraw.Draw(canvas)
    cx = x + 16 + bw / 2
    P.text(d, (cx, top + 20), "Forge", 20, ORANGE, "mm")
    M.slot(canvas, cx - 44, top + 38, 88, M.item_icon(), glow=None if confirm else (255, 240, 200))
    d = ImageDraw.Draw(canvas)
    P.text(d, (cx, top + 146), "Grim Wail", 18, RARE, "mm")
    ax, ay = x + 30, top + 172
    P.text(d, (ax, ay), "Rare Bronze Sword", 16, RARE, "la")
    affixes = [("+4% movement speed while in", "your hand [2–6]", False),
               ("Parrying gives you and nearby", "allies +18% armour [10–30]", not confirm),
               ("+31% of physical damage as", "lightning (+11) [10–50]", False)]
    yy = ay + 30
    for first, second, changed in affixes:
        if changed:
            band = Image.new("RGBA", (int((bw - 20) * S), int(46 * S)), (232, 198, 106, 64))
            canvas.alpha_composite(band, (int((ax - 6) * S), int((yy - 4) * S)))
            d = ImageDraw.Draw(canvas)
        P.text(d, (ax, yy), first, 15, RARE, "la")
        P.text(d, (ax, yy + 19), second, 15, RARE, "la")
        yy += 50
    P.button(canvas, (x + 36, top + bh - 52, bw - 40, 38), "Remove")
    # Description: the game's own panel, copied, without its icon: name and the routine stats.
    dx, dw = x + 262, w - 278
    P.paste(canvas, P.sliced(P.art("inset"), dw, bh, 10), dx, top)
    d = ImageDraw.Draw(canvas)
    P.text(d, (dx + 16, top + 30), "Grim Wail", 24, RARE, "lm", title=True)
    lines = ["Blood-drinker. A thirsty friend.", "", "One-handed", "Weight: 0.8", "Durability: 100% (199/200)",
             "Slash: 35 (11–22)", "Lightning: 11 (3–7)", "Block armour: 12 (13)", "Parry bonus: 2x", "Knockback: 40", "Backstab: 3x"]
    yy = top + 62
    for text in lines:
        if text:
            P.text(d, (dx + 16, yy), text, 14, LIGHT, "la")
        yy += 21 if text else 10
    # Where the cost slots were: the last result, across the whole width.
    ry = top + bh - 126
    if not confirm:
        P.text(d, (dx + 16, ry), "Regal Orb: now Rare, 1 affix added", 16, (200, 235, 160), "la")
        P.text(d, (dx + 16, ry + 22), "+ Parrying gives you and nearby allies +18% armour", 15, (184, 240, 138), "la")
    P.button(canvas, (dx + dw - 160, top + bh - 52, 146, 38), "SHATTER", enabled=confirm)
    if not confirm:
        # The pick follows the cursor.
        ic = P.sized(icons["regal"], 40, 40)
        P.paste(canvas, ic, cx + 30, top + 92)
        P.badge(canvas, cx + 68, top + 130, 18, LIGHT)
        return
    # The game's own Yes/No box.
    bx, by, bw2, bh2 = x + 140, y + 170, 380, 230
    d = ImageDraw.Draw(canvas)
    d.rectangle([bx * S, by * S, (bx + bw2) * S, (by + bh2) * S], fill=(20, 14, 10, 252))
    P.paste(canvas, P.sliced(P.art("panel"), bw2, bh2, 20), bx, by)
    d = ImageDraw.Draw(canvas)
    P.text(d, (bx + bw2 / 2, by + 26), "Shatter", 20, ORANGE, "mm", title=True)
    P.text(d, (bx + bw2 / 2, by + 62), "Grim Wail will be destroyed.", 13, RED, "mm")
    P.text(d, (bx + bw2 / 2, by + 92), "Each line has a 50% chance to become a splinter:", 13, LIGHT, "mm")
    P.text(d, (bx + bw2 / 2, by + 112), "50% for each of its 3 affix lines", 13, RARE, "mm")
    P.text(d, (bx + bw2 / 2, by + 136), "(chances from shattering.csv, through #29's rules)", 11, DIM, "mm")
    P.button(canvas, (bx + 40, by + bh2 - 54, 140, 36), "No")
    P.button(canvas, (bx + 200, by + bh2 - 54, 140, 36), "Yes")


def picture(name, confirm):
    icons = M.orb_icons()
    W, H = 1300, 600
    canvas = Image.new("RGBA", (W * S, H * S), (24, 20, 17, 255))
    inventory(canvas, 20, 80, icons)
    workspace(canvas, 640, 40, icons, confirm)
    canvas = canvas.resize((W, H), Image.LANCZOS)
    path = os.path.join(OUT, name)
    canvas.convert("RGB").save(path)
    print(path)


if __name__ == "__main__":
    picture("riftforge_workspace.png", confirm=False)
    picture("riftforge_workspace_shatter.png", confirm=True)
