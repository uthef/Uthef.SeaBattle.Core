using Uthef.SeaBattle.Core.Emulation;
using Uthef.SeaBattle.Core.Exceptions;

namespace Uthef.SeaBattle.Core.Test
{
    public class RivalEmulationFieldTests
    {
        [Test]
        public void DefeatRandomField()
        {
            var localField = new LocalField(true);
            var rivalField = new RivalEmulationField(localField);

            int shots = 0;
            Cell? cell;

            do
            {
                cell = rivalField.ShootOpponent();
                shots++;
            }
            while (cell is { });

            Assert.Multiple(() =>
            {
                Assert.That(shots, Is.LessThan(LocalField.Width * LocalField.Height));
                Assert.That(localField.AllShipsDestroyed, Is.True);
            });
        }

        [Test]
        public void MakeSureOpponentFieldsAreLocked()
        {
            var field = new LocalField(true);
            var rival = new RivalEmulationField(field);

            Assert.Multiple(() =>
            {
                Assert.That(field.Locked, Is.True);

                Assert.Throws<FieldLockedException>(() => field.ClearShips());
                Assert.Throws<FieldLockedException>(() => field.AddShip(new Range(0, 0, 0, false)));
                Assert.Throws<FieldLockedException>(() => field.RemoveShip(field.Ships.First()));
                Assert.Throws<FieldLockedException>(() => new RivalEmulationField(field));

                rival.SwitchOpponent(new LocalField());
                field.RemoveShip(field.Ships.First());

                Assert.That(field.Locked, Is.False);
            });
        }
    }
}
