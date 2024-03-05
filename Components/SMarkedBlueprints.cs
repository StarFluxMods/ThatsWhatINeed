using Unity.Entities;

namespace ThatsWhatINeed.Components
{
    [InternalBufferCapacity(12)]
    public struct SMarkedBlueprint : IBufferElementData
    {
        public static implicit operator int(SMarkedBlueprint e) { return e.MarkedBlueprint; }
        public static implicit operator SMarkedBlueprint(int e) { return new SMarkedBlueprint { MarkedBlueprint = e }; }
        public int MarkedBlueprint;
    }
}