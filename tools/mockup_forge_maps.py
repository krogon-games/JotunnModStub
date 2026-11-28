"""
Mock-ups of map crafting at the Riftforge (Krogon's decisions of round claude/festive-gates-79y1yr): a map's tier is its
rarity and affix count (tier 1 Normal, 2-3 Magic with 1-2 affixes, 4-5 Rare with 3-4, tier 6 = a tier 5 map corrupted
with a shard); orbs put affixes on maps as on gear. Three pictures, written into docs/images/:

    forge_map_bench.png     the Forge tab with a tier 3 map in the slot (Currencies tab: what can't be used is red)
    forge_map_corrupt.png   the Corrupt confirmation for a tier 5 map (same layout numbers as forge_dialog_corrupt.png)
                            NOTE: this pop-up was removed on 2026-11-27 (Krogon's decision 14: a shard on a map acts
                            with one click); the picture is kept as the record of decision 13
    forge_map_tooltip.png   the Maps tab with tier numbers in the corners, the tier 6 map's tooltip, the rip's hover text

    python tools/mockup_forge_maps.py      (writes into docs/images/)

SUPERSEDED: Krogon's decision 1 of the affix round (2026-11-27, four affixes removed), decision 14 of the rips round (no
corrupt-map pop-up) and decision 10 of the affix round (no affix names on the map tooltip or Forge) supersede the pictures
this script draws. It is kept as the record.

Item names, lines and counts are samples; a picture of the arrangement, not the exact art. Reuses the helpers of
tools/mockup_riftforge_workspace.py and tools/mockup_forge_dialogs.py; map icons come from Corruption/Assets/.
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import preview_skills_panel as P  # noqa: E402
import mockup_riftforge as M  # noqa: E402
import mockup_riftforge_workspace as W  # noqa: E402
import mockup_forge_dialogs as D  # noqa: E402

S = P.S
OUT = M.OUT


def _badge(canvas, cx, cy, value, colour):
    """The round count badge. preview_skills_panel.py no longer has it (removed with the skill-card rework, 5d65c16), so
    the older mock-up scripts fail to run; this is the same drawing, put back for this process only."""
    d = ImageDraw.Draw(canvas)
    r = 11
    d.ellipse([(cx - r) * S, (cy - r) * S, (cx + r) * S, (cy + r) * S], fill=(16, 12, 10, 240), outline=(168, 120, 62, 255),
              width=int(1.5 * S))
    P.text(d, (cx, cy), str(value), 13, colour, "mm", outline=False)


if not hasattr(P, "badge"):
    P.badge = _badge
ASSETS = os.path.join(M.ROOT, "Corruption", "Assets")
WHITE, MAGIC, RARE, CORRUPT = (255, 255, 255), (136, 136, 255), (255, 255, 119), (224, 64, 64)
GOLD_BULLET = (200, 184, 120)
TIER_COLOUR = {1: WHITE, 2: MAGIC, 3: MAGIC, 4: RARE, 5: RARE, 6: CORRUPT}
TIER_KIND = {1: "Normal", 2: "Magic", 3: "Magic", 4: "Rare", 5: "Rare", 6: "Corrupted"}
ORANGE = W.ORANGE
LIGHT, DIM = W.LIGHT, W.DIM

SWARMING = ("Swarming", "Twice as many creatures in every wave")
HARDENED = ("Hardened", "Rip creatures take 30% less damage")
RABID = ("Rabid", "Rip creatures hit 40% harder and move 30% faster")
SMOULDERING = ("Smouldering", "Every hit from a rip creature adds 25% of its damage as fire")
TIER_LINES = {
    3: ["Waves x3 · 1 in 2 creatures two-star", "The heart: 3–8 shards · gear at least Magic", "Drops x1.75 in the rip"],
    6: ["Waves x4.5 · every creature two-star", "The heart: 6–14 shards · gear at least Rare", "Drops x2.5 in the rip"],
}
DESCRIPTION = ["Map: use it to open a rip.",
               "Opens a rip into unexplored Meadows. Walk through, find the rip's heart, destroy it to get home."]


def map_icon(biome):
    return Image.open(os.path.join(ASSETS, "map_icon_%s.png" % biome)).convert("RGBA")


def corner_number(canvas, x, y, size, tier):
    """The tier number in the top-right corner of a slot at (x, y, size): bold digits in the rarity colour, dark outline."""
    d = ImageDraw.Draw(canvas)
    d.text(((x + size - 7) * S, (y + 5) * S), str(tier), font=P.font(19 if size < 70 else 24), fill=TIER_COLOUR[tier],
           anchor="ra", stroke_width=2 * S, stroke_fill=(0, 0, 0))


def bullets(canvas, x, y, width, affixes, colour, size=15, line=19, gap=10):
    """One gold bullet per affix, the text in the rarity colour, wrapped with a hanging indent. Returns the end y."""
    d = ImageDraw.Draw(canvas)
    for name, text in affixes:
        lines = P.wrap(d, "%s: %s" % (name, text), size, width - 14)
        d.ellipse([(x + 1) * S, (y + line / 2 - 3) * S, (x + 7) * S, (y + line / 2 + 3) * S], fill=GOLD_BULLET)
        for i, ln in enumerate(lines):
            P.text(d, (x + 14, y + i * line), ln, size, colour, "la")
        y += len(lines) * line + gap
    return y


def inventory(canvas, x, y, tab, cells, notes, usable=(), pick=None, usable_set=None):
    """The inventory panel (as in the workspace picture). cells: list of 32 (icon, count, dim, tier) or None."""
    w, h = 600, 380
    P.paste(canvas, P.sliced(P.art("panel"), w, h, 20), x, y)
    bw = (w - 28 - 5 * 4) / 6
    for i, name in enumerate(W.TABS):
        bx = x + 14 + i * (bw + 4)
        P.button(canvas, (bx, y - 34, bw, 30), name, enabled=name != tab)
        if name == tab:
            P.text(ImageDraw.Draw(canvas), (bx + bw / 2, y - 19), name, 15, ORANGE, "mm")
    cell, gap = 62, 8
    gx, gy = x + (w - (8 * cell + 7 * gap)) / 2, y + 40
    for i, c in enumerate(cells):
        cx, cy = gx + (i % 8) * (cell + gap), gy + (i // 8) * (cell + gap)
        if c is None:
            M.slot(canvas, cx, cy, cell)
            continue
        icon, count, dim, tier, glow = c
        M.slot(canvas, cx, cy, cell, icon, glow=glow, count=count, dim=dim)
        if tier:
            corner_number(canvas, cx, cy, cell, tier)
    for i, note in enumerate(notes):
        P.text(ImageDraw.Draw(canvas), (x + 20, y + h - 52 + i * 20), note, 14, DIM, "la")


def shell(canvas, x, y, h=540):
    """The Riftforge panel with the FORGE tab open; returns (bench x, top, bench w, bench h)."""
    w = 640
    P.paste(canvas, P.sliced(P.art("panel"), w, h, 20), x, y)
    d = ImageDraw.Draw(canvas)
    P.text(d, (x + w / 2, y + 34), "RIFTFORGE", 26, ORANGE, "mm", title=True)
    for i, name in enumerate(["CRAFT", "UPGRADE", "FORGE"]):
        P.button(canvas, (x + 40 + i * 118, y + 62, 112, 30), name, enabled=name != "FORGE")
    P.text(ImageDraw.Draw(canvas), (x + 40 + 2 * 118 + 56, y + 77), "FORGE", 15, ORANGE, "mm")
    return x + 16, y + 104, 236, h - 124


def bench_picture():
    icons = M.orb_icons()
    Wd, Ht = 1300, 600
    canvas = Image.new("RGBA", (Wd * S, Ht * S), (24, 20, 17, 255))
    # Left: Currencies. Red = can't be used on a tier 3 map; usable = Regal (3->4), Annulment (3->2), Mirror.
    bad = {"transmutation", "augmentation", "alchemy", "chance", "exalted", "divine", "jeweller", "shard"}
    cells = []
    for i in range(32):
        key = W.HOMES[i] if i < len(W.HOMES) else None
        if key is None:
            cells.append(None)
            continue
        icon = icons["divine"] if key == "mirror" else icons[key]
        count = W.COUNTS[key]
        dim = count == 0
        if key in bad and not dim:
            icon = W.tint_red(icon)
        cells.append((icon, count, dim, None, (255, 240, 200) if key == "regal" else None))
    inventory(canvas, 20, 80, "Currencies", cells,
              ["Red: can't be used on this map. Gold glow: your pick.",
               "Regal: tier 3 → 4. Annulment: removes one affix (tier 3 → 2)."])
    x, y = 640, 40
    bx, top, bw, bh = shell(canvas, x, y)
    cx = bx + bw / 2
    P.paste(canvas, P.sliced(P.art("inset"), bw, bh, 10), bx, top)  # the bench, in the recipe list's inset
    d = ImageDraw.Draw(canvas)
    P.text(d, (cx, top + 20), "Forge", 20, ORANGE, "mm")
    M.slot(canvas, cx - 44, top + 38, 88, map_icon("Meadows"), glow=(255, 240, 200))
    corner_number(canvas, cx - 44, top + 38, 88, 3)
    d = ImageDraw.Draw(canvas)
    P.text(d, (cx, top + 146), "Meadows map", 18, MAGIC, "mm")
    ax, ay = bx + 14, top + 172
    P.text(d, (ax, ay), "Tier 3 · Magic map", 16, MAGIC, "la")
    bullets(canvas, ax, ay + 28, bw - 28, [SWARMING, HARDENED], MAGIC)
    P.button(canvas, (x + 36, top + bh - 52, bw - 40, 38), "Remove")
    # Description sheet.
    dx, dw = x + 262, 640 - 278
    P.paste(canvas, P.sliced(P.art("inset"), dw, bh, 10), dx, top)
    d = ImageDraw.Draw(canvas)
    P.text(d, (dx + 16, top + 30), "Meadows map", 24, MAGIC, "lm", title=True)
    yy = top + 62
    for text in DESCRIPTION:
        for ln in P.wrap(d, text, 14, dw - 32):
            P.text(d, (dx + 16, yy), ln, 14, LIGHT, "la")
            yy += 21
    yy += 10
    P.text(d, (dx + 16, yy), "Tier 3 · Magic map", 14, MAGIC, "la")
    yy += 24
    for ln in TIER_LINES[3]:
        P.text(d, (dx + 16, yy), ln, 14, DIM, "la")
        yy += 21
    ry = top + bh - 126
    P.text(d, (dx + 16, ry), "Orb of Augmentation: now tier 3", 16, (200, 235, 160), "la")
    P.text(d, (dx + 16, ry + 22), "+ Hardened: Rip creatures take 30% less damage", 15, (184, 240, 138), "la")
    P.button(canvas, (dx + dw - 160, top + bh - 52, 146, 38), "SHATTER", enabled=False)
    # The pick on the cursor.
    P.paste(canvas, P.sized(icons["regal"], 40, 40), cx + 30, top + 92)
    P.badge(canvas, cx + 68, top + 130, 2, LIGHT)
    save(canvas, Wd, Ht, "forge_map_bench.png")


def save(canvas, w, h, name):
    path = os.path.join(OUT, name)
    canvas.resize((w, h), Image.LANCZOS).convert("RGB").save(path)
    print(path)


def corrupt_picture():
    """The corrupt-map confirmation, from ForgeDialogLayout.cs's numbers (via mockup_forge_dialogs): the map has no
    destroy warning and no outcomes table, so those blocks are left out."""
    explanation = ["The map becomes tier 6: Corrupted Meadows map.",
                   "It never breaks, but it is locked: no orb can change it afterwards. Only a Mirror of Kalandra still works on it."]
    probe = ImageDraw.Draw(Image.new("RGBA", (10, 10)))
    exp_lines = []
    for e in explanation:
        exp_lines += P.wrap(probe, e, 14, D.CONTENT)
    y = D.TITLE_TOP
    title_y = y
    y += D.TITLE_H + D.AFTER_TITLE
    target_y = y
    y += D.ICON + D.AFTER_TARGET
    cost_y = y
    y += D.COST_H + D.AFTER_COST
    explanation_y = y
    y += len(exp_lines) * 19 + D.AFTER_EXPLANATION
    tick_y = y
    y += max(D.TICK, D.LINE) + D.AFTER_TICK
    footer_y = y
    height = y + D.FOOTER_H
    canvas = Image.new("RGBA", (int(D.WIDTH * S), int(height * S)), (0, 0, 0, 0))
    P.paste(canvas, P.sliced(P.art("panel"), D.WIDTH, height, 20), 0, 0)
    d = ImageDraw.Draw(canvas)
    x = D.PADDING
    P.text(d, (D.WIDTH / 2, title_y + D.TITLE_H / 2), "Corrupt Map", 27, D.ORANGE, "mm", title=True)
    d.rectangle([x * S, target_y * S, (x + D.ICON) * S, (target_y + D.ICON) * S], fill=(0, 0, 0, 90))
    P.paste(canvas, P.sized(map_icon("Meadows"), D.ICON, D.ICON), x, target_y)
    corner_number(canvas, x, target_y, D.ICON, 5)
    d = ImageDraw.Draw(canvas)
    P.text(d, (x + D.ICON + D.ICON_GAP, target_y + D.ICON / 2), "Meadows map", 21, RARE, "lm")
    P.text(d, (x, cost_y + D.COST_H / 2), "Cost: 1 Corruption shard (you have 7)", 13, D.DIM, "lm")
    for i, ln in enumerate(exp_lines):
        P.text(d, (x, explanation_y + i * 19 + 9), ln, 14, D.DIM, "lm")
    ty = tick_y + (max(D.TICK, D.LINE) - D.TICK) / 2
    d.rectangle([x * S, ty * S, (x + D.TICK) * S, (ty + D.TICK) * S], fill=GOLD_BULLET)
    d.rectangle([(x + 2) * S, (ty + 2) * S, (x + D.TICK - 2) * S, (ty + D.TICK - 2) * S], fill=(26, 23, 20))
    P.text(d, (x + D.TICK + D.TICK_GAP, tick_y + max(D.TICK, D.LINE) / 2), "Don't warn me again", 14, D.LIGHT, "lm")
    right = x + D.CONTENT
    d.rectangle([x * S, footer_y * S, right * S, (footer_y + 2) * S], fill=(255, 255, 255, 36))
    left = (D.WIDTH - (2 * D.BUTTON_W + D.BUTTON_GAP)) / 2
    by = footer_y + (D.FOOTER_H - D.BUTTON_H) / 2 - 4
    D.native_button(canvas, (left, by, D.BUTTON_W, D.BUTTON_H), "Cancel")
    D.native_button(canvas, (left + D.BUTTON_W + D.BUTTON_GAP, by, D.BUTTON_W, D.BUTTON_H), "Corrupt")
    small = canvas.resize((D.WIDTH, int(height)), Image.LANCZOS)
    frame = Image.new("RGB", (D.WIDTH + 120, int(height) + 80 + 34), (24, 26, 30))
    ImageDraw.Draw(frame).text((12, 9), "Corrupting a full tier 5 map", font=P.font(7), fill=(170, 170, 170))
    frame.paste(small, (60, 34 + 40), small)
    path = os.path.join(OUT, "forge_map_corrupt.png")
    frame.save(path)
    print(path)


def tooltip_picture():
    Wd, Ht = 1220, 720
    canvas = Image.new("RGBA", (Wd * S, Ht * S), (24, 20, 17, 255))
    items = [("Meadows", 1), ("Meadows", 3), ("BlackForest", 2), ("Swamp", 4), ("Meadows", 5), ("Meadows", 6), ("Plains", 1),
             ("Mountain", 3), ("Meadows", 2), ("Swamp", 5)]
    cells = []
    for i in range(32):
        if i < len(items):
            biome, tier = items[i]
            cells.append((map_icon(biome), None, False, tier, (255, 240, 200) if i == 5 else None))
        else:
            cells.append(None)
    inventory(canvas, 20, 80, "Maps", cells, ["The number in the corner is the map's tier (white 1, blue 2–3, yellow 4–5, red 6)."])
    # Tooltip.
    tx, ty, tw = 660, 40, 520
    d = ImageDraw.Draw(canvas)
    lines_h = 0
    # Layout first (measure), then draw on a panel of the right height.
    probe = ImageDraw.Draw(Image.new("RGBA", (10, 10)))
    desc = P.wrap(probe, DESCRIPTION[1], 15, tw - 40)
    affixes = [SWARMING, HARDENED, RABID, SMOULDERING]
    th = 26 + 34 + 26 + len(desc) * 21 + 14 + 26
    for n, t in affixes:
        th += len(P.wrap(probe, "%s: %s" % (n, t), 15, tw - 40 - 14)) * 19 + 10
    th += 30 + 12 + 3 * 21 + 20
    d.rectangle([tx * S, ty * S, (tx + tw) * S, (ty + th) * S], fill=(14, 11, 9, 242), outline=(96, 84, 62), width=2 * S)
    yy = ty + 24
    P.text(d, (tx + 20, yy), "Corrupted Meadows map", 22, CORRUPT, "la", title=True)
    yy += 34
    P.text(d, (tx + 20, yy), "Map: use it to open a rip", 15, DIM, "la")
    yy += 28
    for ln in desc:
        P.text(d, (tx + 20, yy), ln, 15, LIGHT, "la")
        yy += 21
    yy += 14
    P.text(d, (tx + 20, yy), "Tier 6 · Corrupted map", 16, CORRUPT, "la")
    yy += 26
    yy = bullets(canvas, tx + 20, yy, tw - 40, affixes, RARE)
    d = ImageDraw.Draw(canvas)
    P.text(d, (tx + tw / 2, yy + 6), "Corrupted", 17, CORRUPT, "ma")
    yy += 36
    for ln in TIER_LINES[6]:
        P.text(d, (tx + 20, yy), ln, 14, DIM, "la")
        yy += 21
    # Hover text at the crosshair.
    hy = max(ty + th + 40, 500)
    d.rectangle([20 * S, hy * S, (Wd - 20) * S, (hy + 90) * S], fill=(12, 16, 14, 255), outline=(60, 60, 56), width=S)
    P.text(d, (Wd / 2, hy + 16), "Rip into the Meadows · Tier 5 · Swarming, Hardened, Rabid, Smouldering", 17, WHITE, "ma")
    P.text(d, (Wd / 2, hy + 41), "Closes in 18 min", 17, WHITE, "ma")
    t1, t2, t3 = "[", "E", "] Enter"
    f = P.font(17)
    wtot = (d.textlength("[E]", font=f) + d.textlength(" Enter", font=f)) / S
    x0 = Wd / 2 - wtot / 2
    P.text(d, (x0, hy + 62), "[E]", 17, (255, 230, 90), "la")
    P.text(d, (x0 + d.textlength("[E]", font=f) / S, hy + 62), " Enter", 17, WHITE, "la")
    P.text(d, (24, hy - 18), "The rip portal's hover text, at the crosshair:", 13, DIM, "la")
    canvas = canvas.crop((0, 0, Wd * S, (hy + 110) * S))
    save(canvas, Wd, hy + 110, "forge_map_tooltip.png")


if __name__ == "__main__":
    bench_picture()
    corrupt_picture()
    tooltip_picture()
