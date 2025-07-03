namespace BrickController2.PlatformServices.GameController;

public readonly record struct GameControllerEventKey
{
    public readonly GameControllerEventType EventType;
    public readonly string EventCode;
    public readonly string? EventAlias;

    public GameControllerEventKey(GameControllerEventType eventType, string eventCode, string? eventAlias = null)
    {
        EventType = eventType;
        EventCode = eventCode;
        EventAlias = eventAlias;
    }
}
