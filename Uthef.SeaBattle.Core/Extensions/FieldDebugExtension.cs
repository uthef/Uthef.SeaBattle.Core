using System.Text;

namespace Uthef.SeaBattle.Core.Extensions
{
    public static class FieldDebugExtension
    {
        public static string MatrixAsString(this LocalField field)
        {
            var sb = new StringBuilder();

            foreach (var cell in field.Cells)
            {
                if (cell.StartsNewRow)
                {
                    sb.AppendLine();
                }

                string? str = "+";

                if (!cell.Special)
                {
                    str = cell.Type switch
                    {
                        CellType.Empty => cell.Shot ? " " : "0",
                        CellType.EmptyNearShip => cell.Shot ? " " : "+",
                        CellType.Ship => cell.Shot ? "x" : cell.Ship?.Size.ToString(),
                        _ => throw new Exception("Invalid cell type")
                    };
                }

                sb.Append(str);
            }

            return sb.ToString();
        }
    }
}
