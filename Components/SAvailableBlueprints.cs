using Unity.Entities;

namespace ThatsWhatINeed.Components
{
    [InternalBufferCapacity(12)]
    public struct SAvailableBlueprint : IBufferElementData
    {
        public static implicit operator int(SAvailableBlueprint e) { return e.AvailableBlueprint; }
        public static implicit operator SAvailableBlueprint(int e) { return new SAvailableBlueprint { AvailableBlueprint = e }; }
        public int AvailableBlueprint;
    }
}