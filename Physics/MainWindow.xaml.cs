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
            //NameScope.SetNameScope(this, new NameScope());

            //Trajectory trajectoryEven = Services.ProjectileMotionService.CalculateTrajectory(10, 45, 0, 50);
            //Trajectory trajectoryUnEven = Services.ProjectileMotionService.CalculateTrajectory(10, 45, 10, 50);
            //Trajectory trajectory = Services.ProjectileMotionService.CalculateTrajectoryWithDrag(new Cannonball(), 10, 45, 0, 50);
            //DisplayTrajectory(trajectoryEven, 10, 45, 0, 50);
            //DisplayTrajectory(trajectoryUnEven, 10, 45, 10, 50);

            //AnimateTrajectory(trajectoryEven, "Even", Brushes.Black);
            //AnimateTrajectory(trajectoryUnEven, "Uneven", Brushes.Green);

            //CreateAPath();
            //DrawLine();
        }

        private void AnimateTrajectory(Trajectory trajectory, string identifier, Brush brush)
        {
            EllipseGeometry animatedObjectGeometry = new EllipseGeometry(new Point(0, trajectory.Vectors[0].Y * 10), 5, 5);
            RegisterName("AnimatedObjectGeometry"+ identifier, animatedObjectGeometry);

            Path objectPath = new Path
            {
                Data = animatedObjectGeometry,
                Fill = brush
            };
            //cvField.Children.Add(objectPath);

            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure
            {
                StartPoint = new Point(0, trajectory.Vectors[0].Y * 10)
            };
            PolyBezierSegment pBezierSegment = new PolyBezierSegment();
            foreach (Vector item in trajectory.Vectors)
            {
                pBezierSegment.Points.Add(new Point(item.X * 10, item.Y * 10));
            }
            pFigure.Segments.Add(pBezierSegment);
            animationPath.Figures.Add(pFigure);

            DrawLine(animationPath, brush);
            animationPath.Freeze();

            PointAnimationUsingPath centerPointAnimation = new PointAnimationUsingPath
            {
                PathGeometry = animationPath,
                Duration = TimeSpan.FromSeconds(trajectory.AirTime)
            };

            Storyboard.SetTargetName(centerPointAnimation, "AnimatedObjectGeometry" + identifier);
            Storyboard.SetTargetProperty(centerPointAnimation, new PropertyPath(EllipseGeometry.CenterProperty));

            Storyboard pathAnimationStoryboard = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            pathAnimationStoryboard.Children.Add(centerPointAnimation);

            objectPath.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                pathAnimationStoryboard.Begin(this);
            };
        }
        private static void DisplayTrajectory(Trajectory trajectory, double velocity, double angle, double initialHeight = 0, double trajectorySteps = 50)
        {
            Debug.WriteLine("-- Calculating trajectory --");
            Debug.WriteLine($"-- Velocity: {velocity} m/s, Angle: {angle} degrees, Initial height: {initialHeight} meters, Vector steps: {trajectorySteps} --");
            Debug.WriteLine($"Distance travelled: {trajectory.Distance}m, Total time spent in air: {trajectory.AirTime}s, Angle of impact: {trajectory.ImpactAngle} degrees.");
            double x = 0;
            foreach (Vector item in trajectory.Vectors)
            {
                Debug.WriteLine($"{x}: X: {item.X}, Y: {item.Y}, Current time in air: {x++ * (trajectory.AirTime / trajectorySteps)}s");
            }
        }
        private void DrawLine(PathGeometry pathGeometry, Brush brush)
        {
            Path arcPath = new Path();
            arcPath.Stroke = brush;
            arcPath.StrokeThickness = 1;
            arcPath.Data = pathGeometry;
            //cvField.Children.Add(arcPath);
        }
    }
}
