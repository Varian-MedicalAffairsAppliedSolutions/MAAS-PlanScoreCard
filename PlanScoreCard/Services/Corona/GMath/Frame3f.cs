using Dicom.Imaging.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.GMath
{
    public struct Frame3f
    {
        Quaternionf rotation;
        Vector3f origin;

        static readonly public Frame3f Identity = new Frame3f(Vector3f.Zero, Quaternionf.Identity);

        public Frame3f(Frame3f copy)
        {
            this.rotation = copy.rotation;
            this.origin = copy.origin;
        }

        public Frame3f(Vector3f origin)
        {
            rotation = Quaternionf.Identity;
            this.origin = origin;
        }

        public Frame3f(Vector3f origin, Vector3f setZ)
        {
            rotation = Quaternionf.FromTo(Vector3f.AxisZ, setZ);
            this.origin = origin;
        }

        public Frame3f(Vector3f origin, Vector3f setAxis, int nAxis)
        {
            if (nAxis == 0)
                rotation = Quaternionf.FromTo(Vector3f.AxisX, setAxis);
            else if (nAxis == 1)
                rotation = Quaternionf.FromTo(Vector3f.AxisY, setAxis);
            else
                rotation = Quaternionf.FromTo(Vector3f.AxisZ, setAxis);
            this.origin = origin;
        }

        public Frame3f(Vector3f origin, Quaternionf orientation)
        {
            rotation = orientation;
            this.origin = origin;
        }

        public Frame3f(Vector3f origin, Vector3f x, Vector3f y, Vector3f z)
        {
            this.origin = origin;
            Matrix3f m = new Matrix3f(x, y, z, false);
            this.rotation = m.ToQuaternion();
        }


        public Quaternionf Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector3f Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        public Vector3f X
        {
            get { return rotation.AxisX; }
        }
        public Vector3f Y
        {
            get { return rotation.AxisY; }
        }
        public Vector3f Z
        {
            get { return rotation.AxisZ; }
        }

        public Vector3f GetAxis(int nAxis)
        {
            if (nAxis == 0)
                return rotation * Vector3f.AxisX;
            else if (nAxis == 1)
                return rotation * Vector3f.AxisY;
            else if (nAxis == 2)
                return rotation * Vector3f.AxisZ;
            else
                throw new ArgumentOutOfRangeException("nAxis");
        }


        public void Translate(Vector3f v)
        {
            origin += v;
        }
        public Frame3f Translated(Vector3f v)
        {
            return new Frame3f(this.origin + v, this.rotation);
        }
        public Frame3f Translated(float fDistance, int nAxis)
        {
            return new Frame3f(this.origin + fDistance * this.GetAxis(nAxis), this.rotation);
        }

        public void Scale(float f)
        {
            origin *= f;
        }
        public void Scale(Vector3f scale)
        {
            origin *= scale;
        }
        public Frame3f Scaled(float f)
        {
            return new Frame3f(f * this.origin, this.rotation);
        }
        public Frame3f Scaled(Vector3f scale)
        {
            return new Frame3f(scale * this.origin, this.rotation);
        }

        /// <summary>
        /// 3D projection of point p onto frame-axis plane orthogonal to normal axis
        /// </summary>
        public Vector3f ProjectToPlane(Vector3f p, int nNormal)
        {
            Vector3f d = p - origin;
            Vector3f n = GetAxis(nNormal);
            return origin + (d - d.Dot(n) * n);
        }

        ///<summary> distance from p to frame-axes-plane perpendicular to normal axis </summary>
        public float DistanceToPlane(Vector3f p, int nNormal)
        {
            return Math.Abs((p - origin).Dot(GetAxis(nNormal)));
        }
        ///<summary> signed distance from p to frame-axes-plane perpendicular to normal axis </summary>
		public float DistanceToPlaneSigned(Vector3f p, int nNormal)
        {
            return (p - origin).Dot(GetAxis(nNormal));
        }


        ///<summary> Map point *into* local coordinates of Frame </summary>
		public Vector3f ToFrameP(Vector3f v)
        {
            v.X -= origin.X; v.Y -= origin.Y; v.Z -= origin.Z;
            return rotation.InverseMultiply(ref v);
        }

        ///<summary> Map point *into* local coordinates of Frame </summary>
        public Vector3f ToFrameP(ref Vector3f v)
        {
            Vector3f x = new Vector3f(v.X - origin.X, v.Y - origin.Y, v.Z - origin.Z);
            return rotation.InverseMultiply(ref x);
        }

        /// <summary> Map point *from* local frame coordinates into "world" coordinates </summary>
        public Vector3f FromFrameP(Vector3f v)
        {
            return this.rotation * v + this.origin;
        }
        /// <summary> Map point *from* local frame coordinates into "world" coordinates </summary>
        public Vector3f FromFrameP(ref Vector3f v)
        {
            return this.rotation * v + this.origin;
        }

        ///<summary> Map vector *into* local coordinates of Frame </summary>
        public Vector3f ToFrameV(Vector3f v)
        {
            return rotation.InverseMultiply(ref v);
        }
        ///<summary> Map vector *into* local coordinates of Frame </summary>
        public Vector3f ToFrameV(ref Vector3f v)
        {
            return rotation.InverseMultiply(ref v);
        }
        /// <summary> Map vector *from* local frame coordinates into "world" coordinates </summary>
        public Vector3f FromFrameV(Vector3f v)
        {
            return this.rotation * v;
        }
        /// <summary> Map vector *from* local frame coordinates into "world" coordinates </summary>
        public Vector3f FromFrameV(ref Vector3f v)
        {
            return this.rotation * v;
        }

        public bool EpsilonEqual(Frame3f f2, float epsilon)
        {
            return origin.EpsilonEqual(f2.origin, epsilon) &&
                rotation.EpsilonEqual(f2.rotation, epsilon);
        }

        public override string ToString()
        {
            return ToString("F4");
        }
        public string ToString(string fmt)
        {
            return string.Format("[Frame3f: Origin={0}, X={1}, Y={2}, Z={3}]", Origin.ToString(fmt), X.ToString(fmt), Y.ToString(fmt), Z.ToString(fmt));
        }

    }
}
