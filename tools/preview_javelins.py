"""Preview of the C6 looks: Gungnir's Throw (gold ghost spear), Lightning Bolt, Plague Javelin. PIL only."""
import math
import random
import sys

from PIL import Image, ImageDraw, ImageFilter, ImageFont

W, H = 1600, 900
BG = (24, 34, 30)
random.seed(7)

try:
    FONT = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", 16)
    SMALL = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", 13)
except OSError:
    FONT = SMALL = ImageFont.load_default()

GOLD = (255, 200, 90)
GOLD_CORE = (255, 238, 180)
BOLT = (190, 225, 255)
GREEN = (120, 210, 90)


def layer():
    return Image.new("RGBA", (W, H), (0, 0, 0, 0))


def spear_points(x, y, length, angle=0.0):
    """Shaft and leaf blade of a spear pointing along angle, tip at (x, y)."""
    ca, sa = math.cos(angle), math.sin(angle)

    def p(u, v):  # u back along the shaft, v sideways
        return (x - u * ca - v * sa, y - u * sa + v * ca)

    blade = [p(0, 0), p(length * 0.10, length * 0.035), p(length * 0.20, 0.0), p(length * 0.10, -length * 0.035)]
    shaft = (p(length * 0.19, 0), p(length, 0))
    return blade, shaft


def draw_spear(img, x, y, length, alpha=1.0, angle=0.0, glow=True, tint=GOLD):
    blade, shaft = spear_points(x, y, length, angle)
    if glow:
        g = layer()
        d = ImageDraw.Draw(g)
        d.line(shaft, fill=tint + (int(210 * alpha),), width=22)
        d.polygon(blade, fill=tint + (int(170 * alpha),))
        g = g.filter(ImageFilter.GaussianBlur(10))
        img.alpha_composite(g)
    s = layer()
    d = ImageDraw.Draw(s)
    d.line(shaft, fill=tint + (int(140 * alpha),), width=5)
    d.polygon(blade, fill=GOLD_CORE + (int(190 * alpha),))
    d.line(shaft, fill=GOLD_CORE + (int(120 * alpha),), width=1)
    img.alpha_composite(s)


def trail(img, x0, x1, y, color, alpha=1.0):
    t = layer()
    d = ImageDraw.Draw(t)
    n = 40
    for i in range(n):
        f = i / n
        xa = x0 + (x1 - x0) * f
        xb = x0 + (x1 - x0) * (f + 1 / n)
        d.line((xa, y, xb, y), fill=color + (int(90 * f * alpha),), width=max(1, int(6 * f)))
    img.alpha_composite(t.filter(ImageFilter.GaussianBlur(3)))


def lightning(img, x0, x1, y, amp, strands=3):
    t = layer()
    d = ImageDraw.Draw(t)
    for s in range(strands):
        pts = []
        x = x0
        while x < x1:
            pts.append((x, y + random.uniform(-amp, amp)))
            x += random.uniform(10, 22)
        pts.append((x1, y))
        d.line(pts, fill=BOLT + (230,), width=2)
    g = t.filter(ImageFilter.GaussianBlur(5))
    img.alpha_composite(g)
    img.alpha_composite(t)


def burst(img, cx, cy, r, color):
    t = layer()
    d = ImageDraw.Draw(t)
    for i in range(14):
        a = random.uniform(0, 2 * math.pi)
        pts = [(cx, cy)]
        rr = 0
        while rr < r:
            rr += random.uniform(8, 16)
            a += random.uniform(-0.5, 0.5)
            pts.append((cx + rr * math.cos(a), cy + rr * math.sin(a)))
        d.line(pts, fill=color + (220,), width=2)
    d.ellipse((cx - 14, cy - 14, cx + 14, cy + 14), fill=(255, 255, 255, 230))
    img.alpha_composite(t.filter(ImageFilter.GaussianBlur(6)))
    img.alpha_composite(t)


def cloud(img, cx, cy, radius, alpha=1.0):
    t = layer()
    d = ImageDraw.Draw(t)
    for _ in range(int(14 + radius / 4)):
        a = random.uniform(0, 2 * math.pi)
        r = random.uniform(0, radius * 0.6)
        pr = random.uniform(radius * 0.25, radius * 0.5)
        x, y = cx + r * math.cos(a), cy + r * math.sin(a) * 0.45
        d.ellipse((x - pr, y - pr * 0.6, x + pr, y + pr * 0.6), fill=GREEN + (int(55 * alpha),))
    img.alpha_composite(t.filter(ImageFilter.GaussianBlur(radius * 0.18 + 2)))


def drips(img, x0, x1, y):
    t = layer()
    d = ImageDraw.Draw(t)
    for i in range(9):
        x = x0 + (x1 - x0) * i / 9 + random.uniform(-6, 6)
        dy = random.uniform(6, 26)
        d.ellipse((x - 3, y + dy - 4, x + 3, y + dy + 4), fill=GREEN + (210,))
    img.alpha_composite(t.filter(ImageFilter.GaussianBlur(1)))


def panel(d, box, title):
    d.rectangle(box, outline=(90, 110, 100), width=1)
    d.text((box[0] + 10, box[1] + 8), title, font=FONT, fill=(235, 235, 225))


def main(out):
    img = Image.new("RGBA", (W, H), BG + (255,))
    d = ImageDraw.Draw(img)
    M = 150  # pixels per metre in the flight panels
    L = 2.0 * M  # the drawn spear: 2 m (Claude's sketch scale)

    # 1. Gungnir's Throw in flight
    b1 = (20, 20, 1010, 290)
    panel(d, b1, "1. Gungnir's Throw: the gold ghost spear in flight (see-through, soft glow, faint trail)")
    trail(img, 140, 640, 165, GOLD)
    draw_spear(img, 960, 165, L)
    d = ImageDraw.Draw(img)
    d.text((40, 52), "flight direction  >>>", font=SMALL, fill=(200, 200, 190))
    d.line((60, 260, 60 + M, 260), fill=(230, 230, 220), width=2)
    d.text((60 + M / 2 - 10, 266), "1 m", font=SMALL, fill=(230, 230, 220))

    # 2. Sticks, then fades
    b2 = (1030, 20, 1580, 290)
    panel(d, b2, "2. It sticks where it lands, then fades (~1 s)")
    for i, (t, a) in enumerate([(0.0, 1.0), (0.4, 0.6), (0.8, 0.25)]):
        cx = 1110 + i * 170
        img.alpha_composite(Image.new("RGBA", (1, 1)), (0, 0))
        d = ImageDraw.Draw(img)
        d.line((cx - 70, 240, cx + 90, 240), fill=(110, 90, 60), width=3)
        draw_spear(img, cx + 10, 232, 1.1 * M, alpha=a, angle=math.radians(60))
        d = ImageDraw.Draw(img)
        d.text((cx - 40, 252), "%.1f s" % t, font=SMALL, fill=(210, 210, 200))

    # 3. Lightning Bolt
    b3 = (20, 310, 1010, 590)
    panel(d, b3, "3. Lightning Bolt: the gold spear wrapped in crackling lightning (twice as fast, stops at the first hit)")
    trail(img, 60, 520, 450, BOLT)
    lightning(img, 360, 700, 450, 14)
    draw_spear(img, 700, 450, L)
    lightning(img, 420, 690, 450, 9, strands=2)
    burst(img, 900, 450, 70, BOLT)
    d = ImageDraw.Draw(img)
    d.text((800, 545), "the hit: lightning burst", font=SMALL, fill=(210, 210, 200))

    # 4. Plague Javelin flight
    b4 = (1030, 310, 1580, 590)
    panel(d, b4, "4. Plague Javelin in flight: green drips fall")
    trail(img, 1060, 1250, 450, GREEN)
    draw_spear(img, 1540, 450, 1.6 * M)
    drips(img, 1310, 1500, 452)

    # 5. Plague cloud spreading
    b5 = (20, 610, 1580, 880)
    d = ImageDraw.Draw(img)
    panel(d, b5, "5. Plague Javelin's cloud spreading from the landing point to 4 m (level 1: over 3 s; level 20: over 6.8 s)  -  seen from the side, radius line in white")
    S = 25
    for i, (f, lab) in enumerate([(0.1, "start"), (0.4, "40%"), (0.7, "70%"), (1.0, "full: 4 m")]):
        cx = 220 + i * 380
        cloud(img, cx, 790, 4 * S * f + 6, alpha=1.0 if f < 1 else 0.85)
        d = ImageDraw.Draw(img)
        d.line((cx - 150, 800, cx + 150, 800), fill=(80, 70, 50), width=2)
        d.line((cx, 812, cx + 4 * S * f, 812), fill=(240, 240, 230), width=1)
        d.text((cx - 25, 830), lab, font=SMALL, fill=(210, 210, 200))
        if i == 0:
            draw_spear(img, cx + 5, 795, 1.0 * M * 0.6, angle=math.radians(60), alpha=0.8)
    img.convert("RGB").save(out)


if __name__ == "__main__":
    main(sys.argv[1])
