using System;
using Wpf.Ui.Controls;

namespace Entry_Data_Processing.Core.Navigation
{
    public class NavigationService : INavigationService
    {
        private Wpf.Ui.INavigationService? _wpfUiNavigationService;

        public void SetNavigationControl(Wpf.Ui.INavigationService navigationService)
        {
            _wpfUiNavigationService = navigationService;
        }

        public void NavigateTo(Type pageType)
        {
            _wpfUiNavigationService?.Navigate(pageType);
        }

        public void NavigateTo<T>() where T : class
        {
            _wpfUiNavigationService?.Navigate(typeof(T));
        }

        public void GoBack()
        {
            _wpfUiNavigationService?.GoBack();
        }
    }
}
