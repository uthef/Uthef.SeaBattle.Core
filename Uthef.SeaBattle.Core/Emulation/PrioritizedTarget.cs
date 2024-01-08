namespace Uthef.SeaBattle.Core.Emulation
{
    internal class PrioritizedTarget
    {
        internal Cell InitialCell { get; }
        internal Cell LastCell { get; set; }

        internal PrioritizedTarget(Cell initialCell)
        {
            InitialCell = initialCell;
            LastCell = initialCell;
        }
    }
}
