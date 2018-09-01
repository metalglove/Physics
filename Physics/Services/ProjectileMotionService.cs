using Physics.Helpers;
using Physics.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Physics.Services
{
    public class ProjectileMotionService : IService
    {
        private const double gravity = 9.81; // 9.81 meters per second squared.
        private const double dragCoefficient = 0.47; // for a smooth sphere the drag coefficient is 0.47 (Re = 1e5)
        private const double airDensity = 1.2051; // rho (kg/m3) for an environment where temperature is 20C, Air pressure is 1018 (hPa) and Dew point is 7.5C. 

        /// <summary>
        /// Calculates a trajectory for a projectile with the given parameters: velocity, angle & initial height.
        /// </summary>
        /// <param name="velocity">measured in meters per second.</param>
        /// <param name="angle">measured in degrees.</param>
        /// <param name="initialHeight">the height of the projectile being launched from.</param>
        /// <param name="trajectorySteps">the amount of steps a trajectory needs to be calculated for.</param>
        public Trajectory CalculateTrajectory(double velocity, double angle, double initialHeight = 0, double trajectorySteps = 50)
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
                vectors.Add(CalculateVectorFor(velocity, angle, initialHeight, vectors, currentTime));
            }
            double impactAngle = (180 * Math.Atan(temp / (velocity * Math.Cos(angle)))) / Math.PI;
            return new Trajectory(vectors, Math.Round(totalTime, 4), Math.Round(impactAngle, 4), Math.Round(distance, 4));
        }
        public Trajectory CalculateTrajectoryWithDrag(IProjectile projectile, double velocity, double angle, double initialHeight = 0, double trajectorySteps = 50)
        {
            double angleRadians = DegreesToRadians(angle);
            double maxVelocity = Math.Sqrt((2 * projectile.Mass * gravity) / (dragCoefficient * projectile.Area * airDensity));
            double maxVelocity2 = (maxVelocity * maxVelocity);
            double timeAtMaxHeight = (maxVelocity / gravity) * Math.Atan(velocity / maxVelocity);
            double actualYVector = velocity * Math.Sin(angleRadians);

            for (double currentTime = 0; actualYVector > 0; currentTime += 0.1)
            {
                // Y component of velocity
                double vy = velocity * Math.Sin(angleRadians) - 0.5 * gravity * currentTime;
                double vy2 = vy * vy;


                double preActualVelocity = Math.Tan(currentTime * gravity / maxVelocity);
                double actualVelocity = currentTime >= timeAtMaxHeight ? maxVelocity : maxVelocity * (vy - maxVelocity * preActualVelocity) / (maxVelocity + vy * preActualVelocity);
                actualYVector = (0.5 * maxVelocity2 / gravity) * Math.Log((vy2 + maxVelocity2 / (actualVelocity * actualVelocity) + maxVelocity2));

                // X component of velocity
                double vx = velocity * Math.Cos(angleRadians);
                double actualXVector = (maxVelocity2 / gravity) * Math.Log((maxVelocity2 + gravity * vx * currentTime / maxVelocity2));

                Debug.WriteLine($"Current time: {currentTime}, ActualYVector: {actualYVector}, ActualXVector: {actualXVector}, ActualVelocity: {actualVelocity}");
            }       


            return new Trajectory(new VectorCollection(), new double(), new double(), new double());
        }
        public Trajectory CalculateTrajectoryWithDragV2(IProjectile projectile, double velocity, double angle, double initialHeight = 0, double trajectorySteps = 50)
        {
            double dragX = 0;
            double dragY = 0;
            double velocityX = 0; 
            double velocityY = 0;

            double G = -projectile.Mass * gravity;
            double D = 0.5 * airDensity * (velocity * velocity) * dragCoefficient * projectile.Area;
            velocity = Math.Sqrt((velocityX * velocityX) + (velocityY * velocityY));
            dragX = -D * velocityX / velocity;
            dragY = -D * velocityY / velocity;

            return new Trajectory(new VectorCollection(), new double(), new double(), new double());
        }
        private static Vector CalculateVectorFor(double velocity, double angle, double initialHeight, VectorCollection vectors, double currentTime)
        {
            double xVector = Math.Round(velocity * currentTime * Math.Cos(angle), 4);
            double yVector = Math.Round(initialHeight + (velocity * currentTime * Math.Sin(angle)) - 0.5 * gravity * (currentTime * currentTime), 4);
            return new Vector(xVector, yVector);
        }
        private static double DegreesToRadians(double angle)
        {
            return (Math.PI * angle) / 180;
        }
    }
}
