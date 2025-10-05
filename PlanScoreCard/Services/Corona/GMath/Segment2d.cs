using Dicom.Imaging.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.GMath
{
    public struct Segment2d
    {
        // Center-direction-extent representation.
        public Vector2d Center;
        public Vector2d Direction;
        public double Extent;

        public Segment2d(Vector2d p0, Vector2d p1)
        {
            //update_from_endpoints(p0, p1);
            Center = 0.5 * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5 * Direction.Normalize();
        }
        public Segment2d(Vector2d center, Vector2d direction, double extent)
        {
            Center = center; Direction = direction; Extent = extent;
        }

        public Vector2d P0
        {
            get { return Center - Extent * Direction; }
            set { update_from_endpoints(value, P1); }
        }
        public Vector2d P1
        {
            get { return Center + Extent * Direction; }
            set { update_from_endpoints(P0, value); }
        }
        public double Length
        {
            get { return 2 * Extent; }
        }

        public Vector2d Endpoint(int i)
        {
            return (i == 0) ? (Center - Extent * Direction) : (Center + Extent * Direction);
        }

        // parameter is signed distance from center in direction
        public Vector2d PointAt(double d)
        {
            return Center + d * Direction;
        }

        // t ranges from [0,1] over [P0,P1]
        public Vector2d PointBetween(double t)
        {
            return Center + (2 * t - 1) * Extent * Direction;
        }

        public double DistanceSquared(Vector2d p)
        {
            double t = (p - Center).Dot(Direction);
            if (t >= Extent)
                return P1.DistanceSquared(p);
            else if (t <= -Extent)
                return P0.DistanceSquared(p);
            Vector2d proj = Center + t * Direction;
            return proj.DistanceSquared(p);
        }
        public double DistanceSquared(Vector2d p, out double t)
        {
            t = (p - Center).Dot(Direction);
            if (t >= Extent)
            {
                t = Extent;
                return P1.DistanceSquared(p);
            }
            else if (t <= -Extent)
            {
                t = -Extent;
                return P0.DistanceSquared(p);
            }
            Vector2d proj = Center + t * Direction;
            return proj.DistanceSquared(p);
        }

        public Vector2d NearestPoint(Vector2d p)
        {
            double t = (p - Center).Dot(Direction);
            if (t >= Extent)
                return P1;
            if (t <= -Extent)
                return P0;
            return Center + t * Direction;
        }

        public double Project(Vector2d p)
        {
            return (p - Center).Dot(Direction);
        }

        void update_from_endpoints(Vector2d p0, Vector2d p1)
        {
            Center = 0.5 * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5 * Direction.Normalize();
        }




        /// <summary>
        /// Returns:
        ///   +1, on right of line
        ///   -1, on left of line
        ///    0, on the line
        /// </summary>
        public int WhichSide(Vector2d test, double tol = 0)
        {
            // [TODO] subtract Center from test?
            Vector2d vec0 = Center + Extent * Direction;
            Vector2d vec1 = Center - Extent * Direction;
            double x0 = test.X - vec0.X;
            double y0 = test.Y - vec0.Y;
            double x1 = vec1.X - vec0.X;
            double y1 = vec1.Y - vec0.Y;
            double det = x0 * y1 - x1 * y0;
            return (det > tol ? +1 : (det < -tol ? -1 : 0));
        }



        // IParametricCurve2d interface

        public bool IsClosed { get { return false; } }

        public double ParamLength { get { return 1.0f; } }

        // t in range[0,1] spans arc
        public Vector2d SampleT(double t)
        {
            return Center + (2 * t - 1) * Extent * Direction;
        }

        public Vector2d TangentT(double t)
        {
            return Direction;
        }

        public bool HasArcLength { get { return true; } }
        public double ArcLength { get { return 2 * Extent; } }

        public Vector2d SampleArcLength(double a)
        {
            return P0 + a * Direction;
        }

        public void Reverse()
        {
            update_from_endpoints(P1, P0);
        }

        public bool IsTransformable { get { return true; } }


        /// <summary>
        /// distance from pt to segment (a,b), with no square roots
        /// </summary>
        public static double FastDistanceSquared(ref Vector2d a, ref Vector2d b, ref Vector2d pt)
        {
            double vx = b.X - a.X, vy = b.Y - a.Y;
            double len2 = vx * vx + vy * vy;
            double dx = pt.X - a.X, dy = pt.Y - a.Y;
            if (len2 < 1e-13)
            {
                return dx * dx + dy * dy;
            }
            double t = (dx * vx + dy * vy);
            if (t <= 0)
            {
                return dx * dx + dy * dy;
            }
            else if (t >= len2)
            {
                dx = pt.X - b.X; dy = pt.Y - b.Y;
                return dx * dx + dy * dy;
            }

            dx = pt.X - (a.X + ((t * vx) / len2));
            dy = pt.Y - (a.Y + ((t * vy) / len2));
            return dx * dx + dy * dy;
        }


        /// <summary>
        /// Returns:
        ///   +1, on right of line
        ///   -1, on left of line
        ///    0, on the line
        /// </summary>
        public static int WhichSide(ref Vector2d a, ref Vector2d b, ref Vector2d test, double tol = 0)
        {
            double x0 = test.X - a.X;
            double y0 = test.Y - a.Y;
            double x1 = b.X - a.X;
            double y1 = b.Y - a.Y;
            double det = x0 * y1 - x1 * y0;
            return (det > tol ? +1 : (det < -tol ? -1 : 0));
        }




        /// <summary>
        /// Test if segments intersect. Returns true for parallel-line overlaps.
        /// Returns same result as IntrSegment2Segment2.
        /// </summary>
        public bool Intersects(ref Segment2d seg2, double dotThresh = double.Epsilon, double intervalThresh = 0.00000025)
        {
            // see IntrLine2Line2 and IntrSegment2Segment2 for details on this code

            Vector2d diff = seg2.Center - Center;
            double D0DotPerpD1 = Direction.DotPerp(seg2.Direction);
            if (Math.Abs(D0DotPerpD1) > dotThresh)
            {   // Lines intersect in a single point.
                double invD0DotPerpD1 = ((double)1) / D0DotPerpD1;
                double diffDotPerpD0 = diff.DotPerp(Direction);
                double diffDotPerpD1 = diff.DotPerp(seg2.Direction);
                double s = diffDotPerpD1 * invD0DotPerpD1;
                double s2 = diffDotPerpD0 * invD0DotPerpD1;
                return Math.Abs(s) <= (Extent + intervalThresh)
                        && Math.Abs(s2) <= (seg2.Extent + intervalThresh);
            }

            // Lines are parallel.
            diff.Normalize();
            double diffNDotPerpD1 = diff.DotPerp(seg2.Direction);
            if (Math.Abs(diffNDotPerpD1) <= dotThresh)
            {
                // Compute the location of segment1 endpoints relative to segment0.
                diff = seg2.Center - Center;
                double t1 = Direction.Dot(diff);
                double tmin = t1 - seg2.Extent;
                double tmax = t1 + seg2.Extent;
                Interval1d extents = new Interval1d(-Extent, Extent);
                if (extents.Overlaps(new Interval1d(tmin, tmax)))
                    return true;
                return false;
            }

            // lines are parallel but not collinear
            return false;
        }
        public bool Intersects(Segment2d seg2, double dotThresh = double.Epsilon, double intervalThresh = 0.0000025)
        {
            return Intersects(ref seg2, dotThresh, intervalThresh);
        }

        public int Sign { get { return Math.Sign(P1.Y - P0.Y / P1.X - P0.X); } }

        public static bool operator ==(Segment2d obj1, Segment2d obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;
            if (ReferenceEquals(obj1, null))
                return false;
            if (ReferenceEquals(obj2, null))
                return false;
            return obj1.Equals(obj2);
        }
        public static bool operator !=(Segment2d obj1, Segment2d obj2) => !(obj1 == obj2);

        public bool Equals(Segment2d other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return P0.Equals(other.P0)
                   && P1.Equals(other.P1);
        }
    }
}
