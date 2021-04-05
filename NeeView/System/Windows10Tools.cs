namespace NeeView
{
    public static class Windows10Tools
    {
        public static bool IsWindows10 => System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0;

        public static bool IsWindows10_OrGreater => System.Environment.OSVersion.Version.Major >= 10;
    }

}
