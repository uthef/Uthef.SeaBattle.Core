using Uthef.SeaBattle.Core.Exceptions;

namespace Uthef.SeaBattle.Core.Extensions
{
    public static class FieldFillExtension
    {
        public static void ResetAndFill(this LocalField field)
        {
            field.Reset();
            Fill(field);
        }

        internal static void Fill(this LocalField field)
        {
            var random = new Random();

            for (int deckSize = 4, shipAmount = 1; deckSize > 0; deckSize--, shipAmount++)
            {
                var shipsCreated = 0;

                do
                {
                    var randomCell = field.GetRandomFreeCell() ?? throw new OutOfFreeCellsException();
                    var directions = field.GetDirections(randomCell.X, randomCell.Y, deckSize)
                        .Where(x => x.Length >= deckSize)
                        .ToList();

                    if (directions.Count > 0)
                    {
                        var randDir = directions[random.Next(directions.Count)];
                        var ship = field.AddShip(randDir);
                        shipsCreated++;
                    }
                }
                while (shipsCreated < shipAmount);
            }
        }
    }
}