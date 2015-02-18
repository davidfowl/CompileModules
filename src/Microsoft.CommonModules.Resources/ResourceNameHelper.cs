using System;

namespace Microsoft.CommonModules.Resources
{
    public static class ResourceNameHelper
    {
        public static string CreateResourceName(string baseDirectory, string resourcePath)
        {
            // TODO: Port the ugly code from CreateCSharpManifestResourceName in Microsoft.Build.Tasks.v14.dll
            return resourcePath;
        }
    }
}