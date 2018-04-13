using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CrushIt
{
    public class DAT
    {
        List<DatMesh> meshes = new List<DatMesh>();

        public List<DatMesh> Meshes
        {
            get => meshes;
            set => meshes = value;
        }

        public static DAT Load(string path)
        {
            FileInfo fi = new FileInfo(path);
            DAT dat = new DAT();

            DatMesh D = new DatMesh();
            int count = 0;

            using (BEBinaryReader br = new BEBinaryReader(fi.OpenRead(), Encoding.Default))
            {
                if (br.ReadUInt32() != 0x0012 ||
                    br.ReadUInt32() != 0x0008 ||
                    br.ReadUInt32() != 0xface ||
                    br.ReadUInt32() != 0x0002)
                {
                    Console.WriteLine($"{path} isn't a valid DAT file");
                    return null;
                }

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    int tag = (int)br.ReadUInt32();
                    int length = (int)br.ReadUInt32();

                    switch (tag)
                    {
                        case 54: // 00 00 00 36
                            D = new DatMesh()
                            {
                                UnknownAttribute = br.ReadUInt16(),   // I think this is actually two byte values
                                Name = br.ReadString()
                            };
                            break;

                        case 23: // 00 00 00 17 : vertex data
                            count = (int)br.ReadUInt32();

                            for (int i = 0; i < count; i++)
                            {
                                D.Mesh.Vertices.Add(new br_vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                            }
                            break;

                        case 24: // 00 00 00 18 : UV co-ordinates
                            count = (int)br.ReadUInt32();

                            for (int i = 0; i < count; i++)
                            {
                                D.Mesh.UVs.Add(new br_vector2(br.ReadSingle(), br.ReadSingle()));
                            }
                            break;

                        case 53:    // 00 00 00 35 : faces
                            count = (int)br.ReadUInt32();

                            for (int i = 0; i < count; i++)
                            {
                                D.Mesh.Faces.Add(
                                    new br_face(br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16())
                                    {
                                        SmoothingGroup = br.ReadUInt16()
                                    }
                                );

                                br.ReadByte(); // number of edges, 0 and 3 = tri.  4 = quad.
                            }
                            break;

                        case 22: // 00 00 00 16 : material list
                            D.Mesh.Materials.AddRange(br.ReadStrings((int)br.ReadUInt32()));
                            break;

                        case 26: // 00 00 00 1A : face textures
                            count = (int)br.ReadUInt32();
                            br.ReadBytes(4); // fuck knows what this is
                            for (int i = 0; i < count; i++)
                            {
                                D.Mesh.Faces[i].Material = br.ReadUInt16() - 1;
                            }
                            break;

                        case 0:
                            // EndOfFile
                            dat.Meshes.Add(D);
                            break;

                        default:
                            Console.WriteLine($"Unknown DAT tag: {tag} ({br.BaseStream.Position:x2})");
                            return null;
                    }
                }
            }

            return dat;
        }
    }

    public class DatMesh
    {
        int unknownAttribute;
        string name;
        br_model mesh = new br_model();

        public int UnknownAttribute
        {
            get => unknownAttribute;
            set => unknownAttribute = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public br_model Mesh
        {
            get => mesh;
            set => mesh = value;
        }
    }

    public class br_model
    {
        List<br_vector3> vertices;
        List<br_vector2> uvs;
        List<br_face> faces;
        List<string> materials;
        br_bounds bounds;
        br_crush crush;

        public List<br_vector3> Vertices
        {
            get => vertices;
            set => vertices = value;
        }

        public List<br_vector2> UVs
        {
            get => uvs;
            set => uvs = value;
        }

        public List<br_face> Faces
        {
            get => faces;
            set => faces = value;
        }

        public List<string> Materials
        {
            get => materials;
            set => materials = value;
        }

        public br_bounds Bounds
        {
            get => bounds;
            set => bounds = value;
        }

        public br_crush Crush
        {
            get => crush;
            set => crush = value;
        }

        public br_model()
        {
            vertices = new List<br_vector3>();
            uvs = new List<br_vector2>();
            faces = new List<br_face>();
            materials = new List<string>();

            bounds = new br_bounds();
            crush = new br_crush();
        }

        public void CalculateCrush(CrushFile settings)
        {
            CalculateExtents();

            br_bounds subExtents = new br_bounds();

            subExtents.Min.X = bounds.Min.X * (1 - settings.ActiveCrushBoundsSides);
            subExtents.Min.Y = bounds.Min.Y * (1 - settings.ActiveCrushBoundsBottom);
            subExtents.Min.Z = bounds.Min.Z * (1 - settings.ActiveCrushBoundsFront);
            subExtents.Max.X = bounds.Max.X * (1 - settings.ActiveCrushBoundsSides);
            subExtents.Max.Y = bounds.Max.Y * (1 - settings.ActiveCrushBoundsTop);
            subExtents.Max.Z = bounds.Max.Z * (1 - settings.ActiveCrushBoundsBack);

            Dictionary<br_vector3, int> verts = new Dictionary<br_vector3, int>();
            Dictionary<br_vector3, List<int>> dupes = new Dictionary<br_vector3, List<int>>();

            for (int i = 0; i < vertices.Count; i++)
            {
                bool added = false;

                if (!verts.ContainsKey(vertices[i]))
                {
                    foreach (KeyValuePair<br_vector3, int> kvp in verts.OrderBy(v => v.Value))
                    {
                        float dX = vertices[i].X - vertices[kvp.Value].X;
                        float dY = vertices[i].Y - vertices[kvp.Value].Y;
                        float dZ = vertices[i].Z - vertices[kvp.Value].Z;
                        //float dX = vertices[i].X - kvp.Key.X;
                        //float dY = vertices[i].Y - kvp.Key.Y;
                        //float dZ = vertices[i].Z - kvp.Key.Z;

                        float distance = (float)Math.Sqrt(dX * dX + dY * dY + dZ * dZ);

                        if (distance <= settings.CrushPointMergeTolerance)
                        {
                            //Console.WriteLine($"Replace {i} over {kvp.Value}");
                            //Console.WriteLine($"   for {vertices[i].X}, {vertices[i].Y}, {vertices[i].Z} => {kvp.Key.X}, {kvp.Key.Y}, {kvp.Key.Z} (it's close enough: {distance})");
                            verts[kvp.Key] = i;
                            dupes[kvp.Key].Add(i);
                            added = true;
                            break;
                        }
                    }
                }

                if (!added)
                {
                    if (verts.ContainsKey(vertices[i]))
                    {
                        //Console.WriteLine($"Replace {i} over {verts[vertices[i]]} for {vertices[i].X}, {vertices[i].Y}, {vertices[i].Z}");
                        dupes[vertices[i]].Add(i);
                    }
                    else
                    {
                        //Console.WriteLine($"Add {i} for {vertices[i].X}, {vertices[i].Y}, {vertices[i].Z}");
                        dupes.Add(vertices[i], new List<int> { i });
                    }

                    verts[vertices[i]] = i;
                }
            }

            br_vector3 offset = new br_vector3(
                bounds.Max.X - (bounds.Max.X - bounds.Min.X) / 2,
                bounds.Max.Y - (bounds.Max.Y - bounds.Min.Y) / 2,
                bounds.Max.Z - (bounds.Max.Z - bounds.Min.Z) / 2
            );

            foreach (KeyValuePair<br_vector3, int> kvp in verts.OrderBy(v => v.Value))
            {
                br_vector3 vert = vertices[kvp.Value];

                br_vector3 clamped = new br_vector3(
                    1 - (vert.X - offset.X) / ((bounds.Max.X - bounds.Min.X) / 2),
                    1 - (vert.Y - offset.Y) / ((bounds.Max.Y - bounds.Min.Y) / 2),
                    1 - (vert.Z - offset.Z) / ((bounds.Max.Z - bounds.Min.Z) / 2)
                );

                if (!subExtents.Contains(vert))
                {
                    br_crushpoint point = new br_crushpoint
                    {
                        VertIndex = kvp.Value,
                        // used for right, top and back collisions
                        SoftnessNeg = new br_vector3(
                            (2 - clamped.X) * settings.OverallSoftness * (clamped.Z > 2 * settings.PositionOfMiddleOfSides.X && clamped.Z < 2 * settings.PositionOfMiddleOfSides.Y ? settings.RelativeSoftnessSidesMiddle : settings.RelativeSoftnessSidesEnd),
                            ((2 - clamped.Y) * settings.OverallSoftness * settings.RelativeSoftnessTop / 2) + ((2 - clamped.Y) * settings.RelativeSoftnessTop * settings.OverallSoftness * (2 - clamped.Y)),
                            (2 - clamped.Z) * settings.OverallSoftness * settings.RelativeSoftnessBack
                        ),
                        // used for left, bottom and front
                        SoftnessPos = new br_vector3(
                            clamped.X * settings.OverallSoftness * (clamped.Z > 2 * settings.PositionOfMiddleOfSides.X && clamped.Z < 2 * settings.PositionOfMiddleOfSides.Y ? settings.RelativeSoftnessSidesMiddle : settings.RelativeSoftnessSidesEnd),
                            (clamped.Y * settings.OverallSoftness * settings.RelativeSoftnessBottom / 2) + ((2 - clamped.Y) * settings.RelativeSoftnessBottom * settings.OverallSoftness * clamped.Y),
                            clamped.Z * settings.OverallSoftness * settings.RelativeSoftnessFront
                        )
                    };

                    point.LimitMin.X = vert.X - point.SoftnessNeg.X / settings.OverallSoftness * settings.Deformability;
                    point.LimitMin.Y = vert.Y - point.SoftnessNeg.Y / settings.OverallSoftness * settings.Deformability;
                    point.LimitMin.Z = vert.Z - point.SoftnessNeg.Z / settings.OverallSoftness * settings.Deformability;

                    point.LimitMax.X = vert.X + point.SoftnessPos.X / settings.OverallSoftness * settings.Deformability;
                    point.LimitMax.Y = vert.Y + point.SoftnessPos.Y / settings.OverallSoftness * settings.Deformability;
                    point.LimitMax.Z = vert.Z + point.SoftnessPos.Z / settings.OverallSoftness * settings.Deformability;

                    crush.Points.Add(point);

                    int lastVertIndex = -1;
                    int neighbour_index = lastVertIndex;

                    for (int j = 0; j < vertices.Count; j++)
                    {
                        if (j == kvp.Value) { continue; }

                        float dX = vertices[j].X - vert.X;
                        float dY = vertices[j].Y - vert.Y;
                        float dZ = vertices[j].Z - vert.Z;

                        float distance = (float)Math.Sqrt(dX * dX + dY * dY + dZ * dZ);

                        if (distance < settings.LocalitudeOfDamage)
                        {
                            neighbour_index = j - lastVertIndex;
                            lastVertIndex = j;

                            point.Neighbours.Add(new br_crushneighbour
                            {
                                VertIndex = neighbour_index,
                                Factor = (int)Math.Floor(distance / settings.LocalitudeOfDamage * 256)
                            });
                        }
                    }
                }
            }
        }

        public void CalculateExtents()
        {
            bounds.Min = new br_vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            bounds.Max = new br_vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < vertices.Count; i++)
            {
                if (bounds.Min.X > vertices[i].X) { bounds.Min.X = vertices[i].X; }
                if (bounds.Min.Y > vertices[i].Y) { bounds.Min.Y = vertices[i].Y; }
                if (bounds.Min.Z > vertices[i].Z) { bounds.Min.Z = vertices[i].Z; }

                if (bounds.Max.X < vertices[i].X) { bounds.Max.X = vertices[i].X; }
                if (bounds.Max.Y < vertices[i].Y) { bounds.Max.Y = vertices[i].Y; }
                if (bounds.Max.Z < vertices[i].Z) { bounds.Max.Z = vertices[i].Z; }
            }
        }
    }
}