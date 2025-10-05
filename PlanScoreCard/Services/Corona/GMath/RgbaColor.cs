using System.Runtime.InteropServices;

namespace CoronaDVH.GMath
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct RgbaColor
    {
        [FieldOffset(2)]
        public byte R;

        [FieldOffset(1)]
        public byte G;

        [FieldOffset(0)]
        public byte B;

        [FieldOffset(3)]
        public byte A;

        public byte this[int key]
        {
            get
            {
                switch (key)
                {
                    case 0: return R;
                    case 1: return G;
                    case 2: return B;
                    default: return A;
                }

            }
            set
            {
                switch (key)
                {
                    case 0:
                        R = value;
                        break;
                    case 1:
                        G = value;
                        break;
                    case 2:
                        B = value;
                        break;
                    default:
                        A = value;
                        break;
                }
            }
        }

        public byte[] RGB => new byte[3] { R, G, B };

        public RgbaColor(byte greylevel, byte a = 1)
        {
            R = (G = (B = greylevel));
            A = a;
        }

        public RgbaColor(byte r, byte g, byte b, byte a = 1)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public RgbaColor(float r, float g, float b, float a = 1f)
        {
            R = (byte)MathUtil.Clamp((int)(r * 255f), 0, 255);
            G = (byte)MathUtil.Clamp((int)(g * 255f), 0, 255);
            B = (byte)MathUtil.Clamp((int)(b * 255f), 0, 255);
            A = (byte)MathUtil.Clamp((int)(a * 255f), 0, 255);
        }

        public RgbaColor(byte[] v2)
        {
            R = v2[0];
            G = v2[1];
            B = v2[2];
            A = v2[3];
        }

        public RgbaColor(RgbaColor copy)
        {
            R = copy.R;
            G = copy.G;
            B = copy.B;
            A = copy.A;
        }

        public RgbaColor(RgbaColor copy, byte newAlpha)
        {
            R = copy.R;
            G = copy.G;
            B = copy.B;
            A = newAlpha;
        }
    }
}
