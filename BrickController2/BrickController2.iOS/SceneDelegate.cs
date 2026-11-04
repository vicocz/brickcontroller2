using Foundation;
using Microsoft.Maui;

namespace BrickController2.iOS
{
    /// <summary>
    /// Scene delegate required by the iOS 27+ SDK.
    /// Starting with this SDK, apps must adopt the UIScene-based life cycle,
    /// otherwise UIKit refuses to launch them ("UIScene life cycle is required").
    ///
    /// MauiUISceneDelegate already implements everything needed:
    /// it creates the UIWindow for each scene and hosts the MAUI window in it.
    /// We only need a concrete, Objective-C-registered subclass so that
    /// Info.plist can reference it by name.
    /// </summary>
    // The register name must exactly match "UISceneDelegateClassName" in Info.plist.
    [Register("SceneDelegate")]
    public class SceneDelegate : MauiUISceneDelegate
    {
        // No custom code needed - all scene handling is done by the MAUI base class.
    }
}
