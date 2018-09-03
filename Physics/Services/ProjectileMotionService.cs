using Physics.Helpers;
using Physics.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Physics.Services
{
    public class ProjectileMotionService : IService
    {
        private const double gravity = 9.81; // 9.81 meters per second squared.
        private const double dragCoefficient = 0.47; // for a smooth sphere the drag coefficient is 0.47 (Re = 1e5)

        /// <summary>
        /// Calculates a trajectory with the given parameters: velocity, angle & initial height.
        /// </summary>
        /// <param name="velocity">measured in meters per second.</param>
        /// <param name="angle">measured in degrees.</param>
        /// <param name="initialHeight">the height of the projectile being launched from.</param>
        /// <param name="trajectorySteps">the amount of steps a trajectory needs to be calculated for.</param>
        public Trajectory CalculateTrajectory(double velocity, double angle, double initialHeight = 0, double trajectorySteps = 1000)
        {
            VectorCollection vectors = new VectorCollection(); // vectors of trajectory
            double totalTime = 0; // total time in air in seconds
            double distance = 0; // total distance travelled in meters on X-axis
            angle = DegreesToRadians(angle); // degrees to radians

            double temp = Math.Sqrt((velocity * velocity) * (Math.Sin(angle) * Math.Sin(angle)) + 2 * gravity * initialHeight);

            if (initialHeight == 0) // even ground
            {
                totalTime = (velocity * 2 * Math.Sin(angle)) / gravity; // seconds in air
                distance = ((velocity * velocity) / gravity) * Math.Sin(angle * 2); // meters travelled over X-axis
            }
            else // uneven ground
            {
                totalTime = (velocity * Math.Sin(angle) + temp) / gravity; // seconds in air
                distance = (velocity * Math.Cos(angle) / gravity) * ((velocity * Math.Sin(angle)) + temp); // meters travelled over X-axis
            }
            double stepper = totalTime / trajectorySteps;
            for (double currentTime = 0; Math.Round(currentTime, 10) <= Math.Round(totalTime, 10); currentTime += stepper)
            {
                vectors.Add(CalculateVector(velocity, angle, initialHeight, vectors, currentTime));
            }
            double impactAngle = (180 * Math.Atan(temp / (velocity * Math.Cos(angle)))) / Math.PI;
            return new Trajectory(vectors, Math.Round(totalTime, 4), Math.Round(impactAngle, 4), Math.Round(distance, 4));
        }
        /// <summary>
        /// Calculates a trajectory for a projectile with the given parameters: velocity, angle & initial height.
        /// </summary>
        /// <param name="projectile">the projectile a trajectory needs to be calculated for.</param>
        /// <param name="velocity">measured in meters per second.</param>
        /// <param name="angle">measured in degrees.</param>
        /// <param name="initialHeight">the height of the projectile being launched from.</param>
        /// <param name="trajectorySteps">the amount of steps a trajectory needs to be calculated for.</param>
        public Trajectory CalculateTrajectoryWithDrag(IProjectile projectile, double velocity, double angle, double initialHeight = 0, double trajectorySteps = 1000)
        {
            VectorCollection vectors = new VectorCollection();
            vectors.Add(new Vector(0, initialHeight));
            double k = dragCoefficient * Math.PI * projectile.Area * AirDensityCalculator(initialHeight != 0 ? initialHeight : 0.00000001) / projectile.Mass / 16; // different equations for drag exist
            double height = initialHeight != 0 ? initialHeight : 0.00000001;
            angle = DegreesToRadians(angle);
            double vx = velocity * Math.Cos(angle);
            double vy = velocity * Math.Sin(angle);
            double dt = CalculateTotalTimeWithDrag(projectile, velocity, vy, height, 0.01) / trajectorySteps;
            double totalTime = 0;
            List<double> vxnew = new List<double> { vx };
            List<double> vynew = new List<double> { vy };
            List<double> xnew = new List<double> { 0 };
            List<double> ynew = new List<double> { height };

            for (int i = 0; i < trajectorySteps; i++)
            {
                double ax = -k * velocity * vxnew[i];
                double ay = -k * velocity * vynew[i] - gravity;
                CalculateNewVelocity(ref vxnew, ref vynew, dt, i, ax, ay);
                vectors.Add(CalculateVectorWithDrag(vxnew, vynew, ref xnew, ref ynew, dt, i, ax, ay));
                totalTime += dt;
            }
            double distance = xnew[xnew.Count - 1];
            double impactAngle = CalculateImpactAngle(initialHeight, angle, vxnew, vynew);
            return new Trajectory(vectors, Math.Round(totalTime, 4), Math.Round(impactAngle, 4), Math.Round(distance, 4));
        }
        private static double CalculateTotalTimeWithDrag(IProjectile projectile, double velocity, double vy, double height, double dt)
        {
            List<double> vynew = new List<double> { vy };
            List<double> ynew = new List<double> { height };
            int n = 0;
            double totalTime = 0;
            while (ynew[n] >= 0)
            {
                double k = dragCoefficient * Math.PI * projectile.Area * AirDensityCalculator(ynew[n]) / projectile.Mass / 16;
                double ay = -k * velocity * vynew[n] - gravity;
                vynew.Add(vynew[n] + ay * dt);
                ynew.Add(ynew[n] + vynew[n] * dt + ay * dt * dt);
                totalTime += dt;
                n++;
            }
            return totalTime;
        }
        private static double CalculateImpactAngle(double initialHeight, double angle, List<double> vxnew, List<double> vynew)
        {
            double newVelocity = Math.Sqrt(Math.Pow(vxnew[vxnew.Count - 1], 2) + Math.Pow(vynew[vynew.Count - 1], 2));
            double temp = Math.Sqrt((newVelocity * newVelocity) * (Math.Sin(angle) * Math.Sin(angle)) + 2 * gravity * initialHeight);
            double impactAngle = (180 * Math.Atan(temp / (newVelocity * Math.Cos(angle)))) / Math.PI;
            return impactAngle;
        }
        private static void CalculateNewVelocity(ref List<double> vxnew, ref List<double> vynew, double dt, int i, double ax, double ay)
        {
            vxnew.Add(vxnew[i] + ax * dt);
            vynew.Add(vynew[i] + ay * dt);
        }
        private static Vector CalculateVector(double velocity, double angle, double initialHeight, VectorCollection vectors, double currentTime)
        {
            double xVector = Math.Round(velocity * currentTime * Math.Cos(angle), 4);
            double yVector = Math.Round(initialHeight + (velocity * currentTime * Math.Sin(angle)) - 0.5 * gravity * (currentTime * currentTime), 4);
            return new Vector(xVector, yVector);
        }
        private static Vector CalculateVectorWithDrag(List<double> vxnew, List<double> vynew, ref List<double> xnew, ref List<double> ynew, double dt, int i, double ax, double ay)
        {
            xnew.Add(xnew[i] + vxnew[i] * dt + ax * dt * dt);
            ynew.Add(ynew[i] + vynew[i] * dt + ay * dt * dt);
            return new Vector(xnew[i + 1], ynew[i + 1]);
        }
        private static double DegreesToRadians(double angle)
        {
            return (Math.PI * angle) / 180;
        }
        private static double AirDensityCalculator(double currentHeight)
        {
            // Max is 11000 meters on Earth
            double temperature = 15.04 - 0.00649 * currentHeight;
            double pressure = 101.29 * Math.Pow((temperature + 273.1) / 288.08, 5.256);
            double rho = pressure / (0.2869 * (temperature = 273.1));
            return rho;
        }
    }
}
