
namespace CoronaDVH.GMath
{
    public class IndexUtil
    {
        public static Vector3i ToGrid3Index(int idx, int nx, int ny)
        {
            int x = idx % nx;
            int y = idx / nx % ny;
            int z = idx / (nx * ny);
            return new Vector3i(x, y, z);
        }

        public static int ToGrid2Linear(int i, int j, int nx)
        {
            return i + nx * j;
        }
    }
}
