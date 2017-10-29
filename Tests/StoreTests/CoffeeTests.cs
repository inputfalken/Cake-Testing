using Store;
using Xunit;

namespace StoreTests {
    public class CoffeeTests {
        [Fact]
        public void Large_Coffee_Is_Expected_Volume() {
            var coffee = new Coffee(20, Size.Large, "Latte", "Arabica");
            Assert.Equal(500d, coffee.Centiliters);
        }

        [Fact]
        public void Medium_Coffee_Is_Expected_Volume() {
            var coffee = new Coffee(20, Size.Medium, "Latte", "Arabica");
            Assert.Equal(370d, coffee.Centiliters);
        }

        [Fact]
        public void Small_Coffee_Is_Expected_Volume() {
            var coffee = new Coffee(20, Size.Small, "Latte", "Arabica");
            Assert.Equal(200d, coffee.Centiliters);
        }
    }
}