using Physics.Helpers;
using Physics.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Physics.Services
{
    public class ProjectileMotionService : IService
    {
        private const double gravity = 9.81; // 9.81 meters per second squared.

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
            angle = DegreesToRadians(angle);
            double totalTime = 0;
            double vxnew = velocity * Math.Cos(angle);
            double vynew = velocity * Math.Sin(angle);
            double xnew = 0;
            double ynew = initialHeight;
            double dt = 0.001;

            for (int i = 0; ynew > -0.0000000000001; i++)
            {
                double k = projectile.DragCoefficient * projectile.Area * AirDensityCalculator(ynew) / projectile.Mass / 8;
                // update current velocity to the previously calculated velocity
                velocity = Math.Sqrt(vxnew * vxnew + vynew * vynew);
                double ax = -k * velocity * vxnew;
                double ay = -k * velocity * vynew - gravity;
                // calculate new velocity
                vxnew = vxnew + ax * dt;
                vynew = vynew + ay * dt;
                vectors.Add(CalculateVectorWithDrag(vxnew, vynew, ref xnew, ref ynew, dt, i, ax, ay));
                totalTime += dt;
            }
            double distance = xnew;
            double impactAngle = CalculateImpactAngle(initialHeight, angle, vxnew, vynew);
            return new Trajectory(vectors, Math.Round(totalTime, 4), Math.Round(impactAngle, 4), Math.Round(distance, 4));
        }
        private static double CalculateImpactAngle(double initialHeight, double angle, double vxnew, double vynew)
        {
            double newVelocity = Math.Sqrt(Math.Pow(vxnew, 2) + Math.Pow(vynew, 2));
            double temp = Math.Sqrt((newVelocity * newVelocity) * (Math.Sin(angle) * Math.Sin(angle)) + 2 * gravity * initialHeight);
            double impactAngle = (180 * Math.Atan(temp / (newVelocity * Math.Cos(angle)))) / Math.PI;
            return impactAngle;
        }
        private static Vector CalculateVector(double velocity, double angle, double initialHeight, VectorCollection vectors, double currentTime)
        {
            double xVector = Math.Round(velocity * currentTime * Math.Cos(angle), 4);
            double yVector = Math.Round(initialHeight + (velocity * currentTime * Math.Sin(angle)) - 0.5 * gravity * (currentTime * currentTime), 4);
            return new Vector(xVector, yVector);
        }
        private static Vector CalculateVectorWithDrag(double vxnew, double vynew, ref double xnew, ref double ynew, double dt, int i, double ax, double ay)
        {
            xnew = xnew + vxnew * dt + ax * dt * dt;
            ynew = ynew + vynew * dt + ay * dt * dt;
            return new Vector(xnew, ynew);
        }
        private static double DegreesToRadians(double angle)
        {
            return (Math.PI * angle) / 180;
        }
        private static double AirDensityCalculator(double currentHeight)
        {
            // Max is 11000 meters on Earth
            double temperature = 15.04 - 0.00649 * currentHeight;
            double pressure = 101.29 * Math.Pow((temperature + 273.1) / 288.09, 5.256);
            double rho = pressure / (0.2869 * (temperature + 273.1));
            return rho;
        }
    }
}
