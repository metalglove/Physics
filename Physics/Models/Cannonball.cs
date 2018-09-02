using System;

namespace Physics.Models
{
    public struct Cannonball : IProjectile
    {
        public double Mass => 3; // kg
        public double Diameter => 0.3; // meters3
        public double Area => (Math.PI * (Diameter * Diameter)) * 0.25; // meters squared
        public double X => 0;
        public double Y => 0;
    }
}
