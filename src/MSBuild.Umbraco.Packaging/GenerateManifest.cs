using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Umbraco.Core.Models.Packaging;

namespace MSBuild.Umbraco.Packaging
{
    public class GenerateManifest : Task
    {
        [Required]
        public string TargetDirectory { get; set; }

        #region Package info

        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public string UmbracoVersion { get; set; }

        public string Author { get; set; }

        public string AuthorUrl { get; set; } = "https://our.umbraco.com/";

        public string ProjectUrl { get; set; }

        public string IconUrl { get; set; }

        public string LicenseUrl { get; set; }

        #endregion

        public override bool Execute()
        {
            Debugger.Launch();

            Log.LogMessage(MessageImportance.High, "Creating Umbraco package");

            try
            {
                var definition = GetPackageDefinition();

                var definitionHelper = new PackageDefinitionHelper();

                var packageXml = definitionHelper.ExportPackage(definition);

                packageXml.Save(TargetDirectory + "package.xml");
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to create Umbraco package");

                Log.LogErrorFromException(ex);

                return false;
            }

            Log.LogMessage(MessageImportance.High, "Successfully created package");

            return true;
        }

        private PackageDefinition GetPackageDefinition()
        {
            var definition = new PackageDefinition
            {
                Name = Name,
                Readme = Description,
                Version = Version.ToString(),
                Author = Author,
                AuthorUrl = AuthorUrl,
                Url = ProjectUrl,
                IconUrl = IconUrl,
                LicenseUrl = LicenseUrl
            };

            if (System.Version.TryParse(UmbracoVersion, out Version umbracoVersion))
            {
                definition.UmbracoVersion = umbracoVersion;
            }

            return definition;
        }
    }
}