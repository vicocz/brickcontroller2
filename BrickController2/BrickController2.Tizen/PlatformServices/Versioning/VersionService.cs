using BrickController2.PlatformServices.Versioning;
using Application = Tizen.Applications.Application;

namespace BrickController2.Tizen.PlatformServices.Versioning;

public class VersionService : IVersionService
{
    public string ApplicationVersion => Application.Current.Version.ToString();
}