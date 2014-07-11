// <copyright file="MakeFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFile
    {
        private string RelativePath(string path)
        {
            var relativeDir = Opus.Core.RelativePathUtilities.GetPath(path, this.TopLevelMakeFilePath, "$(CURDIR)");
            return relativeDir;
        }

        public MakeFileTargetDictionary ExportedTargets
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary ExportedVariables
        {
            get;
            private set;
        }

        private Opus.Core.StringArray InputFiles
        {
            get;
            set;
        }

        private Opus.Core.StringArray InputVariables
        {
            get;
            set;
        }

        private string ModulePrefixName
        {
            get;
            set;
        }

        public string VariableName
        {
            get
            {
                if (null == this.SecondaryVariableName)
                {
                    return this.MainVariableName;
                }
                else
                {
                    return this.SecondaryVariableName;
                }
            }
        }

        private string MainVariableName
        {
            get;
            set;
        }

        public string SecondaryVariableName
        {
            get;
            private set;
        }

        public string TargetName
        {
            get;
            private set;
        }

        private string TopLevelMakeFilePath
        {
            get;
            set;
        }

        public Opus.Core.StringArray Includes
        {
            get;
            private set;
        }

        public Opus.Core.Array<MakeFileRule> RuleArray
        {
            get;
            private set;
        }

        public static string InstanceName(Opus.Core.DependencyNode node)
        {
            var instanceName = System.String.Format("{0}_{1}", node.UniqueModuleName, node.Target.ToString());
            return instanceName;
        }

        public MakeFile(Opus.Core.DependencyNode node,
                        string topLevelMakeFilePath)
        {
            this.TopLevelMakeFilePath = topLevelMakeFilePath;
            this.ExportedTargets = new MakeFileTargetDictionary();
            this.ExportedVariables = new MakeFileVariableDictionary();
            this.RuleArray = new Opus.Core.Array<MakeFileRule>();
            this.ModulePrefixName = InstanceName(node);
        }

        public void Write(System.IO.TextWriter writer)
        {
            if (0 == this.RuleArray.Count)
            {
                throw new Opus.Core.Exception("MakeFile '{0}' has no rules", this.ModulePrefixName);
            }

            var ruleCount = this.RuleArray.Count;
            var ruleIndex = 0;
            foreach (var rule in this.RuleArray)
            {
                string mainVariableName;
                if (ruleCount > 1)
                {
                    mainVariableName = System.String.Format("{0}{1}", this.ModulePrefixName, ruleIndex++);
                }
                else
                {
                    mainVariableName = this.ModulePrefixName;
                }

                string outputDirectoriesVariable = null;
                if (null != rule.DirectoriesToCreate)
                {
                    writer.WriteLine("# Define directories to create");
                    string linearizedDirsToCreate = null;
                    foreach (var dir in rule.DirectoriesToCreate)
                    {
                        var dirPath = dir.GetSinglePath();
                        var relativeDir = this.RelativePath(dirPath);
                        linearizedDirsToCreate += relativeDir + " ";
                    }
                    outputDirectoriesVariable = System.String.Format("{0}_BuildDirs", mainVariableName);
                    writer.WriteLine("{0} := {1}", outputDirectoriesVariable, linearizedDirsToCreate);
                    writer.WriteLine("builddirs += $({0})", outputDirectoriesVariable);
                    writer.WriteLine("");
                }

                string mainExportVariableName = null;
                if (rule.ExportVariable)
                {
#if true
                    if (rule.ModuleToBuild is Opus.Core.IModuleCollection)
                    {
                        writer.WriteLine("# Output variable (collection)");
                        var exportVariableName = System.String.Format("{0}_{1}_CollectionVariable", mainVariableName, rule.PrimaryOutputLocationKey.ToString());
                        mainExportVariableName = exportVariableName;

                        var variableAndValue = new System.Text.StringBuilder();
                        variableAndValue.AppendFormat("{0} := ", exportVariableName);
                        foreach (var prerequisite in rule.InputVariables)
                        {
                            foreach (var pre in prerequisite.Value)
                            {
                                variableAndValue.AppendFormat("$({0}) ", pre);
                            }
                        }
                        if (null != rule.InputFiles)
                        {
                            foreach (var prerequisiteFile in rule.InputFiles)
                            {
                                var relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                                variableAndValue.AppendFormat("{0} ", relativePrerequisiteFile);
                            }
                        }

                        writer.WriteLine(variableAndValue.ToString());
                        writer.WriteLine("");

                        var exportedVariables = new Opus.Core.StringArray();
                        exportedVariables.Add(exportVariableName);
                        this.ExportedVariables.Add(rule.PrimaryOutputLocationKey, exportedVariables);
                    }
                    else
                    {
#if false
                        var outputKeys = new Opus.Core.Array<Opus.Core.LocationKey>();
                        var fileKeys = rule.ModuleToBuild.Locations.Keys(Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.WillExist);
                        var symlinkKeys = rule.ModuleToBuild.Locations.Keys(Opus.Core.ScaffoldLocation.ETypeHint.Symlink, Opus.Core.Location.EExists.WillExist);
                        // TODO: directory keys here list more locations than are needed
                        var dirKeys = rule.ModuleToBuild.Locations.Keys(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
                        outputKeys.AddRangeUnique(fileKeys);
                        outputKeys.AddRangeUnique(symlinkKeys);
                        outputKeys.AddRangeUnique(dirKeys);
#endif
                        if (null == rule.OutputLocationKeys)
                        {
                            throw new Opus.Core.Exception("No output keys have been assigned to Makefile rule for target '{1}'", rule.ModuleToBuild.OwningNode.UniqueModuleName, rule.Target);
                        }

                        foreach (var outputLocKey in rule.OutputLocationKeys)
                        {
                            var outputLoc = rule.ModuleToBuild.Locations[outputLocKey];
                            if (!outputLoc.IsValid)
                            {
                                continue;
                            }

                            var exportVariableName = System.String.Format("{0}_{1}_Variable", mainVariableName, outputLocKey.ToString());
                            if (outputLocKey == rule.PrimaryOutputLocationKey)
                            {
                                mainExportVariableName = exportVariableName;
                            }
                            writer.WriteLine("# Output variable: '{0}'", outputLocKey.ToString());

                            if (rule.ExportTarget)
                            {
                                var relativeOutputPath = this.RelativePath(outputLoc.GetSinglePath());
                                writer.WriteLine(exportVariableName + " := " + relativeOutputPath);
                                writer.WriteLine("");
                            }
                            else
                            {
                                var variableAndValue = new System.Text.StringBuilder();
                                variableAndValue.AppendFormat("{0} := ", exportVariableName);
                                foreach (var prerequisite in rule.InputVariables)
                                {
                                    foreach (var pre in prerequisite.Value)
                                    {
                                        variableAndValue.AppendFormat("$({0}) ", pre);
                                    }
                                }
                                if (null != rule.InputFiles)
                                {
                                    foreach (var prerequisiteFile in rule.InputFiles)
                                    {
                                        var relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                                        variableAndValue.AppendFormat("{0} ", relativePrerequisiteFile);
                                    }
                                }

                                writer.WriteLine(variableAndValue.ToString());
                                writer.WriteLine("");
                            }

                            var exportedVariables = new Opus.Core.StringArray();
                            exportedVariables.Add(exportVariableName);
                            this.ExportedVariables.Add(outputLocKey, exportedVariables);

                            if (null != outputDirectoriesVariable)
                            {
                                writer.WriteLine("# Order-only dependencies on directories to create");
                                writer.WriteLine("$({0}): | $({1})", exportVariableName, outputDirectoriesVariable);
                                writer.WriteLine("");
                            }
                        }
                    }
#else
                    if (null == rule.OutputPaths)
                    {
                        writer.WriteLine("# Output variable (collection)");
                        string exportVariableName = System.String.Format("{0}_{1}_Variable", mainVariableName, rule.PrimaryOutputType.ToString());
                        mainExportVariableName = exportVariableName;

                        System.Text.StringBuilder variableAndValue = new System.Text.StringBuilder();
                        variableAndValue.AppendFormat("{0} := ", exportVariableName);
                        foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> prerequisite in rule.InputVariables)
                        {
                            foreach (string pre in prerequisite.Value)
                            {
                                variableAndValue.AppendFormat("$({0}) ", pre);
                            }
                        }
                        if (null != rule.InputFiles)
                        {
                            foreach (string prerequisiteFile in rule.InputFiles)
                            {
                                string relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                                variableAndValue.AppendFormat("{0} ", relativePrerequisiteFile);
                            }
                        }

                        writer.WriteLine(variableAndValue.ToString());
                        writer.WriteLine("");

                        Opus.Core.StringArray exportedVariables = new Opus.Core.StringArray();
                        exportedVariables.Add(exportVariableName);
                        this.ExportedVariables.Add(rule.PrimaryOutputType, exportedVariables);
                    }
                    else
                    {
                        foreach (System.Enum outputType in rule.OutputPaths.Types)
                        {
                            string exportVariableName = System.String.Format("{0}_{1}_Variable", mainVariableName, outputType.ToString());
                            if (outputType.Equals(rule.PrimaryOutputType))
                            {
                                mainExportVariableName = exportVariableName;
                            }
                            writer.WriteLine("# Output variable: '{0}'", outputType.ToString());

                            if (rule.ExportTarget)
                            {
                                string relativeOutputPath = this.RelativePath(rule.OutputPaths[outputType]);
                                writer.WriteLine(exportVariableName + " := " + relativeOutputPath);
                                writer.WriteLine("");
                            }
                            else
                            {
                                System.Text.StringBuilder variableAndValue = new System.Text.StringBuilder();
                                variableAndValue.AppendFormat("{0} := ", exportVariableName);
                                foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> prerequisite in rule.InputVariables)
                                {
                                    foreach (string pre in prerequisite.Value)
                                    {
                                        variableAndValue.AppendFormat("$({0}) ", pre);
                                    }
                                }
                                if (null != rule.InputFiles)
                                {
                                    foreach (string prerequisiteFile in rule.InputFiles)
                                    {
                                        string relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                                        variableAndValue.AppendFormat("{0} ", relativePrerequisiteFile);
                                    }
                                }

                                writer.WriteLine(variableAndValue.ToString());
                                writer.WriteLine("");
                            }

                            Opus.Core.StringArray exportedVariables = new Opus.Core.StringArray();
                            exportedVariables.Add(exportVariableName);
                            this.ExportedVariables.Add(outputType, exportedVariables);

                            if (null != outputDirectoriesVariable)
                            {
                                writer.WriteLine("# Order-only dependencies on directories to create");
                                writer.WriteLine("$({0}): | $({1})", exportVariableName, outputDirectoriesVariable);
                                writer.WriteLine("");
                            }
                        }
                    }
#endif
                }

                if (rule.ExportTarget && null != mainExportVariableName)
                {
                    var exportTargetName = System.String.Format("{0}_{1}_Target", mainVariableName, rule.PrimaryOutputLocationKey.ToString());
                    writer.WriteLine("# Output target");
                    if (rule.TargetIsPhony)
                    {
                        writer.WriteLine(".PHONY: {0}", exportTargetName);
                    }
                    writer.WriteLine(exportTargetName + ": $(" + mainExportVariableName + ")");
                    writer.WriteLine("");
                    this.ExportedTargets.Add(rule.PrimaryOutputLocationKey, exportTargetName);

                    if (null == rule.Recipes)
                    {
                        continue;
                    }

                    var targetAndPrerequisites = new System.Text.StringBuilder();
                    if (null != mainExportVariableName)
                    {
                        targetAndPrerequisites.AppendFormat("$({0}): ", mainExportVariableName);
                    }
                    else
                    {
                        var targetName = System.String.Format("{0}_{1}_Target", mainVariableName, rule.PrimaryOutputLocationKey.ToString());
                        if (rule.TargetIsPhony)
                        {
                            writer.WriteLine(".PHONY: {0}", targetName);
                        }
                        targetAndPrerequisites.AppendFormat("{0}: ", targetName);
                        if (rule.ExportTarget)
                        {
                            this.ExportedTargets.Add(rule.PrimaryOutputLocationKey, targetName);
                        }
                    }
                    if (null != rule.InputVariables)
                    {
                        foreach (var prerequisite in rule.InputVariables)
                        {
                            foreach (var pre in prerequisite.Value)
                            {
                                targetAndPrerequisites.AppendFormat("$({0}) ", pre);
                            }
                        }
                    }
                    if (null != rule.InputFiles)
                    {
                        foreach (var prerequisiteFile in rule.InputFiles)
                        {
                            var relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                            targetAndPrerequisites.AppendFormat("{0} ", relativePrerequisiteFile);
                        }
                    }

                    writer.WriteLine("# Rule");
                    writer.WriteLine(targetAndPrerequisites.ToString());
                    foreach (var recipe in rule.Recipes)
                    {
                        writer.WriteLine("\t" + recipe);
                    }
                    writer.WriteLine("");
                }
            }
        }
    }
}
