using System;

namespace Physics.Models
{
    public struct Cannonball : IProjectile
    {
        public double Mass => 3; // kg
        public double Diameter => 0.3; // meters3
        public double DragCoefficient => 0.47; // for a smooth sphere the drag coefficient is 0.47 (Re = 1e5)
        public double Area => Math.PI * (Diameter * Diameter); // meters squared
        public double X => 0;
        public double Y => 0;
    }
}
