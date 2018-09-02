using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Physics.Helpers;
using Physics.Models;
using Physics.Services;

namespace Physics.ViewModels
{
    public class ProjectileLauncherViewModel : ViewModelBase
    {
        #region Fields
        private double _mass, _diameter, _velocity, _angle, _initialHeight, _trajectorySteps;
        private ObservableCollection<Shape> _projectiles;
        #endregion Fields

        #region Properties
        public double Mass
        {
            get => _mass;
            set
            {
                _mass = value;
                RaisePropertyChanged();
            }
        }
        public double Diameter
        {
            get => _diameter;
            set
            {
                _diameter = value;
                RaisePropertyChanged();
            }
        }
        public double Velocity
        {
            get => _velocity;
            set
            {
                _velocity = value;
                RaisePropertyChanged();
            }
        }
        public double Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                RaisePropertyChanged();
            }
        }
        public double InitialHeight
        {
            get => _initialHeight;
            set
            {
                _initialHeight = value;
                RaisePropertyChanged();
            }
        }
        public double TrajectorySteps
        {
            get => _trajectorySteps;
            set
            {
                _trajectorySteps = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<Shape> Projectiles
        {
            get => _projectiles ?? (_projectiles = new ObservableCollection<Shape>());
            set
            {
                _projectiles = value;
                RaisePropertyChanged();
            }
        }
        #endregion Properties

        #region Commands
        public DelegateCommand CalculateTrajectoryCommand { get; private set; }
        #endregion Commands

        public ProjectileLauncherViewModel()
        {
            //NameScope.SetNameScope(this, new NameScope());
            CalculateTrajectoryCommand = new DelegateCommand(DoCalculateTrajectoryCommand, CanDoCalculateTrajectoryCommand);
        }

        #region Methods
        private void AnimateTrajectory(Trajectory trajectory, string identifier, Brush brush)
        {
            EllipseGeometry animatedObjectGeometry = new EllipseGeometry(new Point(0, trajectory.Vectors[0].Y * 10), 5, 5);
            //RegisterName("AnimatedObjectGeometry" + identifier, animatedObjectGeometry);

            Path objectPath = new Path
            {
                Data = animatedObjectGeometry,
                Fill = brush
            };
            Projectiles.Add(objectPath);

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
                pathAnimationStoryboard.Begin();// this
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
            Projectiles.Add(arcPath);
        }
        #endregion Methods

        #region Command Methods
        private void DoCalculateTrajectoryCommand()
        {
            Trajectory trajectory = serviceProvider.GetService<ProjectileMotionService>().CalculateTrajectory(Velocity, Angle, InitialHeight, TrajectorySteps);
            AnimateTrajectory(trajectory, "name", Brushes.Black);
        }

        private bool CanDoCalculateTrajectoryCommand()
        {
            return true;
        }
        #endregion Command Methods
    }
}
