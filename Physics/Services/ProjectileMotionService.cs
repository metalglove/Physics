using Physics.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Physics.Services
{
    public static class ProjectileMotionService
    {
        private const double gravity = 9.81; // 9.81 meters per second squared.

        /// <summary>
        /// Calculates a trajectory for a projectile with the given parameters: velocity & angle.
        /// </summary>
        /// <param name="velocity">measured in meters per second.</param>
        /// <param name="angle">measured in degrees.</param>
        /// <param name="projectile">the projectile to calculate a trajectory for.</param>
        /// <param name="initialHeight">the height of the projectile being launched from.</param>
        /// <param name="trajectorySteps">the amount of steps a trajectory needs to be calculated for.</param>
        public static Trajectory CalculateTrajectory(IProjectile projectile, double velocity, double angle, double initialHeight = 0, double trajectorySteps = 50)
        {
            VectorCollection vectors = new VectorCollection(); // vectors of trajectory
            double totalTime = 0; // total time in air in seconds
            double distance = 0; // total distance travelled in meters on X-axis
            angle = (Math.PI * angle) / 180; // degrees to radians

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
                vectors.Add(CalculateVectorFor(velocity, angle, initialHeight, vectors, currentTime));
            }
            double impactAngle = (180 * Math.Atan(temp / (velocity * Math.Cos(angle)))) / Math.PI;
            return new Trajectory(vectors, Math.Round(totalTime, 4), Math.Round(impactAngle, 4), Math.Round(distance, 4));
        }
        private static Vector CalculateVectorFor(double velocity, double angle, double initialHeight, VectorCollection vectors, double currentTime)
        {
            double xVector = Math.Round(velocity * currentTime * Math.Cos(angle), 4);
            double yVector = Math.Round(initialHeight + (velocity * currentTime * Math.Sin(angle)) - 0.5 * gravity * (currentTime * currentTime), 4);
            return new Vector(xVector, yVector);
        }
    }
}
