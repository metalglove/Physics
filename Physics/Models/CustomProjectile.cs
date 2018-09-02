using System;

namespace Physics.Models
{
    public class CustomProjectile : IProjectile
    {
        public double Mass { get; private set; }
        public double Diameter { get; private set; }
        public double Area => (Math.PI * (Diameter * Diameter)) * 0.25;
        public double X { get; private set; }
        public double Y { get; private set; }

        public CustomProjectile(double mass, double diameter, double y)
        {
            Mass = mass;
            Diameter = diameter;
            Y = y;
            X = 0;
        }
    }
}
