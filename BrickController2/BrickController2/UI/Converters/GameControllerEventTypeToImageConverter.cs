using BrickController2.PlatformServices.GameController;
using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class GameControllerEventTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventType = (GameControllerEventType)value;
            return Convert(eventType);
        }

        public string Convert(GameControllerEventType eventType)
        {
            return eventType switch
            {
                GameControllerEventType.Button => "abc",
                GameControllerEventType.Axis => "gamepad",
                _ => null,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
