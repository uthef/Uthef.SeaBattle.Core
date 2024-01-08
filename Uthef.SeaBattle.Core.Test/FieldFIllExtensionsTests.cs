using Uthef.SeaBattle.Core.Extensions;

namespace Uthef.SeaBattle.Core.Test
{
    public class FieldFIllExtensionsTests
    {
        [Test]
        [Timeout(2000)]
        public void ResetAndFill()
        {
            var field = new LocalField(); 
            field.ResetAndFill();

            Assert.That(field.ShipsCount, Is.EqualTo(10));
        }
    }
}
