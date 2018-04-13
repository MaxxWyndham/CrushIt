namespace CrushIt
{
    public class br_crushneighbour
    {
        int vertIndex;
        int factor;

        public int VertIndex
        {
            get => vertIndex;
            set => vertIndex = value;
        }

        public int Factor
        {
            get => factor;
            set => factor = value;
        }

        public br_crushneighbour() { }

        public br_crushneighbour(DocumentParser file)
        {
            vertIndex = file.ReadInt();
            factor = file.ReadInt();
        }
    }
}