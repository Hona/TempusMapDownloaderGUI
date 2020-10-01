namespace TempusToMomentumMapRenamer.Models
{
    public class MapData
    {
        public bool ToCopy { get; set; } = true;
        public string Name { get; set; }
        public ClassInfo IntendedClass { get; set; }
    }
}