using Physics.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Physics.Services
{
    public class NavigationService : BindableBase, IService
    {
        private string _title;
        private ViewModelBase _currentViewModel;
        private ObservableCollection<ViewModelBase> _viewModels = new ObservableCollection<ViewModelBase>();

        public string Title
        {
            get => _title;
            private set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                Title = _currentViewModel.ViewTitle;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<ViewModelBase> ViewModels
        {
            get => _viewModels;
            private set
            {
                _viewModels = value;
                RaisePropertyChanged();
            }
        }

        public void NavigateTo<ViewModel>() where ViewModel : ViewModelBase
        {
            if (!_viewModels.Any(vm => vm.GetType() == typeof(ViewModel)))
            {
                _viewModels.Add(Activator.CreateInstance<ViewModel>());
                Debug.WriteLine(String.Format("New ViewModel registered: {0}", typeof(ViewModel).Name));
            }
            CurrentViewModel = (ViewModel)_viewModels.Single(vm => vm.GetType() == typeof(ViewModel));
            Debug.WriteLine(String.Format("Navigated to: {0}", typeof(ViewModel).Name));
        }
    }
}
