using System.Collections.Generic;

namespace CrushIt
{
    public class br_crush
    {
        float softnessFactor;
        br_vector2 foldFactor;
        float wibbleFactor;
        float limitDeviant;
        float splitChance;
        float minYFoldDown;
        List<br_crushpoint> crushPoints;

        public float SoftnessFactor
        {
            get => softnessFactor;
            set => softnessFactor = value;
        }

        public br_vector2 FoldFactor
        {
            get => foldFactor;
            set => foldFactor = value;
        }

        public float WibbleFactor
        {
            get => wibbleFactor;
            set => wibbleFactor = value;
        }

        public float LimitDeviant
        {
            get => limitDeviant;
            set => limitDeviant = value;
        }

        public float SplitChance
        {
            get => splitChance;
            set => splitChance = value;
        }

        public float MinYFoldDown
        {
            get => minYFoldDown;
            set => minYFoldDown = value;
        }

        public List<br_crushpoint> Points
        {
            get => crushPoints;
            set => crushPoints = value;
        }

        public br_crush()
        {
            softnessFactor = 0.75f;
            foldFactor = new br_vector2(0.15f, 0.4f);
            wibbleFactor = 0.05f;
            limitDeviant = 0.05f;

            crushPoints = new List<br_crushpoint>();
        }

        public br_crush(DocumentParser file)
            : this()
        {
            softnessFactor = file.ReadSingle();
            foldFactor = file.ReadVector2();
            wibbleFactor = file.ReadSingle();
            limitDeviant = file.ReadSingle();
            splitChance = file.ReadSingle();
            minYFoldDown = file.ReadSingle();

            int pointCount = file.ReadInt();
            for (int i = 0; i < pointCount; i++)
            {
                crushPoints.Add(new br_crushpoint(file));
            }
        }
    }
}