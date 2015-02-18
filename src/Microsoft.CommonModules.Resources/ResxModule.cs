using System.IO;
using System.Resources;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Framework.FileSystemGlobbing;
using Microsoft.Framework.Runtime.Roslyn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CommonModules.Resources
{
    public class ResxModule : ICompileModule
    {
        public static readonly string[] DefaultExcludePatterns = new[] { @"obj\**\*.*", @"bin\**\*.*" };

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

            var patterns = PatternsCollectionHelper.GetPatternsCollection(projectFile, context.ProjectContext.ProjectDirectory, "exclude", DefaultExcludePatterns);

            var matcher = new Matcher();
            matcher.AddInclude("**/*.resx")
                   .AddExcludePatterns(patterns);

            var resourceFiles = matcher.GetResultsInFullPath(context.ProjectContext.ProjectDirectory);

            foreach (var resourceFile in resourceFiles)
            {
                var resourceName = ResourceNameHelper.CreateResourceName(context.ProjectContext.ProjectDirectory, resourceFile);
                context.Resources.Add(new ResourceDescription(resourceName, resourceFile, () => new FileStream(resourceFile, FileMode.Open, FileAccess.Read, FileShare.Read), isPublic: true));
            }
        }

        private static Stream GetResourceStream(string resxFilePath)
        {
            using (var fs = File.OpenRead(resxFilePath))
            {
                var document = XDocument.Load(fs);

                var ms = new MemoryStream();
                var rw = new ResourceWriter(ms);

                foreach (var e in document.Root.Elements("data"))
                {
                    string name = e.Attribute("name").Value;
                    string value = e.Element("value").Value;

                    rw.AddResource(name, value);
                }

                rw.Generate();
                ms.Seek(0, SeekOrigin.Begin);

                return ms;
            }
        }
    }
}
