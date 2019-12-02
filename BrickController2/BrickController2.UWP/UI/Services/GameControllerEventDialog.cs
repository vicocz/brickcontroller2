using BrickController2.Windows.PlatformServices.GameController;
using Windows.UI.Xaml.Input;
using Xamarin.Forms.Platform.UWP;

namespace BrickController2.Windows.UI.Services
{
    public class GameControllerEventDialog : AlertDialog
    {
        private readonly GameControllerService _gameControllerService;

        public GameControllerEventDialog(GameControllerService gameControllerService) 
        {
            _gameControllerService = gameControllerService;
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (_gameControllerService.OnKeyDown(e))
            {
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (_gameControllerService.OnKeyUp(e))
            {
                e.Handled = true;
            }
            base.OnKeyUp(e);
        }
    }
}