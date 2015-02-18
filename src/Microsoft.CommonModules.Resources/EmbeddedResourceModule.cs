using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Framework.FileSystemGlobbing;
using Microsoft.Framework.Runtime.Roslyn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CommonModules.Resources
{
    public class EmbeddedResourceModule : ICompileModule
    {
        public static readonly string[] DefaultResourcesPatterns = new[] { @"compiler\resources\**\*" };


        public void AfterCompile(IAfterCompileContext context)
        {

        }

        public void BeforeCompile(IBeforeCompileContext context)
        {
            JObject projectFile = null;
            using (var fs = File.OpenRead(context.ProjectContext.ProjectFilePath))
            {
                projectFile = JObject.Load(new JsonTextReader(new StreamReader(fs)));
            }

            var patterns = PatternsCollectionHelper.GetPatternsCollection(projectFile, context.ProjectContext.ProjectDirectory, "resources", DefaultResourcesPatterns);

            var matcher = new Matcher();
            matcher.AddIncludePatterns(patterns);

            var resourceFiles = matcher.GetResultsInFullPath(context.ProjectContext.ProjectDirectory);

            foreach (var resourceFile in resourceFiles)
            {
                var resourceName = ResourceNameHelper.CreateResourceName(context.ProjectContext.ProjectDirectory, resourceFile);
                context.Resources.Add(new ResourceDescription(resourceName, resourceFile, () => new FileStream(resourceFile, FileMode.Open, FileAccess.Read, FileShare.Read), isPublic: true));
            }
        }
    }
}