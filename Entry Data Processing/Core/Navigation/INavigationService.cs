using System;

namespace Entry_Data_Processing.Core.Navigation
{
    public interface INavigationService
    {
        void NavigateTo(Type pageType);
        void NavigateTo<T>() where T : class;
        void GoBack();
    }
}
