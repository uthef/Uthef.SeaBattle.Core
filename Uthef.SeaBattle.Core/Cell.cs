namespace Uthef.SeaBattle.Core
{
    public class Cell
    {
        public int X { get; }
        public int Y { get; }
        public CellType Type { get; internal set; } = CellType.Empty;
        public bool Shot { get; private set; } = false;
        public bool StartsNewRow => X == 0 && Y != 0;

        internal readonly List<Ship> RelatedShips = new();
        internal bool Special { get; set; }

        private Ship? _ship = null;
        public Ship? Ship
        {
            get => _ship;
            internal set
            {
                if (value is { })
                {
                    Type = CellType.Ship;
                }
                else
                {
                    Type = RelatedShips.Count > 0 ? CellType.EmptyNearShip : CellType.Empty;
                }

                _ship = value;
            }
        }

        internal Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        internal void Shoot() => Shot = true;
        internal void Reset() => Special = Shot = false;

        public override string ToString() => $"{{{X}, {Y}}}";
    }
}
