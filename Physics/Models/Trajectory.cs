using System.Windows.Media;

namespace Physics.Models
{
    public class Trajectory
    {
        public VectorCollection Vectors { get; private set; }
        public double AirTime { get; private set; }
        public double ImpactAngle { get; private set; }
        public double Distance { get; private set; }

        public Trajectory(VectorCollection vectors, double airTime, double impactAngle, double distance)
        {
            Vectors = vectors;
            AirTime = airTime;
            ImpactAngle = impactAngle;
            Distance = distance;
        }
    }
}
