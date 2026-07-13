using System;

namespace BrickController2.UI.Services.AppIdentifier
{
    public interface IAppIdentifierService
    {
        ReadOnlyMemory<byte> GetAppId(int length);
    }
}
