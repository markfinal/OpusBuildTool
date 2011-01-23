// <copyright file="ProjectData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectData
    {
        public ProjectData(string name, string pathName)
        {
            this.Name = name;
            this.PathName = pathName;
            this.Guid = System.Guid.NewGuid();
            this.Configurations = new ProjectConfigurationCollection();
            this.SourceFiles = new ProjectFileCollection();
            this.HeaderFiles = new ProjectFileCollection();
            this.Platforms = new System.Collections.Generic.List<string>();
            this.DependentProjects = new System.Collections.Generic.List<ProjectData>();
        }

        public string Name
        {
            get;
            private set;
        }

        public string PathName
        {
            get;
            private set;
        }

        public System.Guid Guid
        {
            get;
            private set;
        }

        public ProjectFileCollection SourceFiles
        {
            get;
            private set;
        }

        public ProjectFileCollection HeaderFiles
        {
            get;
            private set;
        }

        public ProjectConfigurationCollection Configurations
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<string> Platforms
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<ProjectData> DependentProjects
        {
            get;
            private set;
        }

        public void Serialize()
        {
            if (0 == this.SourceFiles.Count)
            {
                throw new Opus.Core.Exception(System.String.Format("There are no source files for '{0}'", this.Name));
            }

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.PathName));

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = true;

            try
            {
                System.Uri projectLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(this.PathName, xmlWriterSettings))
                {
                    xmlWriter.WriteComment("Automatically generated by Opus v" + Opus.Core.State.OpusVersionString);

                    xmlWriter.WriteStartElement("VisualStudioProject");
                    {
                        // preamble
                        xmlWriter.WriteAttributeString("ProjectType", "Visual C++");
                        xmlWriter.WriteAttributeString("Version", VisualC.Project.Version);
                        xmlWriter.WriteAttributeString("Name", this.Name);
                        xmlWriter.WriteAttributeString("ProjectGUID", this.Guid.ToString("B").ToUpper());

                        // platforms
                        xmlWriter.WriteStartElement("Platforms");
                        {
                            foreach (string platform in this.Platforms)
                            {
                                xmlWriter.WriteStartElement("Platform");
                                {
                                    xmlWriter.WriteAttributeString("Name", platform);
                                }
                                xmlWriter.WriteEndElement();
                            }
                        }
                        xmlWriter.WriteEndElement();

                        // tool files

                        // configurations
                        xmlWriter.WriteStartElement("Configurations");
                        {
                            foreach (ProjectConfiguration configuration in this.Configurations)
                            {
                                configuration.Serialize(xmlWriter, projectLocationUri);
                            }
                        }
                        xmlWriter.WriteEndElement();

                        // files
                        xmlWriter.WriteStartElement("Files");
                        {
                            xmlWriter.WriteStartElement("Filter");
                            {
                                xmlWriter.WriteAttributeString("Name", "Source Files");
                                {
                                    foreach (ProjectFile path in this.SourceFiles)
                                    {
                                        path.Serialize(xmlWriter, projectLocationUri);
                                    }
                                }
                            }
                            xmlWriter.WriteEndElement();

                            if (this.HeaderFiles.Count > 0)
                            {
                                xmlWriter.WriteStartElement("Filter");
                                {
                                    xmlWriter.WriteAttributeString("Name", "Header Files");
                                    {
                                        foreach (ProjectFile path in this.HeaderFiles)
                                        {
                                            path.Serialize(xmlWriter, projectLocationUri);
                                        }
                                    }
                                }
                                xmlWriter.WriteEndElement();
                            }
                        }
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
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