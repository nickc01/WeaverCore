using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class SpriteUtilities
    {
        public static float GetRed(this SpriteRenderer sr)   => sr != null ? sr.color.r : 0f;
        public static float GetGreen(this SpriteRenderer sr) => sr != null ? sr.color.g : 0f;
        public static float GetBlue(this SpriteRenderer sr)  => sr != null ? sr.color.b : 0f;
        public static float GetAlpha(this SpriteRenderer sr) => sr != null ? sr.color.a : 0f;

        public static SpriteRenderer SetRed(this SpriteRenderer sr, float r)
        {
            if (sr == null) return null;
            var c = sr.color; c.r = Mathf.Clamp01(r); sr.color = c; return sr;
        }

        public static SpriteRenderer SetGreen(this SpriteRenderer sr, float g)
        {
            if (sr == null) return null;
            var c = sr.color; c.g = Mathf.Clamp01(g); sr.color = c; return sr;
        }

        public static SpriteRenderer SetBlue(this SpriteRenderer sr, float b)
        {
            if (sr == null) return null;
            var c = sr.color; c.b = Mathf.Clamp01(b); sr.color = c; return sr;
        }

        public static SpriteRenderer SetAlpha(this SpriteRenderer sr, float a)
        {
            if (sr == null) return null;
            var c = sr.color; c.a = Mathf.Clamp01(a); sr.color = c; return sr;
        }


        public static SpriteRenderer SetRed(this SpriteRenderer sr, byte r)   => sr.SetRed(ToFloat(r));
        public static SpriteRenderer SetGreen(this SpriteRenderer sr, byte g) => sr.SetGreen(ToFloat(g));
        public static SpriteRenderer SetBlue(this SpriteRenderer sr, byte b)  => sr.SetBlue(ToFloat(b));
        public static SpriteRenderer SetAlpha(this SpriteRenderer sr, byte a) => sr.SetAlpha(ToFloat(a));

        public static SpriteRenderer SetRGB(this SpriteRenderer sr, float r, float g, float b)
        {
            if (sr == null) return null;
            var c = sr.color;
            c.r = Mathf.Clamp01(r);
            c.g = Mathf.Clamp01(g);
            c.b = Mathf.Clamp01(b);
            sr.color = c;
            return sr;
        }

        public static SpriteRenderer SetRGBA(this SpriteRenderer sr, float r, float g, float b, float a)
        {
            if (sr == null) return null;
            var c = sr.color;
            c.r = Mathf.Clamp01(r);
            c.g = Mathf.Clamp01(g);
            c.b = Mathf.Clamp01(b);
            c.a = Mathf.Clamp01(a);
            sr.color = c;
            return sr;
        }

        public static SpriteRenderer SetRGB(this SpriteRenderer sr, byte r, byte g, byte b)
            => sr.SetRGB(ToFloat(r), ToFloat(g), ToFloat(b));

        public static SpriteRenderer SetRGBA(this SpriteRenderer sr, byte r, byte g, byte b, byte a)
            => sr.SetRGBA(ToFloat(r), ToFloat(g), ToFloat(b), ToFloat(a));

        private static float ToFloat(byte v) => v / 255f;
        private static byte ToByte(float v) => (byte)Mathf.Clamp(Mathf.RoundToInt(v * 255f), 0, 255);
    }
}
