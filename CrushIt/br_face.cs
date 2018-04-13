namespace CrushIt
{
    public class br_face
    {
        int[] vertices = new int[3];
        int material;
        int smoothing;

        public int SmoothingGroup
        {
            get => smoothing;
            set => smoothing = value;
        }

        public int Material
        {
            get => material;
            set => material = value;
        }

        public br_face(int a, int b, int c)
        {
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }
    }
}