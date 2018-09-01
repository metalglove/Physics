using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Physics.Helpers
{
    public class ServiceProvider
    {
        private List<IService> _services = new List<IService>();

        public Service GetService<Service>() where Service : IService
        {
            if (!_services.Any(service => service.GetType() == typeof(Service)))
            {
                _services.Add(Activator.CreateInstance<Service>());
                Debug.WriteLine($"New Service registered: {typeof(Service).Name}");
            }
            return (Service)_services.Single(service => service.GetType() == typeof(Service));
        }
    }
}
