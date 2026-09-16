using System;
using System.Windows;
using Wpf.Ui.Abstractions;

namespace Entry_Data_Processing.Core.Navigation
{
    public class PageService : INavigationViewPageProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public PageService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T? GetPage<T>() where T : class
        {
            return _serviceProvider.GetService(typeof(T)) as T;
        }

        public object? GetPage(Type pageType)
        {
            return _serviceProvider.GetService(pageType);
        }
    }
}
