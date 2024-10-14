namespace BrickController2.CreationManagement.Sharing;

public interface IShareable
{
    /// <summary>
    /// Defines the unique identifier of shareable model
    /// </summary>
    static abstract string Type { get; }
}
