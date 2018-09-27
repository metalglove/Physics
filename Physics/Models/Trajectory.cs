using System;
using System.Windows;
using System.Windows.Media;

namespace Physics.Models
{
    public class Trajectory
    {
        public PointCollection Points { get; private set; }
        //public PointCollection FullTrajectoryPoints { get; private set; }
        public double AirTime { get; private set; }
        public double ImpactAngle { get; private set; }
        public double Distance { get; private set; }

        public Trajectory(PointCollection vectors, double airTime, double impactAngle, double distance)
        {
            Points = vectors;
            //FullTrajectoryPoints = vectors;
            AirTime = airTime;
            ImpactAngle = impactAngle;
            Distance = distance;
        }
        /*
        public void UpdatePoints(int length)
        {
            Point[] currentArray = new Point[Points.Count];
            Points.CopyTo(currentArray, 0);
            Point[] result = new Point[length];
            //Point x = currentArray[length + 195];
            Array.Copy(currentArray, 0, result, 0, length);
            Points = new PointCollection(result);
        }*/
    }
}
