using System;
using System.IO;
using System.Reflection;

namespace CrushIt
{
    public class CrushFile
    {
        string source = "core settings";

        int minVerts = 100;

        float overallSoftness = 0.3f;
        float deformability = 0.1f;
        float localitudeOfDamage = 0.2f;
        float crushPointMergeTolerance = 0.03f;

        float relativeSoftnessFront = 0.5f;
        float relativeSoftnessBack = 0.5f;
        float relativeSoftnessTop = 0.3f;
        float relativeSoftnessBottom = 0.3f;
        float relativeSoftnessSidesEnd = 0.5f;
        float relativeSoftnessSidesMiddle = 0.75f;

        br_vector2 positionOfMiddleOfSides = new br_vector2(0.3f, 0.7f);

        float activeCrushBoundsSides = 0.35f;
        float activeCrushBoundsFront = 0.2f;
        float activeCrushBoundsBack = 0.2f;
        float activeCrushBoundsTop = 0.15f;
        float activeCrushBoundsBottom = 0.1f;

        public string Source
        {
            get => source;
            set => source = value;
        }

        public int MinVerts
        {
            get => minVerts;
            set => minVerts = value;
        }

        public float OverallSoftness
        {
            get => overallSoftness;
            set => overallSoftness = value;
        }

        public float Deformability
        {
            get => deformability;
            set => deformability = value;
        }

        public float LocalitudeOfDamage
        {
            get => localitudeOfDamage;
            set => localitudeOfDamage = value;
        }

        public float CrushPointMergeTolerance
        {
            get => crushPointMergeTolerance;
            set => crushPointMergeTolerance = value;
        }

        public float RelativeSoftnessFront
        {
            get => relativeSoftnessFront;
            set => relativeSoftnessFront = value;
        }

        public float RelativeSoftnessBack
        {
            get => relativeSoftnessBack;
            set => relativeSoftnessBack = value;
        }

        public float RelativeSoftnessTop
        {
            get => relativeSoftnessTop;
            set => relativeSoftnessTop = value;
        }

        public float RelativeSoftnessBottom
        {
            get => relativeSoftnessBottom;
            set => relativeSoftnessBottom = value;
        }

        public float RelativeSoftnessSidesEnd
        {
            get => relativeSoftnessSidesEnd;
            set => relativeSoftnessSidesEnd = value;
        }

        public float RelativeSoftnessSidesMiddle
        {
            get => relativeSoftnessSidesMiddle;
            set => relativeSoftnessSidesMiddle = value;
        }

        public br_vector2 PositionOfMiddleOfSides
        {
            get => positionOfMiddleOfSides;
            set => positionOfMiddleOfSides = value;
        }

        public float ActiveCrushBoundsSides
        {
            get => activeCrushBoundsSides;
            set => activeCrushBoundsSides = value;
        }

        public float ActiveCrushBoundsFront
        {
            get => activeCrushBoundsFront;
            set => activeCrushBoundsFront = value;
        }

        public float ActiveCrushBoundsBack
        {
            get => activeCrushBoundsBack;
            set => activeCrushBoundsBack = value;
        }

        public float ActiveCrushBoundsTop
        {
            get => activeCrushBoundsTop;
            set => activeCrushBoundsTop = value;
        }

        public float ActiveCrushBoundsBottom
        {
            get => activeCrushBoundsBottom;
            set => activeCrushBoundsBottom = value;
        }

        private object this[string propertyName]
        {
            set
            {
                Type type = typeof(CrushFile);
                PropertyInfo property = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                if (property != null)
                {
                    property.SetValue(this, value, null);
                }
                else
                {
                    Console.WriteLine($"  Invalid setting: {propertyName}");
                }
            }
        }

        public void Load(string file)
        {
            if (File.Exists(file))
            {
                source = Path.GetFileName(file);

                foreach (string line in File.ReadAllLines(file))
                {
                    if (line.Trim().StartsWith("#") || line.Trim().Length == 0) { continue; }

                    string[] parts = line.Split(' ');

                    switch (parts[0])
                    {
                        case "minverts":
                            minVerts = parts[2].ToInt();
                            break;

                        case "positionofmiddleofsides.front":
                            positionOfMiddleOfSides.X = parts[2].ToSingle();
                            break;

                        case "positionofmiddleofsides.back":
                            positionOfMiddleOfSides.Y = parts[2].ToSingle();
                            break;

                        default:
                            this[parts[0].Replace(".", "")] = parts[2].ToSingle();
                            break;
                    }
                }
            }
        }
    }
}
