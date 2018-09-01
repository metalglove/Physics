using System;
using Physics.Helpers;
using Physics.Models;
using Physics.Services;

namespace Physics.ViewModels
{
    public class ProjectileLauncherViewModel : ViewModelBase
    {
        #region Fields
        private double _mass, _diameter, _velocity, _angle, _initialHeight, _trajectorySteps;
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
                _initialHeight = value;
                RaisePropertyChanged();
            }
        }
        #endregion Properties

        #region Commands
        public DelegateCommand CalculateTrajectoryCommand { get; private set; }
        #endregion Commands

        public ProjectileLauncherViewModel()
        {
            CalculateTrajectoryCommand = new DelegateCommand(DoCalculateTrajectoryCommand, CanDoCalculateTrajectoryCommand);
        }

        #region Command Methods
        private void DoCalculateTrajectoryCommand()
        {
            Trajectory trajectory = serviceProvider.GetService<ProjectileMotionService>().CalculateTrajectory(Velocity, Angle, InitialHeight, TrajectorySteps);
        }

        private bool CanDoCalculateTrajectoryCommand()
        {
            throw new NotImplementedException();
        }
        #endregion Command Methods
    }
}
