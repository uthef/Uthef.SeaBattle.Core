using Uthef.SeaBattle.Core.Exceptions;

namespace Uthef.SeaBattle.Core
{
    public class Ship
    {
        private List<Cell> CellList { get; } = new List<Cell>();
        private List<Cell> CellsAroundList { get; } = new List<Cell>();
        public bool Destroyed => CellList.Where(x => x.Shot).Count() == CellList.Count;
        public bool Integrity => !CellList.Where(x => x.Shot).Any();

        public Range Range { get; }

        public int Size => CellList.Count;
        public int CellsAroundCount => CellsAroundList.Count;

        public LocalField Field { get; }

        public IEnumerable<Cell> Cells
        {
            get
            {
                foreach (var cell in CellList)
                {
                    yield return cell;
                }
            }
        }

        public IEnumerable<Cell> CellsAround
        {
            get
            {
                foreach (var cell in CellsAroundList) yield return cell;
            }
        }

        internal Ship(LocalField field, Range range)
        {
            Field = field;
            Range = range;

            var cellsChangeList = new List<(Cell, CellType)>();

            int ignoreX = -1,
                ignoreY = -1;

            var directionCells = field.GetCellsForDirection(range);

            foreach (var cell in directionCells)
            {
                if (cell.Type is CellType.Ship)
                    throw new CellOccupiedException($"Cell {cell} is already owned by other ship");

                CellList.Add(cell);

                var cellsAround = field.GetCellsAround(cell.X, cell.Y, ignoreX, ignoreY);

                ignoreX = cell.X;
                ignoreY = cell.Y;

                foreach (var cellAround in cellsAround)
                {
                    if (cellAround.Type is CellType.Ship)
                        throw new CellOccupiedException($"Cell {cellAround} is not free!");

                    cellsChangeList.Add((cellAround, CellType.EmptyNearShip));
                    CellsAroundList.Add(cellAround);
                }

                cellsChangeList.Add((cell, CellType.Ship));
            }

            foreach (var cellState in cellsChangeList)
            {
                var cell = cellState.Item1;
                var newType = cellState.Item2;

                var prevType = cell.Type;

                cell.Type = newType;

                if (newType == CellType.Ship)
                {
                    cell.Ship = this;
                }
                else
                {
                    cell.RelatedShips.Add(this);
                }

                field.UpdateCellCollections(cell, prevType);
            }
        }

        internal void ClearCells()
        {
            foreach (var cell in CellList)
            {
                var prevType = cell.Type;
                cell.Ship = null;

                Field.UpdateCellCollections(cell, prevType);
            }

            foreach (var cell in CellsAroundList)
            {
                var prevType = cell.Type;
                cell.RelatedShips.Remove(this);

                if (cell.RelatedShips.Count == 0)
                {
                    cell.Type = CellType.Empty;
                    Field.UpdateCellCollections(cell, prevType);
                }
            }
        }
    }
}
