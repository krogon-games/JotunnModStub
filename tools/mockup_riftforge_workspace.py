"""
Mock-ups of the Riftforge workspace as built in round claude/lucid-carson-hs5g89 (issue #30, Krogon's brief of
2026-11-09): the six inventory tabs with Currencies' fixed places, and the Riftforge's third tab ("Forge") with one
target slot, the target's tooltip beside it and the pick-and-click loop. A second picture shows the Shatter
confirmation (which waits for #29's rules). Replaces the two-slot, operation-list mock-ups of tools/mockup_riftforge.py.

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
TABS = ["Bag", "Currencies", "Splinters", "Maps", "Runes", "Essences"]
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
    w, h = 640, 520
    P.paste(canvas, P.sliced(P.art("panel"), w, h, 20), x, y)
    for i, name in enumerate(["Craft", "Upgrade", "Forge"]):
        P.button(canvas, (x + 20 + i * 150, y + 16, 142, 32), name, enabled=name != "Forge")
    d = ImageDraw.Draw(canvas)
    P.text(d, (x + 20 + 2 * 150 + 71, y + 32), "Forge", 15, ORANGE, "mm")
    top = y + 60
    P.text(d, (x + 20, top + 6), "Riftforge", 20, ORANGE, "la", title=True)
    M.slot(canvas, x + 24, top + 40, 72, M.item_icon(), glow=P.RUNE)
    d = ImageDraw.Draw(canvas)
    if confirm:
        P.text(d, (x + 20, top + 128), "Right-click an orb to pick it.", 13, DIM, "la")
    else:
        P.text(d, (x + 20, top + 128), "Regal Orb (2 left)", 15, LIGHT, "la")
        P.text(d, (x + 20, top + 148), "Left-click the slot to use it.", 13, DIM, "la")
    P.button(canvas, (x + 20, top + 180, 170, 32), "Take out")
    P.button(canvas, (x + 20, top + 220, 170, 32), "Shatter", enabled=confirm)
    d = ImageDraw.Draw(canvas)
    note = "Destroys the item; shows the chances first." if confirm else "Shattering isn't built yet\n(it comes with the Affix Splinters)."
    for i, ln in enumerate(note.split("\n")):
        P.text(d, (x + 20, top + 262 + i * 16), ln, 12, DIM, "la")
    # The target's own tooltip, beside the slot.
    tx = x + 210
    P.paste(canvas, P.sliced(P.art("inset"), w - 230, 300, 10), tx, top + 36)
    d = ImageDraw.Draw(canvas)
    lines = [("Vigorous Bronze Spear of Embers", MAGIC, 17), ("A spear of bronze...", LIGHT, 14), ("", LIGHT, 8),
             ("Pierce: 35", LIGHT, 14), ("Durability: 200/200", LIGHT, 14), ("", LIGHT, 8), ("Magic", MAGIC, 15),
             ("+18% weapon damage (+6)", MAGIC, 15), ("+20% fire damage (+7)", MAGIC, 15)]
    yy = top + 50
    for text, colour, size in lines:
        if text:
            P.text(d, (tx + 14, yy), text, size, colour, "la")
        yy += size + 8
    P.text(d, (x + 20, y + h - 88), "Vigorous Bronze Spear of Embers rerolled: +18% weapon damage;", 15, GREEN, "la")
    P.text(d, (x + 20, y + h - 68), "+20% fire damage", 15, GREEN, "la")
    hint = ("Drag gear from your Bag onto the slot. Right-click an orb to pick it, then left-click the slot",
            "to use it (hold Shift to keep it). Esc or right-click drops the pick.")
    for i, ln in enumerate(hint):
        P.text(d, (x + 20, y + h - 42 + i * 16), ln, 12, DIM, "la")
    if not confirm:
        # The pick follows the cursor.
        ic = P.sized(icons["regal"], 44, 44)
        P.paste(canvas, ic, x + 92, top + 70)
        P.badge(canvas, x + 132, top + 110, 2, LIGHT)
        return
    bx, by, bw, bh = x + 120, y + 150, 400, 260
    d = ImageDraw.Draw(canvas)
    d.rectangle([bx * S, by * S, (bx + bw) * S, (by + bh) * S], fill=(13, 8, 8, 250))
    P.paste(canvas, P.sliced(P.art("inset"), bw, bh, 10), bx, by)
    d = ImageDraw.Draw(canvas)
    P.text(d, (bx + 16, by + 16), "Shatter: Vigorous Bronze Spear of Embers", 16, RED, "la")
    P.text(d, (bx + 16, by + 38), "will be destroyed.", 16, RED, "la")
    P.text(d, (bx + 16, by + 74), "Each line has a 50% chance to become a splinter:", 14, LIGHT, "la")
    P.text(d, (bx + 16, by + 96), "50%  +18% weapon damage", 14, MAGIC, "la")
    P.text(d, (bx + 16, by + 116), "50%  +20% fire damage", 14, MAGIC, "la")
    P.text(d, (bx + 16, by + 148), "(chances from shattering.csv; preview text from #29's rules)", 12, DIM, "la")
    P.button(canvas, (bx + 40, by + bh - 50, 150, 32), "Confirm")
    P.button(canvas, (bx + 210, by + bh - 50, 150, 32), "Cancel")


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
