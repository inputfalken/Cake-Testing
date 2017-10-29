using System;

namespace Store {
    public class Coffee : Beverage {
        public Size Size { get; }
        public string Beans { get; }

        private static double SetCentilitters(Size size) {
            switch (size) {
                case Size.Large:
                    return 500;
                case Size.Medium:
                    return 370;
                case Size.Small:
                    return 200;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }
        }

        public Coffee(double degrees, Size size, string name, string beans) :
            base(degrees, SetCentilitters(size), name) {
            Size = size;
            Beans = beans;
        }

        public override string ToString() {
            return $"{Size} ({Centiliters}cl) Coffee {Name}";
        }
    }
}