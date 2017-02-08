using System;
using System.Globalization;

namespace Barbar.Delaunay.Drawing
{
    public struct PortableColor
    {
        private readonly byte m_A;
        private readonly byte m_R;
        private readonly byte m_G;
        private readonly byte m_B;

        public static PortableColor Empty;

        public PortableColor(byte a, byte r, byte g, byte b)
        {
            m_A = a;
            m_R = r;
            m_G = g;
            m_B = b;
        }

        public byte A
        {
            get { return m_A; }
        }

        public byte B
        {
            get { return m_B; }
        }

        public byte G
        {
            get { return m_G; }
        }
        
        public byte R
        {
            get { return m_R; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", m_A, m_R, m_G, m_B);
        }

        public override int GetHashCode()
        {
            return (m_A << 24) | (m_R << 16) | (m_G << 8) | m_B;
        }

        public override bool Equals(object obj)
        {
            if (obj is PortableColor)
            {
                return this == (PortableColor)obj;
            }
            return false;
        }

        public static bool operator ==(PortableColor color1, PortableColor color2)
        {
            return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B && color1.A == color2.A;
        }

        public static bool operator !=(PortableColor color1, PortableColor color2)
        {
            return !(color1 == color2);
        }

        internal uint ToUInt32()
        {
            return (uint)(m_A << 24 | m_R << 16 | m_G << 8 | m_B);
        }

        internal static PortableColor FromUInt32(uint argb)
        {
            return FromArgb(
                (byte)((argb & 4278190080u) >> 24),
                (byte)((argb & 16711680u) >> 16),
                (byte)((argb & 65280u) >> 8),
                (byte)(argb & 255u));
        }

        public static PortableColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new PortableColor(a, r, g, b);
        }

        public static PortableColor Parse(string colorValue)
        {
            if (string.IsNullOrEmpty(colorValue))
            {
                throw new ArgumentNullException("colorValue");
            }
            if (colorValue.StartsWith("#", StringComparison.Ordinal))
            {
                colorValue = colorValue.Substring(1);
            }
            uint baseValue;
            try
            {
                baseValue = Convert.ToUInt32(colorValue, 16);
            }
            catch (FormatException e)
            {
                throw new Exception(string.Format("Unable to parse color '{0}'.", colorValue), e);
            }
            return FromUInt32(baseValue);
        }

        public static readonly PortableColor Yellow = new PortableColor(255, 255, 255, 0);
        public static readonly PortableColor Black = new PortableColor(255, 0, 0, 0);
        public static readonly PortableColor White = new PortableColor(255, 255, 255, 255);
    }
}
