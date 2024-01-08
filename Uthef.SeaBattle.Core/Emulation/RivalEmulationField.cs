using Uthef.SeaBattle.Core.Exceptions;

namespace Uthef.SeaBattle.Core.Emulation
{
    public class RivalEmulationField : LocalField
    {
        private readonly List<Cell> PossibleCells = new();
        private LocalField _opponentField;
        private readonly List<PrioritizedTarget> PrioritizedTargets = new();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
        public RivalEmulationField(LocalField opponentField) : base(true)
        {
            SwitchOpponent(opponentField);
        }
#pragma warning restore CS8618

        public void SwitchOpponent(LocalField opponentField)
        {
            if (opponentField.Locked) throw new FieldLockedException();

            if (_opponentField is { })
            {
                _opponentField.Locked = false;
            }

            _opponentField = opponentField;
            _opponentField.Locked = true;

            PossibleCells.Clear();
            PrioritizedTargets.Clear();

            foreach (var cell in _opponentField.Cells)
            {
                PossibleCells.Add(cell);
            }
        }

        public Cell? ShootOpponent()
        {
            return PrioritizedTargets.Any() ? ShootTarget() : ShootRandomCell();
        }

        private Cell? ShootRandomCell()
        {
            if (PossibleCells.Count == 0) return null;

            var cell = GetRandomPossibleCell();
            _opponentField.Shoot(cell.X, cell.Y);

            if (cell.Ship is { })
            {
                if (cell.Ship.Range.Length > 1)
                {
                    var target = new PrioritizedTarget(cell);
                    PrioritizedTargets.Add(target);
                }
                else RemovePossibleRange(cell.Ship.CellsAround);
            }

            PossibleCells.Remove(cell);

            return cell;
        }

        private Cell? ShootTarget()
        {
            if (PrioritizedTargets.Count == 0) return null;

            var target = PrioritizedTargets.First();
            var dirCells = _opponentField.GetDirectionalCellsAround(target.LastCell.X, target.LastCell.Y);
            var possibleDirCells = dirCells.Where(x => PossibleCells.Contains(x)).ToList();

            if (target.InitialCell != target.LastCell)
            {
                bool vertical = target.InitialCell.X == target.LastCell.X;

                if (vertical)
                    possibleDirCells.RemoveAll(cell => cell.X != target.LastCell.X);
                else
                    possibleDirCells.RemoveAll(cell => cell.Y != target.LastCell.Y);

                if (possibleDirCells.Count == 0)
                {
                    int x = -1, y = -1;

                    if (vertical)
                    {
                        x = target.InitialCell.X;
                        y = target.InitialCell.Y + (target.InitialCell.Y < target.LastCell.Y ? -1 : 1);
                    }
                    else
                    {
                        x = target.InitialCell.X + (target.InitialCell.X < target.LastCell.X ? -1 : 1);
                        y = target.InitialCell.Y;
                    }

                    possibleDirCells.Add(_opponentField.At(x, y) ?? throw new ImpossibleException());
                }
            }

            var cell = GetNextCell(possibleDirCells) ?? throw new ImpossibleException();
            _opponentField.Shoot(cell.X, cell.Y);
            PossibleCells.Remove(cell);

            if (cell.Ship is { })
            {
                target.LastCell = cell;

                if (cell.Ship.Destroyed)
                {
                    PrioritizedTargets.Remove(target);
                    RemovePossibleRange(cell.Ship.CellsAround);
                }
            }

            return cell;
        }

        private void RemovePossibleRange(IEnumerable<Cell> cells)
        {
            foreach (var cell in cells)
            {
                PossibleCells.Remove(cell);
            }
        }

        private Cell? GetNextCell(List<Cell> directions)
        {
            if (directions.Count == 0) return null;
            var random = new Random();

            return directions[random.Next(directions.Count)];
        }

        private Cell GetRandomPossibleCell()
        {
            var random = new Random();

            if (PossibleCells.Count == 0) throw new ImpossibleException();

            return PossibleCells[random.Next(PossibleCells.Count)];
        }
    }
}
