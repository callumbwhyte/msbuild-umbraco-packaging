using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Packaging;

namespace MSBuild.Umbraco.Packaging
{
    public class PackageDefinitionHelper
    {
        public XDocument ExportPackage(PackageDefinition definition)
        {
            ValidatePackage(definition);

            var root = new XElement("umbPackage");

            var info = new XElement("info");

            info.Add(GetPackageInfo(definition));
            info.Add(GetAuthor(definition));
            info.Add(GetReadme(definition));

            root.Add(info);

            root.Add(GetFiles(definition));
            root.Add(GetPackageView(definition));

            var doc = new XDocument(root);

            return doc;
        }

        private void ValidatePackage(PackageDefinition definition)
        {
            var context = new ValidationContext(definition, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(definition, context, results);

            if (!isValid)
            {
                throw new InvalidOperationException("Validation failed, there is invalid data on the model: " + string.Join(", ", results.Select(x => x.ErrorMessage)));
            }
        }

        private static XElement GetPackageInfo(PackageDefinition definition)
        {
            var packageInfo = new XElement("package");

            packageInfo.Add(new XElement("name", definition.Name));
            packageInfo.Add(new XElement("version", definition.Version));
            packageInfo.Add(new XElement("iconUrl", definition.IconUrl));
            packageInfo.Add(new XElement("url", definition.Url));

            packageInfo.Add(GetLicense(definition));
            packageInfo.Add(GetRequirements(definition));

            return packageInfo;
        }

        private static XElement GetLicense(PackageDefinition definition)
        {
            var license = new XElement("license");

            if (string.IsNullOrWhiteSpace(definition.License) == false)
            {
                license.Add(definition.License);
            }

            if (string.IsNullOrWhiteSpace(definition.LicenseUrl) == false)
            {
                license.Add(new XAttribute("url", definition.LicenseUrl));
            }

            return license;
        }

        private static XElement GetRequirements(PackageDefinition definition)
        {
            var requirements = new XElement("requirements");

            if (definition.UmbracoVersion != null)
            {
                requirements.Add(new XElement("major", definition.UmbracoVersion.Major));
                requirements.Add(new XElement("minor", definition.UmbracoVersion.Minor));
                requirements.Add(new XElement("patch", definition.UmbracoVersion.Build));

                requirements.Add(new XAttribute("type", RequirementsType.Strict.ToString()));
            }
            else
            {
                requirements.Add(new XElement("major", UmbracoVersion.SemanticVersion.Major));
                requirements.Add(new XElement("minor", UmbracoVersion.SemanticVersion.Minor));
                requirements.Add(new XElement("patch", UmbracoVersion.SemanticVersion.Patch));
            }

            return requirements;
        }

        private static XElement GetAuthor(PackageDefinition definition)
        {
            var author = new XElement("author");

            if (string.IsNullOrWhiteSpace(definition.Author) == false)
            {
                author.Add(new XElement("name", definition.Author));
            }

            if (string.IsNullOrWhiteSpace(definition.AuthorUrl) == false)
            {
                author.Add(new XElement("website", definition.AuthorUrl));
            }

            return author;
        }

        private static XElement GetReadme(PackageDefinition definition)
        {
            var readme = new XElement("readme");

            if (string.IsNullOrWhiteSpace(definition.Readme) == false)
            {
                readme.Add(new XCData(definition.Readme));
            }

            return readme;
        }

        private static XElement GetPackageView(PackageDefinition definition)
        {
            var view = new XElement("view");

            if (string.IsNullOrWhiteSpace(definition.PackageView) == false)
            {
                view.Add(definition.PackageView);
            }

            return view;
        }

        private static XElement GetFiles(PackageDefinition definition)
        {
            var files = new XElement("files");

            foreach (var fileName in definition.Files)
            {
                AppendFileXml(fileName, files);
            }

            return files;
        }

        private static void AppendFileXml(string filePath, XContainer filesXml)
        {
            var orgPath = filePath.Substring(0, filePath.LastIndexOf('/'));
            var orgName = filePath.Substring(filePath.LastIndexOf('/') + 1);

            filesXml.Add(new XElement("file",
                new XElement("guid", orgName),
                new XElement("orgPath", orgPath == "" ? "/" : orgPath),
                new XElement("orgName", orgName)));
        }
    }
}