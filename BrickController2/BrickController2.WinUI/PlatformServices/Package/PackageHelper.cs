namespace BrickController2.Windows.PlatformServices.Package
{
    public static class PackageHelper
    {
        public static bool IsPackaged { get; } = CheckPackaged();

        private static bool CheckPackaged()
        {
            try
            {
                _ = global::Windows.ApplicationModel.Package.Current.Id; // this will throw an exception if the app is not packaged
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
