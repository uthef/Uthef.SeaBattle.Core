using Uthef.SeaBattle.Core.Exceptions;
using Uthef.SeaBattle.Core.Extensions;

namespace Uthef.SeaBattle.Core
{
    public class LocalField
    {
        public const int Width = 10;
        public const int Height = 10;

        private readonly Cell[,] _matrix = new Cell[Width, Height];
        private readonly List<Cell> _freeCells = new();
        private readonly List<Ship> _ships = new();

        public bool Locked { get; internal set; } = false;

        public int FreeCellsCount => _freeCells.Count;
        public int ShipsCount => _ships.Count;
        public bool AllShipsDestroyed => _ships.Where(ship => ship.Destroyed).Count() == _ships.Count;

        public IEnumerable<Cell> Cells
        {
            get
            {
                for (int i = 0; i < _matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < _matrix.GetLength(1); j++)
                    {
                        yield return _matrix[i, j];
                    }
                }
            }
        }

        public IEnumerable<Cell> FreeCells
        {
            get
            {
                foreach (var cell in _freeCells) yield return cell;
            }
        }

        public IEnumerable<Ship> Ships
        {
            get
            {
                foreach (var ship in _ships) yield return ship;
            }
        }

        public LocalField(bool fill = false)
        {
            for (int i = 0; i < _matrix.GetLength(0); i++)
            {
                for (int j = 0; j < _matrix.GetLength(1); j++)
                {
                    var cell = new Cell(j, i);
                    _matrix[i, j] = cell;
                    _freeCells.Add(cell);
                }
            }

            if (fill) this.Fill();
        }

        public Cell? At(int x, int y)
        {
            if (x < 0 || x >= _matrix.GetLength(1)) return null;
            if (y < 0 || y >= _matrix.GetLength(0)) return null;

            return _matrix[y, x];
        }

        private List<Cell> GetCellsAround8D(int x, int y, int skipX, int skipY)
        {
            var list = new List<Cell>();

            for (int i = -1; i <= 3; i += 2)
            {
                for (int j = -1; j <= 3; j += 2)
                {
                    var nextX = x + (i < 2 ? i : 0);
                    var nextY = y + (j < 2 ? j : 0);

                    if ((nextX == x && nextY == y) || (skipX == nextX && skipY == nextY)) continue;

                    var cell = At(nextX, nextY);

                    if (cell is { } nonNullCell)
                    {
                        list.Add(nonNullCell);
                    }
                }
            }

            return list;
        }

        public List<Cell> GetCellsAround(int x, int y) => GetCellsAround8D(x, y, -1, -1);
        public List<Cell> GetCellsAround(int x, int y, int skipX, int skipY) => GetCellsAround8D(x, y, skipX, skipY);

        public List<Cell> GetDirectionalCellsAround(int x, int y)
        {
            var list = new List<Cell>();

            for (int i = -1; i < 2; i += 2)
            {
                for (int j = -1; j < 2; j += 2)
                {
                    var nextX = x + (i < 0 ? j : 0);
                    var nextY = y + (i < 0 ? 0 : j);

                    var cell = At(nextX, nextY);

                    if (cell is { } nonNullCell)
                    {
                        list.Add(cell);
                    }
                }
            }

            return list;
        }

        public IEnumerable<Cell> GetCellsForDirection(Range range, bool throwWhenNull = true)
        {
            int step = range.StartAxis2 < range.EndAxis2 ? 1 : -1;

            for (int i = range.StartAxis2; range.EndAxis2 - i != -step; i += step)
            {
                var cell = At(range.IsVertical ? range.Axis1 : i, range.IsVertical ? i : range.Axis1);

                if (cell is null)
                {
                    if (throwWhenNull) throw new InvalidCoordinatesException();
                    else break;
                }

                yield return cell;
            }
        }

        public IEnumerable<Range> GetDirections(int x, int y, int maxLength)
        {
            if (maxLength <= 0) 
                throw new ArgumentException("Value must be greater than zero", nameof(maxLength));

            for (int i = 0; i < 4; i++)
            {
                var vertical = i < 2;
                var axis1 = vertical ? x : y;
                var padding = i % 2 == 0 ? 1 : -1;
                var startAxis2 = (vertical ? y : x) + padding;
                var endAxis2 = (vertical ? y : x) + maxLength * padding;

                var cells = GetCellsForDirection(new Range(axis1, startAxis2, endAxis2, vertical), false);
                var list = new List<Cell>();

                foreach (var cell in cells)
                {
                    if (cell.Type == CellType.Empty)
                    {
                        list.Add(cell);
                    }
                    else break;
                }

                if (list.Count > 0)
                {
                    var lastCell = list[^1];
                    endAxis2 = vertical ? lastCell.Y : lastCell.X;
                    yield return new Range(axis1, startAxis2, endAxis2, vertical);
                }
            }
        }

        public Ship AddShip(Range range)
        {
            if (Locked) throw new FieldLockedException();

            var ship = new Ship(this, range);
            _ships.Add(ship);
            return ship;
        }

        public bool RemoveShip(Ship ship)
        {
            if (Locked) throw new FieldLockedException();

            if (!_ships.Contains(ship)) return false;

            _ships.Remove(ship);
            ship.ClearCells();

            return true;
        }

        public void ClearShips()
        {
            for (int i = _ships.Count - 1; i >= 0; i--)
            {
                RemoveShip(_ships[i]);
            }
        }

        public void Reset()
        {
            ClearShips();

            foreach (var cell in Cells)
            {
                cell.Reset();
            }
        }

        internal void UpdateCellCollections(Cell cell, CellType previousType)
        {
            if (Locked) throw new FieldLockedException();

            switch (previousType)
            {
                case CellType.Empty:
                    _freeCells.Remove(cell);
                    break;
            }

            switch (cell.Type)
            {
                case CellType.Empty:
                    _freeCells.Add(cell);
                    break;
            }
        }

        public Cell? GetRandomFreeCell(Random? random = null)
        {
            if (FreeCellsCount == 0) return null;
            random ??= new Random();
            return _freeCells[random.Next(FreeCellsCount)];
        }

        public Cell? Shoot(int x, int y)
        {
            var cell = At(x, y);

            if (cell is { })
            {
                cell.Shoot();
            }

            return cell;
        }
    }
}
