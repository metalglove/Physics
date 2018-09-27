using Physics.Helpers;
using Physics.Models;
using System;
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
            PointCollection vectors = new PointCollection(); // vectors of trajectory
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
                vectors.Add(CalculateVector(velocity, angle, initialHeight, currentTime));
            }
            double impactAngle = CalculateImpactAngle(velocity, angle, temp);
            return new Trajectory(vectors, Math.Round(totalTime, 4), Math.Round(impactAngle, 4), Math.Round(distance, 4));
        }
        public Trajectory CalculateTrajectoryV2(double velocity, double angle, double initialHeight = 0)
        {
            PointCollection vectors = new PointCollection(); // vectors of trajectory
            double totalTime = 0; // total time in air in seconds
            double distance = 0; // total distance travelled in meters on X-axis
            angle = DegreesToRadians(angle); // degrees to radians
            double xVelocity, yVelocity, ax, ay, x = 0, y = 0;
            xVelocity = velocity * Math.Cos(angle);
            yVelocity = velocity * Math.Sin(angle);
            double dt = 0.001;

            for (int i = 0; y > -0.0000000000001; i++)
            {
                ax = 0;
                ay = -gravity;
                xVelocity = xVelocity + ax * dt;
                yVelocity = yVelocity + ay * dt;
                x = x + xVelocity * dt + ax * dt * dt;
                y = y + yVelocity * dt + ay * dt * dt;
                vectors.Add(CalculateVector(velocity, angle, y, totalTime));
                totalTime += dt;
            }
            double temp = Math.Sqrt((velocity * velocity) * (Math.Sin(angle) * Math.Sin(angle)) + 2 * gravity * initialHeight);

            double impactAngle = CalculateImpactAngle(velocity, angle, temp);
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
        public Trajectory CalculateTrajectoryWithDrag(IProjectile projectile, double velocity, double angle, double initialHeight = 0)
        {
            PointCollection vectors = new PointCollection();
            vectors.Add(new Point(0, initialHeight)); // adds the first position
            angle = DegreesToRadians(angle);
            double totalTime = 0;
            double xVelocity = velocity * Math.Cos(angle);
            double yVelocity = velocity * Math.Sin(angle);
            double x = 0;
            double y = initialHeight;
            double timeStep = 0.001;

            for (int i = 0; y > -0.000_000_000_0001; i++)
            {
                double k = projectile.DragCoefficient * projectile.Area * AirDensityCalculator(y) / projectile.Mass / 8;
                // update current velocity to the previously calculated velocity
                velocity = CalculateNewVelocity(xVelocity, yVelocity);
                double ax = -k * velocity * xVelocity;
                double ay = -k * velocity * yVelocity - gravity;
                // calculate new velocity
                xVelocity = xVelocity + ax * timeStep;
                yVelocity = yVelocity + ay * timeStep;
                vectors.Add(CalculateVectorWithDrag(xVelocity, yVelocity, ref x, ref y, timeStep, i, ax, ay));
                totalTime += timeStep;
            }
            double impactAngle = CalculateImpactAngleWithDrag(initialHeight, angle, xVelocity, yVelocity);
            return new Trajectory(vectors, Math.Round(totalTime, 4), Math.Round(impactAngle, 4), Math.Round(x, 4));
        }
        
        private static Point CalculateVector(double velocity, double angle, double initialHeight, double currentTime)
        {
            double xVector = velocity * currentTime * Math.Cos(angle);
            double yVector = initialHeight + (velocity * currentTime * Math.Sin(angle)) - 0.5 * gravity * (currentTime * currentTime);
            return new Point(xVector, yVector);
        }
        private static Point CalculateVectorWithDrag(double xVelocity, double yVelocity, ref double x, ref double y, double dt, int i, double ax, double ay)
        {
            x = x + xVelocity * dt + ax * dt * dt;
            y = y + yVelocity * dt + ay * dt * dt;
            return new Point(x, y);
        }
        private static double CalculateImpactAngle(double velocity, double angle, double temp)
        {
            return (180 * Math.Atan(temp / (velocity * Math.Cos(angle)))) / Math.PI;
        }
        private static double CalculateImpactAngleWithDrag(double initialHeight, double angle, double xVelocity, double yVelocity)
        {
            double velocity = CalculateNewVelocity(xVelocity, yVelocity);
            double temp = Math.Sqrt((velocity * velocity) * (Math.Sin(angle) * Math.Sin(angle)) + 2 * gravity * initialHeight);
            double impactAngle = CalculateImpactAngle(velocity, angle, temp);
            return impactAngle;
        }
        private static double CalculateNewVelocity(double xVelocity, double yVelocity)
        {
            return Math.Sqrt(xVelocity * xVelocity + yVelocity * yVelocity);
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
