using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uthef.SeaBattle.Core
{
    public readonly struct Range
    {
        public bool IsVertical { get; }
        public int Axis1 { get; }
        public int StartAxis2 { get; }
        public int EndAxis2 { get; }
        public readonly int Length => Math.Abs(StartAxis2 - EndAxis2) + 1;

        [JsonConstructor]
        public Range(int axis1, int startAxis2, int endAxis2, bool isVertical = false)
        {
            Axis1 = axis1;
            StartAxis2 = startAxis2;
            EndAxis2 = endAxis2;
            IsVertical = isVertical;
        }

        public Range Normalize()
        {
            return StartAxis2 > EndAxis2 ? new Range(Axis1, EndAxis2, StartAxis2, IsVertical) : this;
        }
    }
}
