using BrickController2.PlatformServices.GameController;

namespace BrickController2.Tizen.PlatformServices.GameController
{
    public class GameControllerService : IGameControllerService
    {
        private readonly object _lockObject = new ();

        private event EventHandler<GameControllerEventArgs> GameControllerEventInternal;

        public event EventHandler<GameControllerEventArgs> GameControllerEvent
        {
            add
            {
                lock (_lockObject)
                {
                    GameControllerEventInternal += value;
                }
            }

            remove
            {
                lock (_lockObject)
                {
                    GameControllerEventInternal -= value;
                }
            }
        }
    }
}