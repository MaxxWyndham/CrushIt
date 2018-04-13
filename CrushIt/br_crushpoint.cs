using System.Collections.Generic;

namespace CrushIt
{
    public class br_crushpoint
    {
        int vertIndex;
        br_vector3 limitMin;
        br_vector3 limitMax;
        br_vector3 softnessNeg;
        br_vector3 softnessPos;
        List<br_crushneighbour> neighbours;

        public int VertIndex
        {
            get => vertIndex;
            set => vertIndex = value;
        }

        public br_vector3 LimitMin
        {
            get => limitMin;
            set => limitMin = value;
        }

        public br_vector3 LimitMax
        {
            get => limitMax;
            set => limitMax = value;
        }

        public br_vector3 SoftnessNeg
        {
            get => softnessNeg;
            set => softnessNeg = value;
        }

        public br_vector3 SoftnessPos
        {
            get => softnessPos;
            set => softnessPos = value;
        }

        public List<br_crushneighbour> Neighbours
        {
            get => neighbours;
            set => neighbours = value;
        }

        public br_crushpoint()
        {
            limitMin = br_vector3.Zero;
            limitMax = br_vector3.Zero;
            softnessNeg = br_vector3.Zero;
            softnessPos = br_vector3.Zero;

            neighbours = new List<br_crushneighbour>();
        }

        public br_crushpoint(DocumentParser file) :
            this()
        {
            vertIndex = file.ReadInt();
            limitMin = file.ReadVector3();
            limitMax = file.ReadVector3();
            softnessNeg = file.ReadVector3();
            softnessPos = file.ReadVector3();

            int neighbourCount = file.ReadInt();
            for (int i = 0; i < neighbourCount; i++)
            {
                neighbours.Add(new br_crushneighbour(file));
            }
        }
    }
}