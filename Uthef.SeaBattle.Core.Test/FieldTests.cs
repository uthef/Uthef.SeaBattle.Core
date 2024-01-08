using System.Text.Json;
using Uthef.SeaBattle.Core.Emulation;
using Uthef.SeaBattle.Core.Exceptions;
using Uthef.SeaBattle.Core.Extensions;
using Uthef.SeaBattle.Core.Test.Types;

namespace Uthef.SeaBattle.Core.Test
{
    public partial class FieldTests
    {
        public LocalField Field { get; private set; }
        public const int CellAmount = LocalField.Width * LocalField.Height;

        [SetUp]
        public void Setup()
        {
            Field = new LocalField();
        }

        [Test]
        public void At()
        {
            var random = new Random();

            Assert.Multiple(() =>
            {
                var cell = Field.At(random.Next(LocalField.Width), random.Next(LocalField.Height));
                Assert.That(cell, Is.Not.Null);

                Assert.That(Field.At(-1, -1), Is.Null);
                Assert.That(Field.At(LocalField.Width, LocalField.Height), Is.Null);
            });
        }

        [Test]
        public void CellEnumeration()
        {
            int x = 0;
            int y = 0;

            foreach (var cell in Field.Cells)
            {
                if (x == LocalField.Width)
                {
                    x = 0;
                    y++;
                }

                Assert.Multiple(() =>
                {
                    Assert.That(cell.X, Is.EqualTo(x++));
                    Assert.That(cell.Y, Is.EqualTo(y));
                });
            }
        }

        [Test]
        [TestCaseSource(typeof(TestRange), nameof(TestRange.Values))]
        public void ShipPlacing(TestRange testRange)
        {
            if (testRange.MustThrow)
            {
                Assert.Multiple(() =>
                {
                    Assert.Throws<InvalidCoordinatesException>(() => Field.AddShip(testRange.Range));
                    Assert.That(Field.ShipsCount, Is.EqualTo(0));
                    Assert.That(Field.FreeCellsCount, Is.EqualTo(CellAmount));
                });

                return;
            }

            Field.AddShip(testRange.Range);

            Assert.Multiple(() =>
            {
                Assert.That(Field.ShipsCount, Is.EqualTo(1));
                Assert.That(Field.Ships.First().Range, Is.EqualTo(testRange.Range));
            });
        }

        [Test]
        public void MultipleShips()
        {
            var ship = Field.AddShip(new Range(0, 1, 1, false));

            Assert.Multiple(() =>
            {
                Assert.Throws<CellOccupiedException>(() => Field.AddShip(new Range(0, 2, 2, false)));
                Assert.That(Field.FreeCellsCount, Is.EqualTo(CellAmount - ship.Range.Length - ship.CellsAroundCount));
            });
        }

        [Test]
        public void ShipRemoval()
        {
            var ship = Field.AddShip(new Range(0, 1, 1, false));
            Field.RemoveShip(ship);

            Assert.Multiple(() =>
            {
                Assert.That(Field.FreeCellsCount, Is.EqualTo(CellAmount));
                Assert.That(Field.ShipsCount, Is.EqualTo(0));

                var takenCells = ship.Cells
                    .Concat(ship.CellsAround)
                    .Where(x => x.Type is CellType.Ship or CellType.EmptyNearShip);

                Assert.That(takenCells, Is.Empty);
            });
        }
    }
}