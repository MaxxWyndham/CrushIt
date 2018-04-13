using System;

namespace CrushIt
{
    public class br_vector3
    {
        float x;
        float y;
        float z;

        public static br_vector3 Zero => new br_vector3(0, 0, 0);

        public float X
        {
            get => x;
            set => x = value;
        }

        public float Y
        {
            get => y;
            set => y = value;
        }

        public float Z
        {
            get => z;
            set => z = value;
        }

        public br_vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float DistanceTo(br_vector3 point)
        {
            float dX = point.X - x;
            float dY = point.Y - y;
            float dZ = point.Z - z;

            return (float)Math.Sqrt(dX * dX + dY * dY + dZ * dZ);
        }

        public static br_vector3 Parse(string v)
        {
            v = v.Replace(" ", "");
            string[] s = v.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return new br_vector3(s[0].ToSingle(), s[1].ToSingle(), s[2].ToSingle());
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
        }

        public override bool Equals(object obj)
        {
            return (obj as br_vector3).GetHashCode() == GetHashCode();
        }
    }
}