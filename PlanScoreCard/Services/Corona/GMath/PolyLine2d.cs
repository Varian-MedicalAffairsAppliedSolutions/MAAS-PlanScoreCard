using Dicom.Imaging.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.GMath
{
    public class PolyLine2d : IEnumerable<Vector2d>
    {
        protected List<Vector2d> vertices;
        public int Timestamp;

        public PolyLine2d()
        {
            vertices = new List<Vector2d>();
            Timestamp = 0;
        }

        public PolyLine2d(PolyLine2d copy)
        {
            vertices = new List<Vector2d>(copy.vertices);
            Timestamp = 0;
        }

        public PolyLine2d(IList<Vector2d> copy)
        {
            vertices = new List<Vector2d>(copy);
            Timestamp = 0;
        }

        public PolyLine2d(IEnumerable<Vector2d> copy)
        {
            vertices = new List<Vector2d>(copy);
            Timestamp = 0;
        }


        public PolyLine2d(params Vector2d[] v)
        {
            vertices = new List<Vector2d>(v);
            Timestamp = 0;
        }
        public Vector2d this[int key]
        {
            get { return vertices[key]; }
            set { vertices[key] = value; Timestamp++; }
        }

        public Vector2d Start
        {
            get { return vertices[0]; }
        }
        public Vector2d End
        {
            get { return vertices[vertices.Count - 1]; }
        }

        public AxisAlignedBox2d Bounds => GetBounds();

        private AxisAlignedBox2d GetBounds()
        {
            if (vertices.Count == 0)
            {
                return AxisAlignedBox2d.Empty;
            }

            AxisAlignedBox2d result = new AxisAlignedBox2d(vertices[0]);
            for (int i = 1; i < vertices.Count; i++)
            {
                result.Contain(vertices[i]);
            }

            return result;
        }

        public ReadOnlyCollection<Vector2d> Vertices
        {
            get { return vertices.AsReadOnly(); }
        }

        public int VertexCount
        {
            get { return vertices.Count; }
        }

        public virtual void AppendVertex(Vector2d v)
        {
            vertices.Add(v);
            Timestamp++;
        }

        public virtual void AppendVertices(IEnumerable<Vector2d> v)
        {
            vertices.AddRange(v);
            Timestamp++;
        }


        public virtual void Reverse()
        {
            vertices.Reverse();
            Timestamp++;
        }


        public Vector2d GetTangent(int i)
        {
            if (i == 0)
                return (vertices[1] - vertices[0]).Normalized;
            else if (i == vertices.Count - 1)
                return (vertices[vertices.Count - 1] - vertices[vertices.Count - 2]).Normalized;
            else
                return (vertices[i + 1] - vertices[i - 1]).Normalized;
        }

        public Vector2d GetNormal(int i)
        {
            return GetTangent(i).Perp;
        }

        public IEnumerator<Vector2d> GetEnumerator()
        {
            return vertices.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return vertices.GetEnumerator();
        }


        [System.Obsolete("This method name is confusing. Will remove in future. Use ArcLength instead")]
        public double Length { get { return ArcLength; } }
        public double ArcLength
        {
            get
            {
                double fLength = 0;
                int N = vertices.Count;
                for (int i = 0; i < N - 1; ++i)
                    fLength += vertices[i].Distance(vertices[i + 1]);
                return fLength;
            }
        }


        /// <summary>
        /// Offset each point by dist along vertex normal direction (ie tangent-perp)
        /// </summary>
        public void VertexOffset(double dist)
        {
            Vector2d[] newv = new Vector2d[vertices.Count];
            for (int k = 0; k < vertices.Count; ++k)
                newv[k] = vertices[k] + dist * GetNormal(k);
            for (int k = 0; k < vertices.Count; ++k)
                vertices[k] = newv[k];
        }

        public IEnumerable<Segment2d> GetSegments()
        {
            for (int i = 0; i < vertices.Count - 1; ++i)
                yield return new Segment2d(vertices[i], vertices[i + 1]);
            if (vertices.Count > 2)
                yield return new Segment2d(vertices[vertices.Count - 1], vertices[0]);
        }

        public double SignedArea
        {
            get
            {
                double fArea = 0;
                int N = vertices.Count;
                if (N == 0)
                    return 0;
                Vector2d v1 = vertices[0], v2 = Vector2d.Zero;
                for (int i = 0; i < N; ++i)
                {
                    v2 = vertices[(i + 1) % N];
                    fArea += v1.X * v2.Y - v1.Y * v2.X;
                    v1 = v2;
                }
                return fArea * 0.5;
            }
        }
        public double Area
        {
            get { return Math.Abs(SignedArea); }
        }

    }
}
