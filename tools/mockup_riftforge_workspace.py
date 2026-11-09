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
    # Bench: in the recipe list's own inset.
    bw, bh = 210, h - 124
    P.paste(canvas, P.sliced(P.art("inset"), bw, bh, 10), x + 16, top)
    d = ImageDraw.Draw(canvas)
    cx = x + 16 + bw / 2
    P.text(d, (cx, top + 22), "Forge", 20, ORANGE, "mm")
    M.slot(canvas, cx - 48, top + 44, 96, M.item_icon(), glow=None if confirm else (255, 240, 200))
    d = ImageDraw.Draw(canvas)
    P.text(d, (cx, top + 160), "Vigorous Bronze Spear", 15, MAGIC, "mm")
    P.text(d, (cx, top + 178), "of Embers", 15, MAGIC, "mm")
    if confirm:
        P.text(d, (cx, top + 214), "Right-click an orb to pick it up", 13, DIM, "mm")
    else:
        P.text(d, (cx, top + 210), "Regal Orb", 15, ORANGE, "mm")
        P.text(d, (cx, top + 230), "Click the forge to use it", 13, DIM, "mm")
    P.text(d, (cx, top + bh - 110), "Vigorous Bronze Spear of Embers", 13, GREEN, "mm")
    P.text(d, (cx, top + bh - 92), "rerolled: +18% weapon damage;", 13, GREEN, "mm")
    P.text(d, (cx, top + bh - 74), "+20% fire damage", 13, GREEN, "mm")
    P.button(canvas, (x + 32, top + bh - 52, bw - 32, 38), "Remove")
    # Description: the game's own panel, copied.
    dx, dw = x + 238, w - 254
    P.paste(canvas, P.sliced(P.art("inset"), dw, bh, 10), dx, top)
    P.paste(canvas, P.sized(M.item_icon(), 64, 64), dx + 14, top + 12)
    d = ImageDraw.Draw(canvas)
    P.text(d, (dx + 90, top + 20), "Vigorous Bronze Spear", 19, MAGIC, "la")
    P.text(d, (dx + 90, top + 44), "of Embers", 19, MAGIC, "la")
    lines = [("A spear of bronze, tipped for hunting.", LIGHT, 14), ("", LIGHT, 8), ("One-handed", LIGHT, 14),
             ("Weight: 1.0", LIGHT, 14), ("Durability: 200/200", LIGHT, 14), ("Pierce: 35", LIGHT, 14), ("", LIGHT, 8),
             ("Magic", MAGIC, 15), ("+18% weapon damage (+6) [4–19]", MAGIC, 14), ("+20% fire damage (+7) [10–30]", MAGIC, 14)]
    yy = top + 90
    for text, colour, size in lines:
        if text:
            P.text(d, (dx + 16, yy), text, size, colour, "la")
        yy += size + 8
    # Cost slot (what one click uses) and the main button.
    if not confirm:
        M.slot(canvas, dx + 16, top + bh - 70, 56, icons["regal"], count=None)
        d = ImageDraw.Draw(canvas)
        P.text(d, (dx + 44, top + bh - 8), "Regal Orb (18)", 12, LIGHT, "mm")
        P.text(d, (dx + 66, top + bh - 64), "1", 14, LIGHT, "ra")
    P.button(canvas, (dx + dw - 150, top + bh - 52, 136, 38), "SHATTER", enabled=confirm)
    if not confirm:
        d = ImageDraw.Draw(canvas)
        P.text(d, (dx + dw - 82, top + bh - 64), "Shattering comes with #29", 11, DIM, "mm")
        # The pick follows the cursor.
        ic = P.sized(icons["regal"], 40, 40)
        P.paste(canvas, ic, cx + 30, top + 96)
        P.badge(canvas, cx + 68, top + 134, 18, LIGHT)
        return
    # The game's own Yes/No box.
    bx, by, bw2, bh2 = x + 140, y + 170, 380, 230
    d = ImageDraw.Draw(canvas)
    d.rectangle([bx * S, by * S, (bx + bw2) * S, (by + bh2) * S], fill=(20, 14, 10, 252))
    P.paste(canvas, P.sliced(P.art("panel"), bw2, bh2, 20), bx, by)
    d = ImageDraw.Draw(canvas)
    P.text(d, (bx + bw2 / 2, by + 26), "Shatter", 20, ORANGE, "mm", title=True)
    P.text(d, (bx + bw2 / 2, by + 62), "Vigorous Bronze Spear of Embers will be destroyed.", 13, RED, "mm")
    P.text(d, (bx + bw2 / 2, by + 92), "Each line has a 50% chance to become a splinter:", 13, LIGHT, "mm")
    P.text(d, (bx + bw2 / 2, by + 112), "50%  +18% weapon damage   ·   50%  +20% fire damage", 13, MAGIC, "mm")
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
