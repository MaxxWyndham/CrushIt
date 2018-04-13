using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CrushIt
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CrushIt.Culture;

            CrushFile settings = new CrushFile();
            br_model crushMesh = null;

            settings.Load(Path.Combine(Directory.GetCurrentDirectory(), "default.crush"));

            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dat"))
            {
                settings.Load(Path.Combine(Directory.GetCurrentDirectory(), $"{Path.GetFileNameWithoutExtension(file)}.crush"));

                Console.WriteLine($"Using {settings.Source}");
                Console.WriteLine($"Crushing {Path.GetFileName(file)}...");

                DAT dat = DAT.Load(file);

                if (dat != null)
                {
                    foreach (DatMesh mesh in dat.Meshes)
                    {
                        Console.WriteLine($"   {mesh.Name}");
                        Console.WriteLine($"      Faces     :  {mesh.Mesh.Faces.Count}");
                        Console.WriteLine($"      Verts     :  {mesh.Mesh.Vertices.Count}");

                        if (mesh.Mesh.Vertices.Count > 100)
                        {
                            mesh.Mesh.CalculateCrush(settings);

                            crushMesh = mesh.Mesh;

                            Console.WriteLine($"      Bounds.Min: {mesh.Mesh.Bounds.Min.X}, {mesh.Mesh.Bounds.Min.Y}, {mesh.Mesh.Bounds.Min.Z}");
                            Console.WriteLine($"      Bounds.Max:  {mesh.Mesh.Bounds.Max.X},  {mesh.Mesh.Bounds.Max.Y},  {mesh.Mesh.Bounds.Max.Z}");
                            Console.WriteLine();

                            using (StreamWriter output = new StreamWriter(Path.Combine(Path.GetDirectoryName(file), $"{mesh.Name}.txt"), false))
                            {
                                output.WriteLine("// CRUSH DATA");
                                output.WriteLine($"{mesh.Mesh.Crush.SoftnessFactor:0.000000}");
                                output.WriteLine($"{mesh.Mesh.Crush.FoldFactor.X:0.000000},{mesh.Mesh.Crush.FoldFactor.Y:0.000000}");
                                output.WriteLine($"{mesh.Mesh.Crush.WibbleFactor:0.000000}");
                                output.WriteLine($"{mesh.Mesh.Crush.LimitDeviant:0.000000}");
                                output.WriteLine($"{mesh.Mesh.Crush.SplitChance:0.000000}");
                                output.WriteLine($"{mesh.Mesh.Crush.MinYFoldDown:0.000000}");

                                output.WriteLine($"{mesh.Mesh.Crush.Points.Count}");

                                foreach (br_crushpoint point in mesh.Mesh.Crush.Points)
                                {
                                    output.WriteLine($"{point.VertIndex}");
                                    output.WriteLine($"{point.LimitMin.X:0.000000}, {point.LimitMin.Y:0.000000}, {point.LimitMin.Z:0.000000}");
                                    output.WriteLine($"{point.LimitMax.X:0.000000}, {point.LimitMax.Y:0.000000}, {point.LimitMax.Z:0.000000}");
                                    output.WriteLine($"{point.SoftnessNeg.X:0.000000}, {point.SoftnessNeg.Y:0.000000}, {point.SoftnessNeg.Z:0.000000}");
                                    output.WriteLine($"{point.SoftnessPos.X:0.000000}, {point.SoftnessPos.Y:0.000000}, {point.SoftnessPos.Z:0.000000}");

                                    output.WriteLine($"{point.Neighbours.Count}");

                                    foreach (br_crushneighbour neighbour in point.Neighbours)
                                    {
                                        output.WriteLine($"{neighbour.VertIndex}");
                                        output.WriteLine($"{neighbour.Factor}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("      Too few verts to crush, skipping...");
                        }

                        Console.WriteLine();
                    }
                }
            }

            if (false)
            {
                foreach (string file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Cars"), "*.txt"))
                {
                    Car car = Car.Load(file);
                    DAT dat = DAT.Load(Path.Combine(Directory.GetCurrentDirectory(), "Cars", car.Models[2]));
                    br_model model = new br_model();

                    using (StreamWriter output = new StreamWriter(Path.Combine(Path.GetDirectoryName(file), "monkey.log"), false))
                    {
                        foreach (DatMesh mesh in dat.Meshes)
                        {
                            mesh.Mesh.CalculateExtents();

                            if (mesh.Mesh.Vertices.Count > model.Vertices.Count)
                            {
                                model = mesh.Mesh;
                            }
                        }

                        float maxSide = 0;
                        float maxFront = 0;
                        float maxBack = 0;
                        float maxTop = 0;
                        float maxBottom = 0;

                        float crushPointMergeTolerance = 1;
                        float localitudeOfDamage = 0;

                        for (int i = 0; i < model.Vertices.Count; i++)
                        {
                            br_vector3 vert = model.Vertices[i];

                            //float width = model.Bounds.Max.X - model.Bounds.Min.X;
                            //float height = model.Bounds.Max.Y - model.Bounds.Min.Y;
                            //float length = model.Bounds.Max.Z - model.Bounds.Min.Z;

                            float left = 1 - vert.X / model.Bounds.Min.X;
                            float right = 1 - vert.X / model.Bounds.Max.X;
                            float bottom = 1 - vert.Y / model.Bounds.Min.Y;
                            float top = 1 - vert.Y / model.Bounds.Max.Y;
                            float front = 1 - vert.Z / model.Bounds.Min.Z;
                            float back = 1 - vert.Z / model.Bounds.Max.Z;

                            float minX = Math.Min(left, right);
                            float minY = Math.Min(top, bottom);
                            float minZ = Math.Min(front, back);

                            if (minX <= minY && minX <= minZ)
                            {
                                maxSide = Math.Max(maxSide, minX);
                            }
                            else if (minY <= minX && minY <= minZ)
                            {
                                if (top < bottom)
                                {
                                    maxTop = Math.Max(maxTop, top);
                                }
                                else
                                {
                                    maxBottom = Math.Max(maxBottom, bottom);
                                }
                            }
                            else if (minZ <= minX && minZ <= minY)
                            {
                                if (front < back)
                                {
                                    maxFront = Math.Max(maxFront, front);
                                }
                                else
                                {
                                    maxBack = Math.Max(maxBack, back);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Huh?!");
                            }

                            br_crushpoint point = car.Crushes[1].Points.Where(p => p.VertIndex == i).FirstOrDefault();
                            br_crushpoint myPoint = crushMesh.Crush.Points.Where(p => p.VertIndex == i).FirstOrDefault();

                            if (point != null)
                            {
                                int neighbourIndex = -1;

                                foreach (br_crushneighbour neighbour in point.Neighbours)
                                {
                                    if (neighbour.VertIndex == 0)
                                    {
                                        neighbourIndex += neighbour.Factor;
                                        continue;
                                    }

                                    neighbourIndex += neighbour.VertIndex;

                                    float dist = vert.DistanceTo(model.Vertices[neighbourIndex]);

                                    localitudeOfDamage = Math.Max(dist, localitudeOfDamage);
                                }
                            }

                            int closestPoint = -1;
                            float closestPointDist = float.MaxValue;

                            for (int j = 0; j < model.Vertices.Count; j++)
                            {
                                if (j == i) { continue; }

                                br_crushpoint matchPoint = car.Crushes[1].Points.Where(p => p.VertIndex == j).FirstOrDefault();

                                if (point != null && matchPoint != null)
                                {
                                    float pointDist = vert.DistanceTo(model.Vertices[j]);

                                    Console.WriteLine($"{i} => {j} == {pointDist}");

                                    crushPointMergeTolerance = Math.Min(pointDist, crushPointMergeTolerance);
                                }

                                if (myPoint != null && point == null && matchPoint != null)
                                {
                                    float pointDist = vert.DistanceTo(model.Vertices[j]);

                                    if (pointDist <= closestPointDist)
                                    {
                                        closestPoint = j;
                                        closestPointDist = pointDist;
                                    }
                                }
                            }

                            output.WriteLine($"{i}\t\t{vert.X}\t{vert.Y}\t{vert.Z}\t{(point != null)}\t{(myPoint != null)}\t{point != null || myPoint != null}\t{left}\t{right}\t{top}\t{bottom}\t{front}\t{back}\t{closestPoint}\t{(closestPoint > -1 ? closestPointDist : 0)}");
                        }

                        output.WriteLine("==================");
                        output.WriteLine($"float localitudeOfDamage = {localitudeOfDamage}f;");
                        output.WriteLine($"float crushPointMergeTolerance = {crushPointMergeTolerance}f;");
                        output.WriteLine($"float activeCrushBoundsSides = {maxSide}f;");
                        output.WriteLine($"float activeCrushBoundsFront = {maxFront}f;");
                        output.WriteLine($"float activeCrushBoundsBack = {maxBack}f;");
                        output.WriteLine($"float activeCrushBoundsTop = {maxTop}f;");
                        output.WriteLine($"float activeCrushBoundsBottom = {maxBottom}f;");
                    }
                }
            }


            Console.WriteLine("Press ENTER to EXIT");
            Console.ReadLine();
        }
    }
}