using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
        private string _name, _xTitle, _yTitle;
        private double _mass, _diameter, _dragCoefficient, _velocity, _angle, _initialHeight, _trajectorySteps;
        private Canvas _canvas;
        private ObservableCollection<string> _calculations;
        #endregion Fields

        #region Properties
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }
        public string XTitle
        {
            get => _xTitle;
            set
            {
                _xTitle = value;
                RaisePropertyChanged();
            }
        }
        public string YTitle
        {
            get => _yTitle;
            set
            {
                _yTitle = value;
                RaisePropertyChanged();
            }
        }
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
        public double DragCoefficient
        {
            get => _dragCoefficient;
            set
            {
                _dragCoefficient = value;
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
        public Canvas Canvas
        {
            get => _canvas ?? (_canvas = new Canvas()
            {
                Background = Brushes.Wheat,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform() { ScaleY = -1 }
            });
            set
            {
                _canvas = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<string> Calculations
        {
            get => _calculations ?? (_calculations = new ObservableCollection<string>());
            set
            {
                _calculations = value;
                RaisePropertyChanged();
            }
        }
        #endregion Properties

        #region Commands
        public DelegateCommand CalculateTrajectoryCommand { get; private set; }
        #endregion Commands

        public ProjectileLauncherViewModel()
        {
            NameScope.SetNameScope(Canvas, new NameScope());
            CalculateTrajectoryCommand = new DelegateCommand(DoCalculateTrajectoryCommand, CanDoCalculateTrajectoryCommand);
            SetDefaults();
            DrawXYAxisNumbers();
        }

        #region Methods
        private void SetDefaults()
        {
            Mass = 0.8;
            Diameter = 0.3;
            DragCoefficient = 0.47;
            Velocity = 30;
            Angle = 42;
            InitialHeight = 0;
            TrajectorySteps = 1000;
            XTitle = "X - Range in meters";
            YTitle = "Y - Height in meters";
        }
        private void AnimateTrajectory(Trajectory trajectory, string identifier, Brush brush)
        {
            double multiplier = 8; // based on grid columns & rows. 40 each
            EllipseGeometry animatedObjectGeometry = new EllipseGeometry(new Point(0, trajectory.Vectors[0].Y * multiplier), 5, 5);
            string name = "AnimatedObjectGeometryBall" + identifier;
            Canvas.RegisterName(name, animatedObjectGeometry);

            Path objectPath = new Path
            {
                Data = animatedObjectGeometry,
                Fill = brush
            };
            Canvas.Children.Add(objectPath);

            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure
            {
                StartPoint = new Point(0, trajectory.Vectors[0].Y * multiplier)
            };
            PolyBezierSegment pBezierSegment = new PolyBezierSegment();
            foreach (Vector item in trajectory.Vectors)
            {
                pBezierSegment.Points.Add(new Point(item.X * multiplier, item.Y * multiplier));
            }

            DrawResultLabel(trajectory, multiplier, pBezierSegment);

            pFigure.Segments.Add(pBezierSegment);
            animationPath.Figures.Add(pFigure);

            DrawLine(animationPath, brush);
            animationPath.Freeze();

            PointAnimationUsingPath centerPointAnimation = new PointAnimationUsingPath
            {
                PathGeometry = animationPath,
                Duration = TimeSpan.FromSeconds(trajectory.AirTime)
            };

            Storyboard.SetTargetName(centerPointAnimation, name);
            Storyboard.SetTargetProperty(centerPointAnimation, new PropertyPath(EllipseGeometry.CenterProperty));

            Storyboard pathAnimationStoryboard = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            pathAnimationStoryboard.Children.Add(centerPointAnimation);

            objectPath.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                pathAnimationStoryboard.Begin(Canvas);
            };
        }
        private void DrawResultLabel(Trajectory trajectory, double multiplier, PolyBezierSegment pBezierSegment)
        {
            Point endX = pBezierSegment.Points[pBezierSegment.Points.Count - 1];
            Label textBlock = new Label()
            {
                Content = $"X: {endX.X / multiplier}m, Airtime: {trajectory.AirTime}s",
                RenderTransform = new ScaleTransform() { ScaleY = -1 }
            };
            Canvas.SetLeft(textBlock, endX.X / 2);
            Canvas.SetTop(textBlock, -30);
            //Canvas.Children.Add(textBlock);
            Calculations.Add($"X: {Math.Round(endX.X / multiplier, 4)}m, Airtime: {trajectory.AirTime}s");
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
            Canvas.Children.Add(arcPath);
        }
        private void DrawXYAxisNumbers()
        {
            for (double i = 0; i <= 100; i += 10)
            {
                Label label = new Label
                {
                    Content = i,
                    RenderTransform = new ScaleTransform() { ScaleY = -1 }
                };
                Canvas.SetLeft(label, -30);
                Canvas.SetTop(label, (i * 8) + 12.5);
                Canvas.Children.Add(label);
                Label label2 = new Label
                {
                    Content = i,
                    RenderTransform = new ScaleTransform() { ScaleY = -1 }
                };
                Canvas.SetLeft(label2, (i * 8) - 12.5);
                Canvas.SetTop(label2, -10);
                Canvas.Children.Add(label2);
            }
        }
        private static Brush PickBrush()
        {
            Brush result = Brushes.Transparent;
            Random rnd = new Random();
            Type brushesType = typeof(Brushes);
            PropertyInfo[] properties = brushesType.GetProperties();
            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);
            return result;
        }
        #endregion Methods

        #region Command Methods
        private void DoCalculateTrajectoryCommand()
        {
            Trajectory trajectoryNoDrag = serviceProvider.GetService<ProjectileMotionService>().CalculateTrajectory(Velocity, Angle, InitialHeight, TrajectorySteps);
            Trajectory trajectoryDrag = serviceProvider.GetService<ProjectileMotionService>().CalculateTrajectoryWithDrag(new CustomProjectile(Mass, Diameter, InitialHeight, DragCoefficient), Velocity, Angle, InitialHeight, TrajectorySteps);
            
            //DisplayTrajectory(trajectoryNoDrag, Velocity, Angle, InitialHeight, TrajectorySteps);
            AnimateTrajectory(trajectoryNoDrag, Name + "nodrag", PickBrush());

            //DisplayTrajectory(trajectoryDrag, Velocity, Angle, InitialHeight, TrajectorySteps);
            AnimateTrajectory(trajectoryDrag, Name + "drag", PickBrush());
        }
        private bool CanDoCalculateTrajectoryCommand()
        {
            return true;
        }
        #endregion Command Methods
    }
}
