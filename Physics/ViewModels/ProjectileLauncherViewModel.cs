using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Physics.Helpers;
using Physics.Models;
using Physics.Services;

namespace Physics.ViewModels
{
    public class ProjectileLauncherViewModel : ViewModelBase
    {
        #region Fields
        private const string PROJECT_DIRECTORY_PATH = "pack://application:,,,/Physics;component";
        private const int CANVAS_SIZE = 800;
        private const int PROJECTILE_SIZE = 10;
        private string _xTitle, _yTitle, _ipAddress = "192.168.137";
        private int _counter = 0;
        private double _mass, _diameter, _dragCoefficient, _velocity, _angle, _initialHeight, _trajectorySteps;
        private Canvas _canvas;
        private ObservableCollection<string> _calculations;
        private Rectangle _castle;
        private ProjectileMotionService _projectileMotionService;
        private EV3TcpService _EV3TcpService;
        private List<string> names = new List<string>();
        private bool _isReady;
        private bool _isBusy;
        private bool _canReset;
        private Random _randomBuild = new Random();
        #endregion Fields

        #region Properties
        public int Counter
        {
            get => _counter;
            set
            {
                _counter = value;
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
                //LaunchProjectileCommand.RaiseCanExecuteChanged();
                ReadyProjectileCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged();
            }
        }
        public double Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                // LaunchProjectileCommand.RaiseCanExecuteChanged();
                ReadyProjectileCommand.RaiseCanExecuteChanged();
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
                RenderTransform = FlippedTransform
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
        public Rect CastleRect
        {
            get
            {
                if (_castle.Equals(null))
                {
                    throw new ArgumentException("Castle not set");
                }
                return new Rect()
                {
                    Location = new Point(Canvas.GetLeft(Castle), Canvas.GetTop(Castle)),
                    Size = new Size(Castle.Width, Castle.Height)
                };
            }
        }
        public Rectangle Castle
        {
            get => _castle;
            set
            {
                _castle = value;
                RaisePropertyChanged();
            }
        }
        public bool IsConnected => _EV3TcpService.IsConnected;
        public bool IsNotConnected => !_EV3TcpService.IsConnected;
        public string ErrorMessage => _EV3TcpService.ErrorMessage;
        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value ?? (_ipAddress = "");
                RaisePropertyChanged();
                ConnectToEV3Command.RaiseCanExecuteChanged();
            }
        }
        private static ScaleTransform FlippedTransform => new ScaleTransform() { ScaleY = -1 };
        public Trajectory CurrentTrajectory { get; private set; }
        public bool IsReady
        {
            get => _isReady;
            private set
            {
                _isReady = value;
                RaisePropertyChanged();
                LaunchProjectileCommand.RaiseCanExecuteChanged();
                ReadyProjectileCommand.RaiseCanExecuteChanged();
            }
        }
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                RaisePropertyChanged();
                LaunchProjectileCommand.RaiseCanExecuteChanged();
                ReadyProjectileCommand.RaiseCanExecuteChanged();
            }
        }
        public bool CanReset
        {
            get => _canReset;
            set
            {
                _canReset = value;
                RaisePropertyChanged();
                ResetCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion Properties

        #region Commands
        public DelegateCommand ReadyProjectileCommand { get; private set; }
        public DelegateCommand LaunchProjectileCommand { get; private set; }
        public DelegateCommand ConnectToEV3Command { get; private set; }
        public DelegateCommand DisconnectCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        #endregion Commands

        public ProjectileLauncherViewModel()
        {

        }
        public ProjectileLauncherViewModel(ProjectileMotionService projectileMotionService, EV3TcpService _EV3TcpService)
        {
            NameScope.SetNameScope(Canvas, new NameScope());
            _projectileMotionService = projectileMotionService;
            this._EV3TcpService = _EV3TcpService;
            this._EV3TcpService.MessagesChanged += VerifyCommandMessage;
            ConnectToEV3Command = new DelegateCommand(DoConnectToEV3Command, CanDoConnectToEV3Command);
            LaunchProjectileCommand = new DelegateCommand(DoLaunchProjectileCommand, CanDoLaunchProjectileCommand);
            ReadyProjectileCommand = new DelegateCommand(DoReadyProjectileCommand, CanDoReadyProjectileCommand);
            DisconnectCommand = new DelegateCommand(DoDisconnectCommand, CanDoDisconnectCommand);
            ResetCommand = new DelegateCommand(DoResetCommand, CanDoResetCommand);
            SetDefaults();
            DrawXYAxisNumbers();
        }


        #region Methods
        private void Reset()
        {
            SetDefaults();
            foreach (string name in names)
            {
                Canvas.UnregisterName(name);
            }
            names.Clear();
            CanReset = false;
        }
        private void SetDefaults()
        {
            IsBusy = false;
            IsReady = false;
            Mass = 0.8;
            Diameter = 0.3;
            DragCoefficient = 0.47;
            Velocity = 30;
            Angle = 10;
            InitialHeight = 0;
            TrajectorySteps = 1000;
            XTitle = "X - Range in centimeters"; //X - Range in meters
            YTitle = "Y - Height in centimeters"; // Y - Height in meters
            BuildCastle();
            Canvas.Children.Clear();
        }
        private void BuildCastle()
        {
            Castle = new Rectangle
            {
                RenderTransform = FlippedTransform,
                Width = 100,
                Height = 100,
                Fill = new ImageBrush(new BitmapImage(new Uri(PROJECT_DIRECTORY_PATH + "/Images/castle.png", UriKind.Absolute)))
            };
            int location = _randomBuild.Next(500, 1460);
            Canvas.SetLeft(Castle, location - 100);
            Canvas.SetTop(Castle, Castle.Height);
            Canvas.Children.Add(Castle);
        }
        private void AnimateTrajectory(Trajectory trajectory, string identifier, Brush brush)
        {
            Point pointOfImpact = trajectory.Points.FirstOrDefault(ContainedPoint);
            double multiplier = 8; // based on grid columns & rows. 40 each
            EllipseGeometry animatedObjectGeometry = new EllipseGeometry(new Point(0, trajectory.Points[0].Y * multiplier), PROJECTILE_SIZE, PROJECTILE_SIZE);

            string name = "AnimatedObjectGeometryBall" + identifier;
            names.Add(name);
            Canvas.RegisterName(name, animatedObjectGeometry);

            Path objectPath = new Path
            {
                Data = animatedObjectGeometry,
                Fill = new ImageBrush(new BitmapImage(new Uri(PROJECT_DIRECTORY_PATH + "/Images/Projectile.png", UriKind.Absolute)))
            };

            Canvas.Children.Add(objectPath);

            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure
            {
                StartPoint = new Point(0, trajectory.Points[0].Y * multiplier)
            };
            PolyBezierSegment pBezierSegment = new PolyBezierSegment();
            foreach (Point item in trajectory.Points)
            {
                Point x = new Point(item.X * multiplier, item.Y * multiplier);
                if (x.X > pointOfImpact.X * 10 && !pointOfImpact.Equals(default(Point)))
                {
                    break;
                }
                pBezierSegment.Points.Add(x);
            }

            DrawResultLabel(trajectory.AirTime, multiplier, pBezierSegment);

            pFigure.Segments.Add(pBezierSegment);
            animationPath.Figures.Add(pFigure);

            Path arcPath = DrawLine(animationPath, brush);
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
                //RepeatBehavior = RepeatBehavior.Forever
            };
            pathAnimationStoryboard.Children.Add(centerPointAnimation);
            objectPath.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                pathAnimationStoryboard.Completed += delegate (object s, EventArgs ee)
                {
                    PathAnimationStoryboard_Completed(s, ee, objectPath, arcPath, trajectory);
                };
                pathAnimationStoryboard.Begin(Canvas);
                new SoundPlayer(Properties.Resources.launch).Play();
            };
        }
        private void PathAnimationStoryboard_Completed(object sender, EventArgs e, Path objectPath, Path arcPath, Trajectory trajectory)
        {
            Canvas.Children.Remove(objectPath);
            Canvas.Children.Remove(arcPath);
            CalculateImpactOnCastle(trajectory);
        }
        private bool ContainedPoint(Point point)
        {
            return CastleRect.Contains(new Point(point.X * 10, point.Y * 10));
        }
        private void CalculateImpactOnCastle(Trajectory trajectory)
        {
            if (trajectory.Points.Any(ContainedPoint))
            {
                Point pointOfImpact = trajectory.Points.First(ContainedPoint);
                Canvas.Children.Remove(Castle);
                new SoundPlayer(Properties.Resources.impact).Play();
            }
        }
        private void DrawResultLabel(double airTime, double multiplier, PolyBezierSegment pBezierSegment)
        {
            Point endX = pBezierSegment.Points[pBezierSegment.Points.Count - 1];
            Label textBlock = new Label()
            {
                Content = $"X: {endX.X / multiplier}m, Airtime: {airTime}s",
                RenderTransform = FlippedTransform
            };
            Canvas.SetLeft(textBlock, endX.X / 2);
            Canvas.SetTop(textBlock, -30);
            Calculations.Add($"X: {Math.Round(endX.X / multiplier, 4)}m, Airtime: {airTime}s");
        }
        private static void DisplayTrajectory(Trajectory trajectory, double velocity, double angle, double initialHeight = 0, double trajectorySteps = 50)
        {
            Debug.WriteLine("-- Calculating trajectory --");
            Debug.WriteLine($"-- Velocity: {velocity} m/s, Angle: {angle} degrees, Initial height: {initialHeight} meters, Vector steps: {trajectorySteps} --");
            Debug.WriteLine($"Distance travelled: {trajectory.Distance}m, Total time spent in air: {trajectory.AirTime}s, Angle of impact: {trajectory.ImpactAngle} degrees.");
            double x = 0;
            foreach (Point item in trajectory.Points)
            {
                Debug.WriteLine($"{x}: X: {item.X}, Y: {item.Y}, Current time in air: {x++ * (trajectory.AirTime / trajectorySteps)}s");
            }
        }
        private Path DrawLine(PathGeometry pathGeometry, Brush brush)
        {
            Path arcPath = new Path
            {
                Stroke = brush,
                StrokeThickness = 1,
                //StrokeDashArray = new DoubleCollection { 10 },
                Data = pathGeometry
            };
            Canvas.Children.Add(arcPath);
            return arcPath;
        }
        private void DrawXYAxisNumbers()
        {
            for (double i = 0; i <= 180; i += 10)
            {
                if (i <= 100)
                {
                    Label ylabel = new Label
                    {
                        Content = i,
                        RenderTransform = FlippedTransform
                    };
                    Canvas.SetLeft(ylabel, -30);
                    Canvas.SetTop(ylabel, (i * 8) + 12.5);
                    Canvas.Children.Add(ylabel);
                }
                Label xlabel = new Label
                {
                    Content = i,
                    RenderTransform = FlippedTransform
                };
                Canvas.SetLeft(xlabel, (i * 8) - 12.5);
                Canvas.SetTop(xlabel, -10);
                Canvas.Children.Add(xlabel);
            }
        }
        private void VerifyCommandMessage(object sender, string e)
        {
            switch (e)
            {
                case "Ready":
                    Application.Current.Dispatcher.Invoke(() => { IsReady = true; IsBusy = false; });
                    break;
                case "Done":
                    Application.Current.Dispatcher.Invoke(() => CanReset = true);
                    break;
                default:
                    break;
            }
        }
        #endregion Methods

        #region Command Methods

        private void DoConnectToEV3Command()
        {
            _EV3TcpService.Connect(IpAddress);
            RaisePropertyChanged("ErrorMessage");
            RaisePropertyChanged("IsConnected");
            RaisePropertyChanged("IsNotConnected");
            new Thread(() => _EV3TcpService.StartReceivingMessages()).Start(); // TODO: check
            DisconnectCommand.RaiseCanExecuteChanged();
        }
        private bool CanDoConnectToEV3Command()
        {
            if (string.IsNullOrWhiteSpace(IpAddress))
                return false;
            return IpAddress.Split('.').Length == 4 && IPAddress.TryParse(IpAddress, out IPAddress iPAddress);
        }
        private void DoLaunchProjectileCommand()
        {
            _EV3TcpService.SendMessage("GO");
            AnimateTrajectory(CurrentTrajectory, Counter + "drag", Brushes.Red);
            IsBusy = true;
        }
        private bool CanDoLaunchProjectileCommand()
        {
            return Velocity >= 30 && Velocity <= 70 && Angle >= 1 && Angle <= 35 && IsConnected && IsReady && !IsBusy;
        }
        private void DoReadyProjectileCommand()
        {
            //Trajectory trajectoryNoDrag = _projectileMotionService.CalculateTrajectory(Velocity, Angle, InitialHeight);
            CurrentTrajectory = _projectileMotionService.CalculateTrajectoryWithDrag(new CustomProjectile(Mass, Diameter, InitialHeight, DragCoefficient), Velocity, Angle, InitialHeight);

            //AnimateTrajectory(trajectoryNoDrag, Counter + "nodrag", Brushes.Black);
            _EV3TcpService.SendMessage($"Ready {Angle} {Velocity}");

            Counter++;
        }
        private bool CanDoReadyProjectileCommand()
        {
            return Velocity >= 30 && Velocity <= 70 && Angle >= 1 && Angle <= 35 && IsConnected && !IsBusy && !IsReady;
        }
        private void DoResetCommand()
        {
            Reset();
        }
        private bool CanDoResetCommand()
        {
            return CanReset;
        }
        private void DoDisconnectCommand()
        {
            _EV3TcpService.Disconnect();
            RaisePropertyChanged("IsConnected");
            RaisePropertyChanged("IsNotConnected");
        }
        private bool CanDoDisconnectCommand()
        {
            return IsConnected;
        }
        #endregion Command Methods
    }
}
