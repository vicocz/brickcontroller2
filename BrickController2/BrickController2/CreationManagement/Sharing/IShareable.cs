using System;

namespace BrickController2.CreationManagement.Sharing;

public interface IShareable
{
    /// <summary>
    /// Defines the unique identifier of shareable model
    /// </summary>
    static virtual string Type => throw new InvalidOperationException();

    /// <summary>Name associated with the item</summary>
    string Name { get; }
}
