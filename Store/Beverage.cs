namespace Store {
    public abstract class Beverage {
        public string Name { get; }
        public double Degrees { get; }
        public double Centiliters { get; }

        protected Beverage(double degrees, double centiliters, string name) {
            Name = name;
            Degrees = degrees;
            Centiliters = centiliters;
        }
    }
}