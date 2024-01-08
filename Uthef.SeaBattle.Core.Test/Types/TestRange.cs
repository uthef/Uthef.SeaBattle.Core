namespace Uthef.SeaBattle.Core.Test.Types
{
    public record TestRange(Range Range, bool MustThrow)
    {
        public static IEnumerable<TestRange> Values
        {
            get
            {
                yield return new(new Range(0, 1, 1, false), false);
                yield return new(new Range(5, 5, 7, false), false);
                yield return new(new Range(-1, -1, 1, false), true);
            }
        }
    }
}