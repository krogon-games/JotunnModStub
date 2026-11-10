"""
Preview pictures of the Shatter confirmation (issue #29; Krogon's brief of 2026-11-10), drawn from the same layout
numbers the game uses (Corruption/Rules/ShatterDialogLayout.cs, repeated below: keep them equal).

    python tools/mockup_shatter_dialog.py      (writes docs/images/shatter_dialog_*.png)

These are PREVIEWS, not screenshots: the game uses its own wood panel, fonts and buttons, and wraps text with its own
font, so line breaks can differ a little. What carries over exactly is the arrangement: the order, the two columns,
the hanging indents, the height from the content and the footer. Item names and lines are samples.
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import preview_skills_panel as P  # noqa: E402
import mockup_riftforge as M  # noqa: E402

S = P.S
# --- ShatterDialogLayout.cs
WIDTH, PADDING, TITLE_TOP, TITLE_H, AFTER_TITLE = 600, 34, 26, 34, 20
ICON, ICON_GAP, AFTER_TARGET, COST_H, AFTER_COST = 56, 14, 8, 20, 16
AFTER_WARNING, HEADER_H, AFTER_HEADER, ROW_GAP, AFTER_ROWS = 20, 22, 12, 12, 18
AFTER_EXPLANATION, FOOTER_H, BUTTON_W, BUTTON_H, BUTTON_GAP = 22, 78, 160, 42, 48
CHANCE_W, COLUMN_GAP, MAX_SHARE = 72, 18, 0.9
CONTENT = WIDTH - 2 * PADDING
DESC_W = CONTENT - CHANCE_W - COLUMN_GAP
NAME_W = CONTENT - ICON - ICON_GAP
# ---
BODY, LINE = 16, 22
INDENT = 18                       # the Forge's hanging indent (1.1em)
LIGHT, DIM, ORANGE = (237, 227, 207), (184, 173, 158), (255, 161, 60)
WARN = (255, 128, 96)
MAGIC, RARE, GREEN, BULLET = M.MAGIC, M.RARE, (91, 209, 106), (200, 184, 120)

WARNING = "This permanently destroys the item."
EXPLANATION = ["Each affix rolls independently; you may recover none.", "Recovered splinters keep their exact values."]


def measure(d, text, width, size=BODY, line=LINE):
    lines = P.wrap(d, text, size, width)
    return lines, len(lines) * line


def dialog(name, colour, rows, chance="50%", screen=None, label=None):
    """rows: (text, resonant). Returns the picture; `screen` = a screen height that forces the dialog to shrink."""
    probe = ImageDraw.Draw(Image.new("RGBA", (10, 10)))
    name_lines, name_h = measure(probe, name, NAME_W, 21, 27)
    wrapped = [measure(probe, t, DESC_W - INDENT) for t, _ in rows]

    y = TITLE_TOP
    title_y = y
    y += TITLE_H + AFTER_TITLE
    target_h = max(ICON, name_h)
    target_y = y
    y += target_h + AFTER_TARGET
    cost_y = y
    y += COST_H + AFTER_COST
    warning_y = y
    y += LINE + AFTER_WARNING
    header_y = y
    y += HEADER_H + AFTER_HEADER
    row_ys = []
    for i, (_, h) in enumerate(wrapped):
        row_ys.append(y)
        y += max(LINE, h) + (ROW_GAP if i < len(wrapped) - 1 else 0)
    y += AFTER_ROWS
    explanation_y = y
    y += len(EXPLANATION) * 19 + AFTER_EXPLANATION
    footer_y = y
    height = y + FOOTER_H

    canvas = Image.new("RGBA", (int(WIDTH * S), int(height * S)), (0, 0, 0, 0))
    P.paste(canvas, P.sliced(P.art("panel"), WIDTH, height, 20), 0, 0)
    d = ImageDraw.Draw(canvas)
    x = PADDING
    P.text(d, (WIDTH / 2, title_y + TITLE_H / 2), "SHATTER ITEM", 24, ORANGE, "mm", title=True)

    iy = target_y + (target_h - ICON) / 2
    d.rectangle([x * S, iy * S, (x + ICON) * S, (iy + ICON) * S], fill=(0, 0, 0, 90))
    P.paste(canvas, P.sized(M.item_icon(), ICON, ICON), x, iy)
    d = ImageDraw.Draw(canvas)
    ny = target_y + (target_h - name_h) / 2
    for i, ln in enumerate(name_lines):
        P.text(d, (x + ICON + ICON_GAP, ny + i * 27 + 13), ln, 21, colour, "lm")
    P.text(d, (x, cost_y + COST_H / 2), "Cost: Free", 13, DIM, "lm")
    P.text(d, (x, warning_y + LINE / 2), WARNING, BODY, WARN, "lm")

    chance_right = x + CONTENT
    P.text(d, (x, header_y + HEADER_H - 3), "Potential splinters", 14, ORANGE, "ls")
    P.text(d, (chance_right, header_y + HEADER_H - 3), "Chance", 14, ORANGE, "rs")
    d.rectangle([x * S, (header_y + HEADER_H + 3) * S, chance_right * S, (header_y + HEADER_H + 5) * S], fill=(255, 255, 255, 36))

    for (text, resonant), (lines, _), ry in zip(rows, wrapped, row_ys):
        tone = GREEN if resonant else (RARE if colour == RARE else MAGIC)
        if resonant:
            M.diamond(canvas, x + 6, ry + LINE / 2, 5, GREEN)
            d = ImageDraw.Draw(canvas)
        else:
            d.ellipse([(x + 2) * S, (ry + LINE / 2 - 4) * S, (x + 10) * S, (ry + LINE / 2 + 4) * S], fill=BULLET)
        for i, ln in enumerate(lines):
            P.text(d, (x + INDENT, ry + i * LINE + LINE / 2), ln, BODY, tone, "lm")
        P.text(d, (chance_right, ry + LINE / 2), chance, BODY, LIGHT, "rm")

    for i, ln in enumerate(EXPLANATION):
        P.text(d, (x, explanation_y + i * 19 + 9), ln, 14, DIM, "lm")

    d.rectangle([x * S, footer_y * S, chance_right * S, (footer_y + 2) * S], fill=(255, 255, 255, 36))
    left = (WIDTH - (2 * BUTTON_W + BUTTON_GAP)) / 2
    by = footer_y + (FOOTER_H - BUTTON_H) / 2 - 4
    P.button(canvas, (left, by, BUTTON_W, BUTTON_H), "CANCEL")
    P.button(canvas, (left + BUTTON_W + BUTTON_GAP, by, BUTTON_W, BUTTON_H), "SHATTER")

    scale = 1.0
    if screen and height > screen * MAX_SHARE:
        scale = screen * MAX_SHARE / height
    out_w, out_h = int(WIDTH * scale), int(height * scale)
    small = canvas.resize((out_w, out_h), Image.LANCZOS)
    frame_h = int(screen) if screen else out_h + 80
    frame = Image.new("RGB", (WIDTH + 120, frame_h + (34 if label else 0)), (24, 26, 30))
    fd = ImageDraw.Draw(frame)
    top = 34 if label else 0
    if label:
        fd.text((12, 9), label, font=P.font(7), fill=(170, 170, 170))
    if screen:
        fd.rectangle([0, top, frame.width - 1, top + frame_h - 1], outline=(70, 74, 82))
    frame.paste(small, ((frame.width - out_w) // 2, top + (frame_h - out_h) // 2), small)
    return frame


SHORT = [("+43% backstab damage [20–100]", False), ("+54% parry bonus [20–100]", False)]
LONG = [("Parrying restores 13 stamina to you and nearby allies [5–15]", False),
        ("+43% backstab damage [20–100]", False),
        ("+54% parry bonus [20–100]", False)]
MOST = [("+44 max health [10–50]", True),
        ("Parrying gives you and nearby allies +8% attack speed for 10 seconds [3–15]", False),
        ("Killing an enemy grants +4% attack speed for 10 seconds, stacking up to 5 times [2–6]", False),
        ("A combo finisher releases a shockwave dealing 38% of the hit's damage to nearby enemies [20–60]", False)]


def main():
    out = M.OUT
    shots = [
        ("shatter_dialog_short.png", dialog("Sharp Bronze Sword of the Fox", MAGIC, SHORT, label="Short affixes (Magic item, 2 lines)")),
        ("shatter_dialog_long.png", dialog("Iron Wail", RARE, LONG, label="A long affix that wraps (Rare item, 3 lines)")),
        ("shatter_dialog_most.png", dialog("Storm Aegis", RARE, MOST, label="The most lines (4), three of them long, one resonant")),
        ("shatter_dialog_small_screen.png", dialog("Storm Aegis", RARE, MOST, screen=430,
                                                   label="The same on a very large UI scale: the whole dialog shrinks to fit")),
    ]
    for name, picture in shots:
        picture.save(os.path.join(out, name))
    width = max(p.width for _, p in shots)
    sheet = Image.new("RGB", (width, sum(p.height for _, p in shots) + 12 * (len(shots) - 1)), (24, 26, 30))
    y = 0
    for _, picture in shots:
        sheet.paste(picture, ((width - picture.width) // 2, y))
        y += picture.height + 12
    sheet.save(os.path.join(out, "shatter_dialog_preview.png"))
    print("wrote", ", ".join(n for n, _ in shots), "and shatter_dialog_preview.png")


if __name__ == "__main__":
    main()
