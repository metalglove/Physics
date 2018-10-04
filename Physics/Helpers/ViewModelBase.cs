using Physics.Services;

namespace Physics.Helpers
{
    public abstract class ViewModelBase : BindableBase
    {
        protected static ServiceProvider serviceProvider = App.serviceProvider;
        protected static NavigationService navigationService = serviceProvider.GetService<NavigationService>();

        public string ViewTitle { get; set; }

        public ViewModelBase()
        {

        }
    }
}
