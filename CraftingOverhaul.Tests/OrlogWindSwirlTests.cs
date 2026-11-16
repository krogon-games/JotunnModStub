using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogWindSwirlTests
    {
        private const float Tol = 1e-4f;

        private static SwirlPoint[] Fill(SwirlShape shape, int ribbon, float time)
        {
            var points = new SwirlPoint[shape.Points];
            WindSwirlRules.Ribbon(shape, ribbon, time, points);
            return points;
        }

        [Fact]
        public void PresetsHaveTheAgreedNumbers()
        {
            var l = WindSwirlRules.Lance;
            Assert.Equal(3, l.Ribbons);
            Assert.Equal(40, l.Points);
            Assert.Equal(4.0f, l.Length);
            Assert.Equal(0.6f, l.HeadZ);
            Assert.Equal(0.08f, l.RadiusHead);
            Assert.Equal(0.32f, l.RadiusTail);
            Assert.Equal(3.0f, l.Turns);
            Assert.Equal(5.0f, l.SpinPerSecond);
            Assert.Equal(0.09f, l.WidthHead);
            Assert.Equal(0.02f, l.WidthTail);
            Assert.Equal(0.06f, l.FadeIn);

            var w = WindSwirlRules.Wake;
            Assert.Equal(2, w.Ribbons);
            Assert.Equal(0.22f, w.RadiusHead);
            Assert.Equal(0.22f, w.RadiusTail);
            Assert.Equal(0.07f, w.WidthHead);
            Assert.Equal(0.07f, w.WidthTail);
            Assert.Equal(1.5f, w.SpinPerSecond);
            Assert.Equal(0f, w.FadeIn);
            Assert.Equal(0.35f, WindSwirlRules.WakeTurnsPerMetre);
            Assert.Equal(5f, WindSwirlRules.WakePointsPerMetre);
            Assert.Equal(0.7f, WindSwirlRules.WakeSeconds);
            Assert.Equal(1.8f, WindSwirlRules.WakeGrow);

            var c = WindSwirlRules.Charge;
            Assert.Equal(2, c.Ribbons);
            Assert.Equal(24, c.Points);
            Assert.Equal(0.9f, c.Length);
            Assert.Equal(0.45f, c.HeadZ);
            Assert.Equal(0.10f, c.RadiusHead);
            Assert.Equal(0.16f, c.RadiusTail);
            Assert.Equal(1.5f, c.Turns);
            Assert.Equal(3.0f, c.SpinPerSecond);
            Assert.Equal(0.04f, c.WidthHead);
            Assert.Equal(0.015f, c.WidthTail);
            Assert.Equal(0.15f, c.FadeIn);

            Assert.Equal(0.35f, WindSwirlRules.BurstLife);
            Assert.Equal(0.15f, WindSwirlRules.BurstStart);
            Assert.Equal(0.9f, WindSwirlRules.BurstEnd);
            Assert.Equal(0.3f, WindSwirlRules.PuffLife);
            Assert.Equal(0.25f, WindSwirlRules.PuffStart);
            Assert.Equal(1.0f, WindSwirlRules.PuffEnd);
            Assert.Equal(28, WindSwirlRules.RingPoints);
            Assert.Equal(3f, WindSwirlRules.ArrowScale);
            Assert.Equal(2.6f, WindSwirlRules.CoreLength);
            Assert.Equal(0.16f, WindSwirlRules.CoreWidth);
        }

        [Fact]
        public void HeadStartsAtAngleZeroAndTailFadesOut()
        {
            var l = WindSwirlRules.Lance;
            var p = Fill(l, 0, 0f);
            Assert.Equal(l.RadiusHead, p[0].X, 4);
            Assert.Equal(0f, p[0].Y, 4);
            Assert.Equal(l.HeadZ, p[0].Z, 4);
            Assert.Equal(0f, p[0].Alpha, 4);   // faded in from nothing at the head
            var tail = p[l.Points - 1];
            Assert.Equal(l.HeadZ - l.Length, tail.Z, 4);
            Assert.Equal(0f, tail.Alpha, 4);
            Assert.Equal(l.WidthTail, tail.Width, 4);
            Assert.Equal(l.WidthHead, p[0].Width, 4);
        }

        [Fact]
        public void RadiusGrowsFromHeadToTailAndStaysInTheTube()
        {
            var l = WindSwirlRules.Lance;
            foreach (float time in new[] { 0f, 0.13f, 1.7f })
            {
                for (int r = 0; r < l.Ribbons; r++)
                {
                    var p = Fill(l, r, time);
                    float last = -1f;
                    for (int i = 0; i < p.Length; i++)
                    {
                        float u = i / (float)(l.Points - 1);
                        float expected = l.RadiusHead + (l.RadiusTail - l.RadiusHead) * u;
                        float dist = (float)Math.Sqrt(p[i].X * p[i].X + p[i].Y * p[i].Y);
                        Assert.Equal(expected, dist, 3);
                        Assert.True(dist >= last - Tol);
                        Assert.True(dist <= 0.32f + Tol);
                        last = dist;
                    }
                }
            }
        }

        [Fact]
        public void RibbonsAreEvenlySpaced()
        {
            var l = WindSwirlRules.Lance;
            var a = Fill(l, 0, 0.4f);
            var b = Fill(l, 1, 0.4f);
            foreach (int i in new[] { 0, 10, 25, 39 })
            {
                double angleA = Math.Atan2(a[i].Y, a[i].X), angleB = Math.Atan2(b[i].Y, b[i].X);
                double diff = (angleB - angleA) * 180.0 / Math.PI;
                diff = ((diff % 360.0) + 360.0) % 360.0;
                Assert.Equal(120.0, diff, 2);
            }
        }

        [Fact]
        public void ShapeRepeatsAfterOneSpin()
        {
            var l = WindSwirlRules.Lance;
            var a = Fill(l, 2, 0.3f);
            var b = Fill(l, 2, 0.3f + 1f / l.SpinPerSecond);
            for (int i = 0; i < a.Length; i++)
            {
                Assert.Equal(a[i].X, b[i].X, 3);
                Assert.Equal(a[i].Y, b[i].Y, 3);
                Assert.Equal(a[i].Z, b[i].Z, 3);
            }
        }

        [Fact]
        public void SpinMovesThePointsInBetween()
        {
            var l = WindSwirlRules.Lance;
            var a = Fill(l, 0, 0f);
            var b = Fill(l, 0, 0.05f);
            Assert.NotEqual(a[20].X, b[20].X);
        }

        [Fact]
        public void BadInputDoesNothing()
        {
            var l = WindSwirlRules.Lance;
            WindSwirlRules.Ribbon(l, 0, 0f, null);
            WindSwirlRules.Ribbon(null, 0, 0f, new SwirlPoint[40]);
            var shortArray = new SwirlPoint[5];
            WindSwirlRules.Ribbon(l, 0, 0f, shortArray);
            Assert.All(shortArray, p => Assert.Equal(0f, p.Alpha));
            var tooFew = new SwirlShape { Ribbons = 1, Points = 1 };
            var one = new SwirlPoint[3];
            WindSwirlRules.Ribbon(tooFew, 0, 0f, one);
            Assert.All(one, p => Assert.Equal(0f, p.Width));
        }

        [Fact]
        public void RingsGrowAndFade()
        {
            const float life = 0.35f;
            Assert.Equal(0.15f, WindSwirlRules.RingRadius(0f, life, 0.15f, 0.9f), 4);
            Assert.Equal(0.15f + 0.75f * 0.75f, WindSwirlRules.RingRadius(life / 2f, life, 0.15f, 0.9f), 4);
            Assert.Equal(0.9f, WindSwirlRules.RingRadius(life, life, 0.15f, 0.9f), 4);
            Assert.Equal(0.9f, WindSwirlRules.RingRadius(life * 3f, life, 0.15f, 0.9f), 4);
            Assert.Equal(0.15f, WindSwirlRules.RingRadius(-1f, life, 0.15f, 0.9f), 4);
            Assert.Equal(1f, WindSwirlRules.RingAlpha(0f, life), 4);
            Assert.Equal(0.25f, WindSwirlRules.RingAlpha(life / 2f, life), 4);
            Assert.Equal(0f, WindSwirlRules.RingAlpha(life, life), 4);
            Assert.Equal(0f, WindSwirlRules.RingAlpha(life * 2f, life), 4);
        }

        /// <summary>
        ///     Writes the swirl numbers for <c>tools/preview_wind_lance.py</c> when ORLOG_SWIRL_JSON names a file; does
        ///     nothing otherwise.
        /// </summary>
        [Fact]
        public void WritesTheSwirlForThePreviewWhenAsked()
        {
            string path = Environment.GetEnvironmentVariable("ORLOG_SWIRL_JSON");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            var inv = CultureInfo.InvariantCulture;
            string F(float v) => v.ToString("0.####", inv);
            string Points(SwirlPoint[] p) => "[" + string.Join(",", p.Select(q =>
                "[" + F(q.X) + "," + F(q.Y) + "," + F(q.Z) + "," + F(q.Width) + "," + F(q.Alpha) + "]")) + "]";
            string Shape(SwirlShape s, float time)
            {
                var strands = Enumerable.Range(0, s.Ribbons).Select(r => Points(Fill(s, r, time)));
                return "{\"time\":" + F(time) + ",\"ribbons\":[" + string.Join(",", strands) + "]}";
            }

            var sb = new StringBuilder("{\n");
            sb.Append("\"arrowScale\":").Append(F(WindSwirlRules.ArrowScale))
                .Append(",\"coreLength\":").Append(F(WindSwirlRules.CoreLength))
                .Append(",\"coreWidth\":").Append(F(WindSwirlRules.CoreWidth)).Append(",\n");
            sb.Append("\"lance\":[").Append(Shape(WindSwirlRules.Lance, 0f)).Append(",").Append(Shape(WindSwirlRules.Lance, 0.04f)).Append("],\n");
            sb.Append("\"charge\":[").Append(Shape(WindSwirlRules.Charge, 0f)).Append("],\n");

            // A wake along a straight 20 m path (along +Z), as WindLanceLook builds it: a helix around the path.
            var wake = WindSwirlRules.Wake;
            int count = (int)(20f * WindSwirlRules.WakePointsPerMetre) + 1;
            var wakes = new StringBuilder("[");
            for (int r = 0; r < wake.Ribbons; r++)
            {
                var pts = new SwirlPoint[count];
                for (int i = 0; i < count; i++)
                {
                    float s = i / WindSwirlRules.WakePointsPerMetre;
                    double angle = 2.0 * Math.PI * (r / (double)wake.Ribbons + WindSwirlRules.WakeTurnsPerMetre * s - wake.SpinPerSecond * 0.0);
                    pts[i] = new SwirlPoint
                    {
                        X = wake.RadiusHead * (float)Math.Cos(angle),
                        Y = wake.RadiusHead * (float)Math.Sin(angle),
                        Z = s,
                        Width = wake.WidthHead,
                        Alpha = 1f,
                    };
                }
                wakes.Append(r > 0 ? "," : "").Append(Points(pts));
            }
            wakes.Append("]");
            sb.Append("\"wake\":{\"length\":20,\"ribbons\":").Append(wakes).Append("},\n");

            var ages = new[] { 0f, 0.1f, 0.2f, 0.3f };
            sb.Append("\"burst\":[").Append(string.Join(",", ages.Select(a =>
                "{\"age\":" + F(a) + ",\"radius\":" + F(WindSwirlRules.RingRadius(a, WindSwirlRules.BurstLife, WindSwirlRules.BurstStart, WindSwirlRules.BurstEnd)) +
                ",\"alpha\":" + F(WindSwirlRules.RingAlpha(a, WindSwirlRules.BurstLife)) + "}"))).Append("],\n");
            sb.Append("\"ringPoints\":").Append(WindSwirlRules.RingPoints).Append("\n}\n");
            File.WriteAllText(path, sb.ToString());
        }
    }
}
