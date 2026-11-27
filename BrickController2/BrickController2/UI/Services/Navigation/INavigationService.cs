using BrickController2.UI.ViewModels;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Navigation
{
    public interface INavigationService
    {
        NavigationState State { get; }
        Task NavigateToMoreAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase;
        void RegisterRootViewModel(PageViewModelBase viewModel);
        T? GetRootViewModel<T>() where T : PageViewModelBase;
        Task NavigateToAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase;
        Task NavigateToModalAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase;
        Task NavigateBackAsync();
        Task NavigateModalBackAsync();
    }
}
