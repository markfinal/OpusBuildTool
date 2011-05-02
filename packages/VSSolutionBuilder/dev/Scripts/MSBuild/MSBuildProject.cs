// <copyright file="MSBuildProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildProject : ICProject
    {
        private string ProjectName = null;
        private string PathName = null;
        private System.Uri PackageUri = null;
        private System.Guid ProjectGuid = System.Guid.NewGuid();
        private System.Collections.Generic.List<string> PlatformList = new System.Collections.Generic.List<string>();
        private ProjectConfigurationCollection ProjectConfigurations = new ProjectConfigurationCollection();
        private ProjectFileCollection SourceFileCollection = new ProjectFileCollection();
        private ProjectFileCollection HeaderFileCollection = new ProjectFileCollection();
        private System.Collections.Generic.List<IProject> DependentProjectList = new System.Collections.Generic.List<IProject>();

        public MSBuildProject(string moduleName, string projectPathName, string packageDirectory)
        {
            this.ProjectName = moduleName;
            this.PathName = projectPathName;
            this.PackageDirectory = packageDirectory;

            if (packageDirectory[packageDirectory.Length - 1] == System.IO.Path.DirectorySeparatorChar)
            {
                this.PackageUri = new System.Uri(packageDirectory, System.UriKind.Absolute);
            }
            else
            {
                this.PackageUri = new System.Uri(packageDirectory + System.IO.Path.DirectorySeparatorChar, System.UriKind.Absolute);
            }
        }

        string IProject.Name
        {
            get
            {
                return this.ProjectName;
            }
        }

        string IProject.PathName
        {
            get
            {
                return this.PathName;
            }
        }

        System.Guid IProject.Guid
        {
            get
            {
                return this.ProjectGuid;
            }
        }

        public string PackageDirectory
        {
            get;
            private set;
        }

        System.Collections.Generic.List<string> IProject.Platforms
        {
            get
            {
                return this.PlatformList;
            }
        }

        ProjectConfigurationCollection IProject.Configurations
        {
            get
            {
                return this.ProjectConfigurations;
            }
        }

        ProjectFileCollection IProject.SourceFiles
        {
            get
            {
                return this.SourceFileCollection;
            }
        }

        ProjectFileCollection ICProject.HeaderFiles
        {
            get
            {
                return this.HeaderFileCollection;
            }
        }

        System.Collections.Generic.List<IProject> IProject.DependentProjects
        {
            get
            {
                return this.DependentProjectList;
            }
        }

        void IProject.Serialize()
        {
            System.Xml.XmlDocument xmlDocument = null;
            try
            {
                System.Uri projectLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                xmlDocument = new System.Xml.XmlDocument();

                xmlDocument.AppendChild(xmlDocument.CreateComment("Automatically generated by Opus v" + Opus.Core.State.OpusVersionString));

                string xmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

                // NOTE: .vcxproj files are read IN ORDER, TOP DOWN, so order matters

                // project root element
                System.Xml.XmlElement projectElement = xmlDocument.CreateElement("", "Project", xmlNamespace);
                projectElement.SetAttribute("DefaultTargets", "Build");
                projectElement.SetAttribute("ToolsVersion", "4.0");

                // project globals (guid, etc)
                {
                    System.Xml.XmlElement globalsElement = xmlDocument.CreateElement("", "PropertyGroup", xmlNamespace);
                    globalsElement.SetAttribute("Label", "Globals");
                    System.Xml.XmlElement projectGuidElement = xmlDocument.CreateElement("", "ProjectGuid", xmlNamespace);
                    projectGuidElement.InnerText = this.ProjectGuid.ToString("B").ToUpper();
                    globalsElement.AppendChild(projectGuidElement);
                    projectElement.AppendChild(globalsElement);
                }

                // import default project props
                {
                    System.Xml.XmlElement importElement = xmlDocument.CreateElement("", "Import", xmlNamespace);
                    importElement.SetAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.Default.props");
                    projectElement.AppendChild(importElement);
                }

                // configurations
                this.ProjectConfigurations.SerializeMSBuild(xmlDocument, projectElement, projectLocationUri, xmlNamespace);

                // source files
                if (this.SourceFileCollection.Count > 0)
                {
                    projectElement.AppendChild(this.SourceFileCollection.SerializeMSBuild(xmlDocument, "ClCompile", projectLocationUri, this.PackageUri, xmlNamespace));
                }
                if (this.HeaderFileCollection.Count > 0)
                {
                    projectElement.AppendChild(this.SourceFileCollection.SerializeMSBuild(xmlDocument, "ClInclude", projectLocationUri, this.PackageUri, xmlNamespace));
                }

                // project dependencies
                // these were in the .sln file pre MSBuild
                if (this.DependentProjectList.Count > 0)
                {
                    System.Xml.XmlElement dependentProjectItemGroup = xmlDocument.CreateElement("", "ItemGroup", xmlNamespace);
                    foreach (IProject dependentProject in this.DependentProjectList)
                    {
                        System.Xml.XmlElement projectReference = xmlDocument.CreateElement("", "ProjectReference", xmlNamespace);
                        string relativePath = Opus.Core.RelativePathUtilities.GetPath(dependentProject.PathName, this.PathName);
                        projectReference.SetAttribute("Include", relativePath);

                        System.Xml.XmlElement projectGuid = xmlDocument.CreateElement("", "Project", xmlNamespace);
                        projectGuid.InnerText = dependentProject.Guid.ToString("B");
                        projectReference.AppendChild(projectGuid);

                        System.Xml.XmlElement referenceOutputAssembly = xmlDocument.CreateElement("", "ReferenceOutputAssembly", xmlNamespace);
                        referenceOutputAssembly.InnerText = "false";
                        projectReference.AppendChild(referenceOutputAssembly);

                        dependentProjectItemGroup.AppendChild(projectReference);
                    }
                    projectElement.AppendChild(dependentProjectItemGroup);
                }

                // import targets
                {
                    System.Xml.XmlElement importElement = xmlDocument.CreateElement("", "Import", xmlNamespace);
                    importElement.SetAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.targets");
                    projectElement.AppendChild(importElement);
                }

                xmlDocument.AppendChild(projectElement);
            }
            catch (Opus.Core.Exception exception)
            {
                string message = System.String.Format("Xml construction error from project '{0}'", this.PathName);
                throw new Opus.Core.Exception(message, exception);
            }

            // write XML to disk
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.PathName));

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = false;

            try
            {
                using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(this.PathName, xmlWriterSettings))
                {
                    xmlDocument.Save(xmlWriter);
                }
            }
            catch (Opus.Core.Exception exception)
            {
                string message = System.String.Format("Serialization error from project '{0}'", this.PathName);
                throw new Opus.Core.Exception(message, exception);
            }
        }
    }
}
