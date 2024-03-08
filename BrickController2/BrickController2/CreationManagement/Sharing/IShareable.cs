namespace BrickController2.CreationManagement.Sharing;

public interface IShareable
{
    /// <summary>
    /// Unique sharing ID
    /// </summary>
    static abstract string Type { get; }
}
