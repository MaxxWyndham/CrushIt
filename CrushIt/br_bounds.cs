namespace CrushIt
{
    public class br_bounds
    {
        br_vector3 min = br_vector3.Zero;
        br_vector3 max = br_vector3.Zero;

        public br_vector3 Min
        {
            get => min;
            set => min = value;
        }

        public br_vector3 Max
        {
            get => max;
            set => max = value;
        }

        public float Width => max.X - min.X;
        public float Height => max.Y - min.Y;
        public float Length => max.Z - min.Z;

        public bool Contains(br_vector3 point)
        {
            return
                min.X < point.X && point.X < max.X &&
                min.Y < point.Y && point.Y < max.Y &&
                min.Z < point.Z && point.Z < max.Z;
        }
    }
}