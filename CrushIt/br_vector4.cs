using System;

namespace CrushIt
{
    public class br_vector4
    {
        float x;
        float y;
        float z;
        float w;

        public static br_vector4 Zero => new br_vector4(0, 0, 0, 0);

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

        public float W
        {
            get => w;
            set => w = value;
        }

        public br_vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static br_vector4 Parse(string v)
        {
            v = v.Replace(" ", "");
            string[] s = v.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return new br_vector4(s[0].ToSingle(), s[1].ToSingle(), s[2].ToSingle(), s[3].ToSingle());
        }
    }
}