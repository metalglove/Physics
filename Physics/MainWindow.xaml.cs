using Physics.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Physics
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            #region Code
            //Trajectory trajectoryEven = Services.ProjectileMotionService.CalculateTrajectory(new Cannonball(), 10, 45, 0, 50);
            //DisplayTrajectory(trajectoryEven, 50);

            Trajectory trajectoryUnEven = Services.ProjectileMotionService.CalculateTrajectory(new Cannonball(), 10, 45, 10, 50);
            Debug.WriteLine("-- Calculating trajectory for a Cannonball --");
            Debug.WriteLine($"-- Velocity: 10 m/s, Angle: 45 degrees, Initial height: 10 meters, Vector steps: 50 --");
            DisplayTrajectory(trajectoryUnEven, 50);
            #endregion Code

            NameScope.SetNameScope(this, new NameScope());

            // Create the EllipseGeometry to animate.
            EllipseGeometry animatedCannonBallGeometry = new EllipseGeometry(new Point(0, 100), 15, 15);

            // Register the EllipseGeometry's name with
            // the page so that it can be targeted by a
            // storyboard.
            this.RegisterName("AnimatedCannonBallGeometry", animatedCannonBallGeometry);

            // Create a Path element to display the geometry.
            Path cannonBallPath = new Path
            {
                Data = animatedCannonBallGeometry,
                Fill = Brushes.Black,
                Margin = new Thickness(15)
            };

            // Create a Canvas to contain ellipsePath
            // and add it to the page.
            cvField.Children.Add(cannonBallPath);

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();
            pFigure.StartPoint = new Point(0, 100);
            PolyBezierSegment pBezierSegment = new PolyBezierSegment();
            foreach (Vector item in trajectoryUnEven.Vectors)
            {
                pBezierSegment.Points.Add(new Point(item.X * 10, item.Y * 10));
            }
            pFigure.Segments.Add(pBezierSegment);
            animationPath.Figures.Add(pFigure);

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a PointAnimationgUsingPath to move
            // the EllipseGeometry along the animation path.
            PointAnimationUsingPath centerPointAnimation = new PointAnimationUsingPath
            {
                PathGeometry = animationPath,
                Duration = TimeSpan.FromSeconds(trajectoryUnEven.AirTime)
                //RepeatBehavior = RepeatBehavior.Forever
            };

            // Set the animation to target the Center property
            // of the EllipseGeometry named "AnimatedEllipseGeometry".
            Storyboard.SetTargetName(centerPointAnimation, "AnimatedCannonBallGeometry");
            Storyboard.SetTargetProperty(centerPointAnimation, new PropertyPath(EllipseGeometry.CenterProperty));

            // Create a Storyboard to contain and apply the animation.
            Storyboard pathAnimationStoryboard = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever
                //AutoReverse = true
            };
            pathAnimationStoryboard.Children.Add(centerPointAnimation);

            // Start the Storyboard when ellipsePath is loaded.
            cannonBallPath.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                // Start the storyboard.
                pathAnimationStoryboard.Begin(this);
            };
        }

        private static void DisplayTrajectory(Trajectory trajectoryEven, double trajectorySteps = 50)
        {
            Debug.WriteLine($"Distance travelled: {trajectoryEven.Distance}m, Total time spent in air: {trajectoryEven.AirTime}s, Angle of impact: {trajectoryEven.ImpactAngle} degrees.");
            double x = 0;
            foreach (Vector item in trajectoryEven.Vectors)
            {
                Debug.WriteLine($"X: {item.X}, Y: {item.Y}, Current time in air: {x++ * (trajectoryEven.AirTime / trajectorySteps)}s");
            }
        }
    }
}
