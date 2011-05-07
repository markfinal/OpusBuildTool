// <copyright file="ProjectConfigurationCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectConfigurationCollection : System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectConfiguration> list = new System.Collections.Generic.List<ProjectConfiguration>();
        private System.Collections.Generic.Dictionary<string, string> targetToConfig = new System.Collections.Generic.Dictionary<string, string>();

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public void Add(Opus.Core.Target target, ProjectConfiguration configuration)
        {
            this.list.Add(configuration);
            this.targetToConfig.Add(target.ToString(), configuration.Name);
        }

        public string GetConfigurationNameForTarget(Opus.Core.Target target)
        {
            string configurationName = this.targetToConfig[target.ToString()];
            return configurationName;
        }

        public bool Contains(string configurationName)
        {
            foreach (ProjectConfiguration configuration in this.list)
            {
                if (configurationName == configuration.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public ProjectConfiguration this[string configurationName]
        {
            get
            {
                foreach (ProjectConfiguration configuration in this.list)
                {
                    if (configurationName == configuration.Name)
                    {
                        return configuration;
                    }
                }

                throw new Opus.Core.Exception(System.String.Format("There is no ProjectConfiguration called '{0}'", configurationName));
            }
        }

        public void SerializeMSBuild(MSBuildProject project, System.Uri projectUri)
        {
            // ProjectConfigurations item group
            {
                MSBuildItemGroup configurationsGroup = project.CreateItemGroup();
                configurationsGroup.Label = "ProjectConfigurations";
                foreach (ProjectConfiguration configuration in this.list)
                {
                    configuration.SerializeMSBuild(configurationsGroup, projectUri);
                }
            }

            // configuration type and character set
            foreach (ProjectConfiguration configuration in this.list)
            {
                string[] split = configuration.ConfigurationPlatform();

                MSBuildPropertyGroup configurationGroup = project.CreatePropertyGroup();
                configurationGroup.Label = "Configuration";
                configurationGroup.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                configurationGroup.CreateProperty("ConfigurationType", configuration.Type.ToString());
                configurationGroup.CreateProperty("CharacterSet", configuration.CharacterSet.ToString());
            }

            // import property sheets AFTER the configuration types
            project.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.props");

            // output and intermediate directories
            foreach (ProjectConfiguration configuration in this.list)
            {
                string[] split = configuration.ConfigurationPlatform();

                MSBuildPropertyGroup dirGroup = project.CreatePropertyGroup();
                dirGroup.CreateProperty("_ProjectFileVersion", "10.0.40219.1"); // TODO, and this means what?

                if (null != configuration.OutputDirectory)
                {
                    string outputDir = Opus.Core.RelativePathUtilities.GetPath(configuration.OutputDirectory, projectUri);
                    if (!outputDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    {
                        outputDir += System.IO.Path.DirectorySeparatorChar;
                    }
                    MSBuildProperty outDirProperty = dirGroup.CreateProperty("OutDir", outputDir);
                    outDirProperty.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                }

                if (null != configuration.IntermediateDirectory)
                {
                    string intermediateDir = Opus.Core.RelativePathUtilities.GetPath(configuration.IntermediateDirectory, projectUri);
                    if (!intermediateDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    {
                        intermediateDir += System.IO.Path.DirectorySeparatorChar;
                    }
                    MSBuildProperty intDirProperty = dirGroup.CreateProperty("IntDir", intermediateDir);
                    intDirProperty.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                }
            }

            // tools
            foreach (ProjectConfiguration configuration in this.list)
            {
                configuration.Tools.SerializeMSBuild(project, configuration, projectUri);
            }
        }
    }
}