namespace TRS.CaptureTool
{
    public static class ToolInfo
    {
        public static bool isScreenshotTool;
        public static bool isGifTool;

        static ToolInfo()
        {
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly assembly in assemblies)
            {
                if (assembly.GetName().Name != "Assembly-CSharp")
                    continue;

                System.Type[] types = assembly.GetTypes();
                foreach (System.Type type in types)
                {
                    if (type.ToString().Contains("TRS.CaptureTool.ScreenshotScript"))
                        isScreenshotTool = true;
                    if (type.ToString().Contains("TRS.CaptureTool.GifScript"))
                        isGifTool = true;
                }
            }
        }

        public static string ScreenshotVersion()
        {
            if (!isScreenshotTool) return "";
            return VersionForType("TRS.CaptureTool.ScreenshotScript");
        }

        public static string GifVersion()
        {
            if (!isGifTool) return "";
            return VersionForType("TRS.CaptureTool.GifScript");
        }

        static string VersionForType(string type)
        {
            System.Type versionClass = System.Reflection.Assembly.GetAssembly(typeof(CaptureScript)).GetType(type);
            if (versionClass == null) return "";
            System.Reflection.FieldInfo versionField = versionClass.GetField("version");
            if (versionField == null) return "";
            return versionField.GetValue(null) as string;
        }

        public static string ToolVersion()
        {
            float screenshotVersion = float.MaxValue;
            string screenshotVersionString = ScreenshotVersion();
            if (!string.IsNullOrEmpty(screenshotVersionString))
                screenshotVersion = (float)System.Convert.ToDouble(screenshotVersionString);

            float gifVersion = float.MaxValue;
            string gifVersionString = GifVersion();
            if (!string.IsNullOrEmpty(gifVersionString))
                gifVersion = (float)System.Convert.ToDouble(gifVersionString);

            float minVersion = UnityEngine.Mathf.Min(screenshotVersion, gifVersion);
            return minVersion.ToString("0.00");
        }
    }
}