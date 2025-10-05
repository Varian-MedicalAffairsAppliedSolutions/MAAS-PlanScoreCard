using System;

namespace CoronaDVH.GMath
{
    public class MathUtil
    {
        public const double Epsilon = 2.2204460492503131e-016;
        public const double ZeroTolerance = 1e-08;
        public const double Deg2Rad = (Math.PI / 180.0);
        public const double Rad2Deg = (180.0 / Math.PI);
        public const float Epsilonf = 1.192092896e-07F;
        public const float ZeroTolerancef = 1e-06f;
        public static float Clamp(float f, float low, float high)
        {
            return (f < low) ? low : (f > high) ? high : f;
        }

        public static double Clamp(double f, double low, double high)
        {
            return (f < low) ? low : (f > high) ? high : f;
        }

        public static int Clamp(int f, int low, int high)
        {
            return (f < low) ? low : (f > high) ? high : f;
        }
    }
}