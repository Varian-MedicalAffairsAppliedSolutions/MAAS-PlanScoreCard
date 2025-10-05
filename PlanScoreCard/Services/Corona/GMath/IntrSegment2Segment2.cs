using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.GMath
{
    public enum IntersectionResult
    {
        NotComputed,
        Intersects,
        NoIntersection,
        InvalidQuery
    }
    public enum IntersectionType
    {
        Empty, Point, Segment, Line, Polygon, Plane, Unknown
    }

    public class IntrSegment2Segment2
    {
        Segment2d segment1;
        public Segment2d Segment1
        {
            get { return segment1; }
            set { segment1 = value; Result = IntersectionResult.NotComputed; }
        }

        Segment2d segment2;
        public Segment2d Segment2
        {
            get { return segment2; }
            set { segment2 = value; Result = IntersectionResult.NotComputed; }
        }

        double intervalThresh = 0.0000025;
        public double IntervalThreshold
        {
            get { return intervalThresh; }
            set { intervalThresh = Math.Max(value, 0); Result = IntersectionResult.NotComputed; }
        }

        double dotThresh = MathUtil.ZeroTolerance;
        public double DotThreshold
        {
            get { return dotThresh; }
            set { dotThresh = Math.Max(value, 0); Result = IntersectionResult.NotComputed; }
        }

        public int Quantity = 0;
        public IntersectionResult Result = IntersectionResult.NotComputed;
        public IntersectionType Type = IntersectionType.Empty;

        public bool IsSimpleIntersection
        {
            get { return Result == IntersectionResult.Intersects && Type == IntersectionType.Point; }
        }

        // these values are all on segment 1, unlike many other tests!!

        public Vector2d Point0;
        public Vector2d Point1;     // only set if Quantity == 2, ie segment overlap

        public double Parameter0;
        public double Parameter1;     // only set if Quantity == 2, ie segment overlap

        public IntrSegment2Segment2(Segment2d seg1, Segment2d seg2)
        {
            segment1 = seg1; segment2 = seg2;
        }

        public IntrSegment2Segment2 Compute()
        {
            Find();
            return this;
        }


        public bool Find()
        {
            if (Result != IntersectionResult.NotComputed)
                return (Result == IntersectionResult.Intersects);

            // [RMS] if either segment direction is not a normalized vector, 
            //   results are garbage, so fail query
            if (segment1.Direction.IsNormalized == false || segment2.Direction.IsNormalized == false)
            {
                Type = IntersectionType.Empty;
                Result = IntersectionResult.InvalidQuery;
                return false;
            }


            Vector2d s = Vector2d.Zero;
            Type = IntrLine2Line2.Classify(segment1.Center, segment1.Direction,
                                           segment2.Center, segment2.Direction,
                                           dotThresh, ref s);

            if (Type == IntersectionType.Point)
            {
                // Test whether the line-line intersection is on the segments.
                if (Math.Abs(s[0]) <= segment1.Extent + intervalThresh
                    && Math.Abs(s[1]) <= segment2.Extent + intervalThresh)
                {
                    Quantity = 1;
                    Point0 = segment1.Center + s[0] * segment1.Direction;
                    Parameter0 = s[0];
                }
                else
                {
                    Quantity = 0;
                    Type = IntersectionType.Empty;
                }
            }
            else if (Type == IntersectionType.Line)
            {
                // Compute the location of segment1 endpoints relative to segment0.
                Vector2d diff = segment2.Center - segment1.Center;
                double t1 = segment1.Direction.Dot(diff);
                double tmin = t1 - segment2.Extent;
                double tmax = t1 + segment2.Extent;
                Intersector1 calc = new Intersector1(-segment1.Extent, segment1.Extent, tmin, tmax);
                calc.Find();
                Quantity = calc.NumIntersections;
                if (Quantity == 2)
                {
                    Type = IntersectionType.Segment;
                    Parameter0 = calc.GetIntersection(0);
                    Point0 = segment1.Center +
                        Parameter0 * segment1.Direction;
                    Parameter1 = calc.GetIntersection(1);
                    Point1 = segment1.Center +
                        Parameter1 * segment1.Direction;
                }
                else if (Quantity == 1)
                {
                    Type = IntersectionType.Point;
                    Parameter0 = calc.GetIntersection(0);
                    Point0 = segment1.Center +
                        Parameter0 * segment1.Direction;
                }
                else
                {
                    Type = IntersectionType.Empty;
                }
            }
            else
            {
                Quantity = 0;
            }

            Result = (Type != IntersectionType.Empty) ?
                IntersectionResult.Intersects : IntersectionResult.NoIntersection;

            // [RMS] for debugging...
            //sanity_check();

            return (Result == IntersectionResult.Intersects);
        }

    }
}
