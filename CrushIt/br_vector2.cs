using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrushIt
{
    public class br_vector2
    {
        float x;
        float y;

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

        public float Min
        {
            get => x;
            set => x = value;
        }

        public float Max
        {
            get => y;
            set => y = value;
        }

        public br_vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static br_vector2 Parse(string v)
        {
            v = v.Replace(" ", "");
            string[] s = v.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return new br_vector2(s[0].ToSingle(), s[1].ToSingle());
        }
    }
}