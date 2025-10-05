using System;

namespace CoronaDVH.GMath
{
    public struct Matrix3f
    {
        public Vector3f Row0;
        public Vector3f Row1;
        public Vector3f Row2;

        public Matrix3f(bool bIdentity)
        {
            if (bIdentity)
            {
                Row0 = Vector3f.AxisX; Row1 = Vector3f.AxisY; Row2 = Vector3f.AxisZ;
            }
            else
            {
                Row0 = Row1 = Row2 = Vector3f.Zero;
            }
        }

        // assumes input is row-major...
        public Matrix3f(float[,] mat)
        {
            Row0 = new Vector3f(mat[0, 0], mat[0, 1], mat[0, 2]);
            Row1 = new Vector3f(mat[1, 0], mat[1, 1], mat[1, 2]);
            Row2 = new Vector3f(mat[2, 0], mat[2, 1], mat[2, 2]);
        }
        public Matrix3f(float[] mat)
        {
            Row0 = new Vector3f(mat[0], mat[1], mat[2]);
            Row1 = new Vector3f(mat[3], mat[4], mat[5]);
            Row2 = new Vector3f(mat[6], mat[7], mat[8]);
        }
        public Matrix3f(double[,] mat)
        {
            Row0 = new Vector3f(mat[0, 0], mat[0, 1], mat[0, 2]);
            Row1 = new Vector3f(mat[1, 0], mat[1, 1], mat[1, 2]);
            Row2 = new Vector3f(mat[2, 0], mat[2, 1], mat[2, 2]);
        }
        public Matrix3f(double[] mat)
        {
            Row0 = new Vector3f(mat[0], mat[1], mat[2]);
            Row1 = new Vector3f(mat[3], mat[4], mat[5]);
            Row2 = new Vector3f(mat[6], mat[7], mat[8]);
        }
        public Matrix3f(Func<int, float> matBufferF)
        {
            Row0 = new Vector3f(matBufferF(0), matBufferF(1), matBufferF(2));
            Row1 = new Vector3f(matBufferF(3), matBufferF(4), matBufferF(5));
            Row2 = new Vector3f(matBufferF(6), matBufferF(7), matBufferF(8));
        }
        public Matrix3f(Func<int, int, float> matF)
        {
            Row0 = new Vector3f(matF(0, 0), matF(0, 1), matF(0, 2));
            Row1 = new Vector3f(matF(1, 0), matF(1, 1), matF(1, 2));
            Row2 = new Vector3f(matF(2, 0), matF(1, 2), matF(2, 2));
        }
        public Matrix3f(float m00, float m11, float m22)
        {
            Row0 = new Vector3f(m00, 0, 0);
            Row1 = new Vector3f(0, m11, 0);
            Row2 = new Vector3f(0, 0, m22);
        }
        public Matrix3f(Vector3f v1, Vector3f v2, Vector3f v3, bool bRows)
        {
            if (bRows)
            {
                Row0 = v1; Row1 = v2; Row2 = v3;
            }
            else
            {
                Row0 = new Vector3f(v1.X, v2.X, v3.X);
                Row1 = new Vector3f(v1.Y, v2.Y, v3.Y);
                Row2 = new Vector3f(v1.Z, v2.Z, v3.Z);
            }
        }
        public Matrix3f(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
        {
            Row0 = new Vector3f(m00, m01, m02);
            Row1 = new Vector3f(m10, m11, m12);
            Row2 = new Vector3f(m20, m21, m22);
        }


        public static readonly Matrix3f Identity = new Matrix3f(true);
        public static readonly Matrix3f Zero = new Matrix3f(false);



        public float this[int r, int c]
        {
            get
            {
                return (r == 0) ? Row0[c] : ((r == 1) ? Row1[c] : Row2[c]);
            }
            set
            {
                if (r == 0) Row0[c] = value;
                else if (r == 1) Row1[c] = value;
                else Row2[c] = value;
            }
        }


        public float this[int i]
        {
            get
            {
                return (i > 5) ? Row2[i % 3] : ((i > 2) ? Row1[i % 3] : Row0[i % 3]);
            }
            set
            {
                if (i > 5) Row2[i % 3] = value;
                else if (i > 2) Row1[i % 3] = value;
                else Row0[i % 3] = value;
            }
        }



        public Vector3f Row(int i)
        {
            return (i == 0) ? Row0 : (i == 1) ? Row1 : Row2;
        }
        public Vector3f Column(int i)
        {
            if (i == 0) return new Vector3f(Row0.X, Row1.X, Row2.X);
            else if (i == 1) return new Vector3f(Row0.Y, Row1.Y, Row2.Y);
            else return new Vector3f(Row0.Z, Row1.Z, Row2.Z);
        }


        public float[] ToBuffer()
        {
            return new float[9] {
                Row0.X, Row0.Y, Row0.Z,
                Row1.X, Row1.Y, Row1.Z,
                Row2.X, Row2.Y, Row2.Z };
        }
        public void ToBuffer(float[] buf)
        {
            buf[0] = Row0.X; buf[1] = Row0.Y; buf[2] = Row0.Z;
            buf[3] = Row1.X; buf[4] = Row1.Y; buf[5] = Row1.Z;
            buf[6] = Row2.X; buf[7] = Row2.Y; buf[8] = Row2.Z;
        }




        public static Matrix3f operator *(Matrix3f mat, float f)
        {
            return new Matrix3f(
                mat.Row0.X * f, mat.Row0.Y * f, mat.Row0.Z * f,
                mat.Row1.X * f, mat.Row1.Y * f, mat.Row1.Z * f,
                mat.Row2.X * f, mat.Row2.Y * f, mat.Row2.Z * f);
        }
        public static Matrix3f operator *(float f, Matrix3f mat)
        {
            return new Matrix3f(
                mat.Row0.X * f, mat.Row0.Y * f, mat.Row0.Z * f,
                mat.Row1.X * f, mat.Row1.Y * f, mat.Row1.Z * f,
                mat.Row2.X * f, mat.Row2.Y * f, mat.Row2.Z * f);
        }


        public static Vector3f operator *(Matrix3f mat, Vector3f v)
        {
            return new Vector3f(
                mat.Row0.X * v.X + mat.Row0.Y * v.Y + mat.Row0.Z * v.Z,
                mat.Row1.X * v.X + mat.Row1.Y * v.Y + mat.Row1.Z * v.Z,
                mat.Row2.X * v.X + mat.Row2.Y * v.Y + mat.Row2.Z * v.Z);
        }

        public Vector3f Multiply(ref Vector3f v)
        {
            return new Vector3f(
                Row0.X * v.X + Row0.Y * v.Y + Row0.Z * v.Z,
                Row1.X * v.X + Row1.Y * v.Y + Row1.Z * v.Z,
                Row2.X * v.X + Row2.Y * v.Y + Row2.Z * v.Z);
        }

        public void Multiply(ref Vector3f v, ref Vector3f vOut)
        {
            vOut.X = Row0.X * v.X + Row0.Y * v.Y + Row0.Z * v.Z;
            vOut.Y = Row1.X * v.X + Row1.Y * v.Y + Row1.Z * v.Z;
            vOut.Z = Row2.X * v.X + Row2.Y * v.Y + Row2.Z * v.Z;
        }

        public static Matrix3f operator *(Matrix3f mat1, Matrix3f mat2)
        {
            float m00 = mat1.Row0.X * mat2.Row0.X + mat1.Row0.Y * mat2.Row1.X + mat1.Row0.Z * mat2.Row2.X;
            float m01 = mat1.Row0.X * mat2.Row0.Y + mat1.Row0.Y * mat2.Row1.Y + mat1.Row0.Z * mat2.Row2.Y;
            float m02 = mat1.Row0.X * mat2.Row0.Z + mat1.Row0.Y * mat2.Row1.Z + mat1.Row0.Z * mat2.Row2.Z;

            float m10 = mat1.Row1.X * mat2.Row0.X + mat1.Row1.Y * mat2.Row1.X + mat1.Row1.Z * mat2.Row2.X;
            float m11 = mat1.Row1.X * mat2.Row0.Y + mat1.Row1.Y * mat2.Row1.Y + mat1.Row1.Z * mat2.Row2.Y;
            float m12 = mat1.Row1.X * mat2.Row0.Z + mat1.Row1.Y * mat2.Row1.Z + mat1.Row1.Z * mat2.Row2.Z;

            float m20 = mat1.Row2.X * mat2.Row0.X + mat1.Row2.Y * mat2.Row1.X + mat1.Row2.Z * mat2.Row2.X;
            float m21 = mat1.Row2.X * mat2.Row0.Y + mat1.Row2.Y * mat2.Row1.Y + mat1.Row2.Z * mat2.Row2.Y;
            float m22 = mat1.Row2.X * mat2.Row0.Z + mat1.Row2.Y * mat2.Row1.Z + mat1.Row2.Z * mat2.Row2.Z;

            return new Matrix3f(m00, m01, m02, m10, m11, m12, m20, m21, m22);
        }



        public static Matrix3f operator +(Matrix3f mat1, Matrix3f mat2)
        {
            return new Matrix3f(mat1.Row0 + mat2.Row0, mat1.Row1 + mat2.Row1, mat1.Row2 + mat2.Row2, true);
        }
        public static Matrix3f operator -(Matrix3f mat1, Matrix3f mat2)
        {
            return new Matrix3f(mat1.Row0 - mat2.Row0, mat1.Row1 - mat2.Row1, mat1.Row2 - mat2.Row2, true);
        }


        public float Determinant
        {
            get
            {
                float a11 = Row0.X, a12 = Row0.Y, a13 = Row0.Z, a21 = Row1.X, a22 = Row1.Y, a23 = Row1.Z, a31 = Row2.X, a32 = Row2.Y, a33 = Row2.Z;
                float i00 = a33 * a22 - a32 * a23;
                float i01 = -(a33 * a12 - a32 * a13);
                float i02 = a23 * a12 - a22 * a13;
                return a11 * i00 + a21 * i01 + a31 * i02;
            }
        }


        public Matrix3f Inverse()
        {
            float a11 = Row0.X, a12 = Row0.Y, a13 = Row0.Z, a21 = Row1.X, a22 = Row1.Y, a23 = Row1.Z, a31 = Row2.X, a32 = Row2.Y, a33 = Row2.Z;
            float i00 = a33 * a22 - a32 * a23;
            float i01 = -(a33 * a12 - a32 * a13);
            float i02 = a23 * a12 - a22 * a13;

            float i10 = -(a33 * a21 - a31 * a23);
            float i11 = a33 * a11 - a31 * a13;
            float i12 = -(a23 * a11 - a21 * a13);

            float i20 = a32 * a21 - a31 * a22;
            float i21 = -(a32 * a11 - a31 * a12);
            float i22 = a22 * a11 - a21 * a12;

            float det = a11 * i00 + a21 * i01 + a31 * i02;
            if (Math.Abs(det) < float.Epsilon)
                throw new Exception("Matrix3f.Inverse: matrix is not invertible");
            det = 1.0f / det;
            return new Matrix3f(i00 * det, i01 * det, i02 * det, i10 * det, i11 * det, i12 * det, i20 * det, i21 * det, i22 * det);
        }

        public Matrix3f Transpose()
        {
            return new Matrix3f(
                Row0.X, Row1.X, Row2.X,
                Row0.Y, Row1.Y, Row2.Y,
                Row0.Z, Row1.Z, Row2.Z);
        }

        public Quaternionf ToQuaternion()
        {
            return new Quaternionf(this);
        }

        public bool EpsilonEqual(Matrix3f m2, float epsilon)
        {
            return Row0.EpsilonEqual(m2.Row0, epsilon) &&
                Row1.EpsilonEqual(m2.Row1, epsilon) &&
                Row2.EpsilonEqual(m2.Row2, epsilon);
        }


        public static Matrix3f AxisAngleD(Vector3f axis, float angleDeg)
        {
            double angle = angleDeg * MathUtil.Deg2Rad;
            float cs = (float)Math.Cos(angle);
            float sn = (float)Math.Sin(angle);
            float oneMinusCos = 1.0f - cs;
            float x2 = axis[0] * axis[0];
            float y2 = axis[1] * axis[1];
            float z2 = axis[2] * axis[2];
            float xym = axis[0] * axis[1] * oneMinusCos;
            float xzm = axis[0] * axis[2] * oneMinusCos;
            float yzm = axis[1] * axis[2] * oneMinusCos;
            float xSin = axis[0] * sn;
            float ySin = axis[1] * sn;
            float zSin = axis[2] * sn;
            return new Matrix3f(
                x2 * oneMinusCos + cs, xym - zSin, xzm + ySin,
                xym + zSin, y2 * oneMinusCos + cs, yzm - xSin,
                xzm - ySin, yzm + xSin, z2 * oneMinusCos + cs);
        }




        public override string ToString()
        {
            return string.Format("[{0}] [{1}] [{2}]", Row0, Row1, Row2);
        }
        public string ToString(string fmt)
        {
            return string.Format("[{0}] [{1}] [{2}]", Row0.ToString(fmt), Row1.ToString(fmt), Row2.ToString(fmt));
        }
    }
}