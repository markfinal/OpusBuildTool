#region License
// Copyright (c) 2010-2019, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using System.Linq;
namespace VSSolutionBuilder
{
    /// <summary>
    /// Utility class offering more useful functions for writing .vcxprojs
    /// </summary>
    static partial class Support
    {
        static private void
        AddModuleDirectoryCreationShellCommands(
            Bam.Core.Module module,
            Bam.Core.StringArray shellCommandLines)
        {
            foreach (var dir in module.OutputDirectories)
            {
                var quotedDir = dir.ToStringQuoteIfNecessary();
                shellCommandLines.Add($"IF NOT EXIST {quotedDir} MKDIR {quotedDir}");
            }
        }

        static private void
        AddModuleCommandLineShellCommand(
            Bam.Core.Module module,
            Bam.Core.StringArray shellCommandLines,
            bool includeEnvironmentVariables)
        {
            if (null == module.Tool)
            {
                throw new Bam.Core.Exception(
                    $"Command line tool passed with module '{module.ToString()}' is invalid"
                );
            }
            System.Diagnostics.Debug.Assert(module.Tool is Bam.Core.ICommandLineTool);

            var args = new Bam.Core.StringArray();
            if (module.WorkingDirectory != null)
            {
                args.Add(
                    $"cd /D {module.WorkingDirectory.ToStringQuoteIfNecessary()} &&"
                );
            }
            var tool = module.Tool as Bam.Core.ICommandLineTool;
            if (tool.EnvironmentVariables != null && includeEnvironmentVariables)
            {
                foreach (var envVar in tool.EnvironmentVariables)
                {
                    args.Add("set");
                    var content = new System.Text.StringBuilder();
                    content.Append($"{envVar.Key}=");
                    foreach (var value in envVar.Value)
                    {
                        content.Append($"{value.ToStringQuoteIfNecessary()};");
                    }
                    content.Append($"%{envVar.Key}%");
                    args.Add(content.ToString());
                    args.Add("&&");
                }
            }
            args.Add(CommandLineProcessor.Processor.StringifyTool(tool));
            args.AddRange(
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                )
            );
            args.Add(CommandLineProcessor.Processor.TerminatingArgs(tool));
            shellCommandLines.Add(args.ToString(' '));
        }

        static private Bam.Core.Module
        GetModuleForProject(
            Bam.Core.Module requestingModule)
        {
            var projectModule = requestingModule.EncapsulatingModule;
#if D_PACKAGE_PUBLISHER
            if (projectModule is Publisher.Collation)
            {
                (projectModule as Publisher.Collation).ForEachAnchor(
                    (collation, anchor, customData) =>
                    {
                        projectModule = anchor.SourceModule;
                    },
                    null
                );
            }
#endif
            return projectModule;
        }

        private static void
        AddRedirectToFile(
            Bam.Core.TokenizedString outputFile,
            Bam.Core.StringArray shellCommandLines)
        {
            var command = shellCommandLines.Last();
            shellCommandLines.Remove(command);
            var redirectedCommand = command + $" > {outputFile.ToString()}";
            shellCommandLines.Add(redirectedCommand);
        }

        /// <summary>
        /// Add a custom build step for the module into the .vcxproj
        /// </summary>
        /// <param name="module">Module whose .vcxproj will be modified</param>
        /// <param name="inputs">List of input paths</param>
        /// <param name="outputs">List of output paths</param>
        /// <param name="message">Message to output when running the custom build step</param>
        /// <param name="commandList">List of commands to run</param>
        /// <param name="commandIsForFirstInputOnly">Whether the command should only be run on the first input, or all.</param>
        /// <param name="addInputFilesToProject">Whether to add input files to the project file</param>
        static public void
        AddCustomBuildStep(
            Bam.Core.Module module,
            System.Collections.Generic.IEnumerable<Bam.Core.TokenizedString> inputs,
            System.Collections.Generic.IEnumerable<Bam.Core.TokenizedString> outputs,
            string message,
            Bam.Core.StringArray commandList,
            bool commandIsForFirstInputOnly,
            bool addInputFilesToProject)
        {
            var projectModule = GetModuleForProject(module);
            var solution = Bam.Core.Graph.Instance.MetaData as VSSolution;
            var project = solution.EnsureProjectExists(projectModule.GetType(), projectModule.BuildEnvironment);
            var config = project.GetConfiguration(projectModule);

            var is_first_input = true;
            foreach (var input in inputs)
            {
                if (addInputFilesToProject)
                {
                    config.AddOtherFile(input);
                }
                if (!is_first_input && commandIsForFirstInputOnly)
                {
                    continue;
                }
                var customBuild = config.GetSettingsGroup(
                    VSSettingsGroup.ESettingsGroup.CustomBuild,
                    input
                );
                customBuild.AddSetting("Command", commandList.ToString(System.Environment.NewLine), condition: config.ConditionText);
                customBuild.AddSetting("Message", message, condition: config.ConditionText);
                customBuild.AddSetting("Outputs", outputs, condition: config.ConditionText);
                customBuild.AddSetting("AdditionalInputs", inputs, condition: config.ConditionText); // TODO: incorrectly adds all inputs
                is_first_input = false;
            }
        }

        /// <summary>
        /// Add a custom build step for the .vcxproj based on an ICommandLineTool
        /// </summary>
        /// <param name="module">Module containing the ICommandLineTool to run</param>
        /// <param name="outputPath">Output path generated from the build step</param>
        /// <param name="messagePrefix">Message to prefix the output with from running</param>
        /// <param name="addInputFilesToProject">Whether to add the input files to the project</param>
        static public void
        AddCustomBuildStepForCommandLineTool(
            Bam.Core.Module module,
            Bam.Core.TokenizedString outputPath,
            string messagePrefix,
            bool addInputFilesToProject)
        {
            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            AddModuleCommandLineShellCommand(module, shellCommandLines, includeEnvironmentVariables: true);

            System.Diagnostics.Debug.Assert(1 == module.InputModulePaths.Count());
            var firstInput = module.InputModulePaths.First();
            var sourcePath = firstInput.module.GeneratedPaths[firstInput.pathKey];
            AddCustomBuildStep(
                module,
                System.Linq.Enumerable.Repeat(sourcePath, 1),
                System.Linq.Enumerable.Repeat(outputPath, 1),
                $"{messagePrefix} {sourcePath.ToString()} into {outputPath.ToString()}",
                shellCommandLines,
                true,
                addInputFilesToProject
            );
        }

        /// <summary>
        /// Add prebuild steps for the module
        /// </summary>
        /// <param name="module">Module to add the prebuild steps for</param>
        /// <param name="config">Optional VSProjectConfiguration to add the prebuild step for. Default to null.</param>
        /// <param name="addOrderOnlyDependencyOnTool">Optional whether to add the tool as an order only dependency. Default to false.</param>
        static public void
        AddPreBuildSteps(
            Bam.Core.Module module,
            VSProjectConfiguration config = null,
            bool addOrderOnlyDependencyOnTool = false)
        {
            var solution = Bam.Core.Graph.Instance.MetaData as VSSolution;
            if (config == null)
            {
                var projectModule = GetModuleForProject(module);
                var project = solution.EnsureProjectExists(projectModule.GetType(), projectModule.BuildEnvironment);
                config = project.GetConfiguration(projectModule);
            }

            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            AddModuleCommandLineShellCommand(module, shellCommandLines, includeEnvironmentVariables: true);
            config.AddPreBuildCommands(shellCommandLines);

            if (addOrderOnlyDependencyOnTool)
            {
                var tool = module.Tool as Bam.Core.Module;
#if D_PACKAGE_PUBLISHER
                // note the custom handler here, which checks to see if we're running a tool
                // that has been collated
                if (tool is Publisher.CollatedCommandLineTool)
                {
                    tool = (tool as Publisher.ICollatedObject).SourceModule;
                }
#endif
                var toolProject = solution.EnsureProjectExists(tool.GetType(), tool.BuildEnvironment);
                config.RequiresProject(toolProject);
            }
        }

        /// <summary>
        /// Add post-build steps for the module
        /// </summary>
        /// <param name="module">Module whose tool generates command lines</param>
        /// <param name="config">Optional VSProjectConfiguration to add to. Default to null.</param>
        /// <param name="addOrderOnlyDependencyOnTool">Optional whether the tool is added as an order only dependency. Default to false.</param>
        static public void
        AddPostBuildSteps(
            Bam.Core.Module module,
            VSProjectConfiguration config = null,
            bool addOrderOnlyDependencyOnTool = false)
        {
            var solution = Bam.Core.Graph.Instance.MetaData as VSSolution;
            if (config == null)
            {
                var projectModule = GetModuleForProject(module);
                var project = solution.EnsureProjectExists(projectModule.GetType(), projectModule.BuildEnvironment);
                config = project.GetConfiguration(projectModule);
            }

            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            AddModuleCommandLineShellCommand(module, shellCommandLines, includeEnvironmentVariables: true);
            config.AddPostBuildCommands(shellCommandLines);

            if (addOrderOnlyDependencyOnTool)
            {
                var tool = module.Tool as Bam.Core.Module;
#if D_PACKAGE_PUBLISHER
                // note the custom handler here, which checks to see if we're running a tool
                // that has been collated
                if (tool is Publisher.CollatedCommandLineTool)
                {
                    tool = (tool as Publisher.ICollatedObject).SourceModule;
                }
#endif
                var toolProject = solution.EnsureProjectExists(tool.GetType(), tool.BuildEnvironment);
                config.RequiresProject(toolProject);
            }
        }

        /// <summary>
        /// Add custom shell commands to a post build step
        /// </summary>
        /// <param name="config">VSProjectConfiguration add to</param>
        /// <param name="module">Module corresponding to the project</param>
        /// <param name="commands">List of shell commands to add</param>
        static public void
        AddCustomPostBuildStep(
            VSProjectConfiguration config,
            Bam.Core.Module module,
            Bam.Core.StringArray commands)
        {
            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            shellCommandLines.AddRange(commands);
            config.AddPostBuildCommands(shellCommandLines);
        }

        /// <summary>
        /// Add a post build step corresponding to an ICommandLineTool
        /// </summary>
        /// <param name="module">Module with ICommandLineTool</param>
        /// <param name="moduleToAddBuildStepTo">The Module corresponding to the VSProject to add to</param>
        /// <param name="project">The VSProject written</param>
        /// <param name="configuration">The VSProjectConfiguration written</param>
        /// <param name="redirectToFile">Optional whether the output from the commands are redirected to this file. Default to null.</param>
        /// <param name="includeEnvironmentVariables">Optional whether environment variables are added to the commands. Default to true.</param>
        static public void
        AddPostBuildStepForCommandLineTool(
            Bam.Core.Module module,
            Bam.Core.Module moduleToAddBuildStepTo,
            out VSProject project,
            out VSProjectConfiguration configuration,
            Bam.Core.TokenizedString redirectToFile = null,
            bool includeEnvironmentVariables = true)
        {
            var solution = Bam.Core.Graph.Instance.MetaData as VSSolution;
            project = solution.EnsureProjectExists(moduleToAddBuildStepTo.GetType(), moduleToAddBuildStepTo.BuildEnvironment);
            configuration = project.GetConfiguration(moduleToAddBuildStepTo);

            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            AddModuleCommandLineShellCommand(module, shellCommandLines, includeEnvironmentVariables: includeEnvironmentVariables);
            if (null != redirectToFile)
            {
                AddRedirectToFile(redirectToFile, shellCommandLines);
            }
            configuration.AddPostBuildCommands(shellCommandLines);
        }

        /// <summary>
        /// Add a pre build step corresponding to an ICommandLineTool
        /// </summary>
        /// <param name="module">Module with ICommandLineTool</param>
        /// <param name="project">The VSProject written</param>
        /// <param name="configuration">The VSProjectConfiguration written</param>
        /// <param name="redirectToFile">Optional whether the output from the commands are redirected to this file. Default to null.</param>
        /// <param name="includeEnvironmentVariables">Optional whether environment variables are added to the commands. Default to true.</param>
        static public void
        AddPreBuildStepForCommandLineTool(
            Bam.Core.Module module,
            out VSProject project,
            out VSProjectConfiguration configuration,
            Bam.Core.TokenizedString redirectToFile = null,
            bool includeEnvironmentVariables = true)
        {
            var moduleToAddBuildStepTo = GetModuleForProject(module);
            var solution = Bam.Core.Graph.Instance.MetaData as VSSolution;
            project = solution.EnsureProjectExists(moduleToAddBuildStepTo.GetType(), moduleToAddBuildStepTo.BuildEnvironment);
            configuration = project.GetConfiguration(moduleToAddBuildStepTo);

            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            AddModuleCommandLineShellCommand(module, shellCommandLines, includeEnvironmentVariables: includeEnvironmentVariables);
            if (null != redirectToFile)
            {
                AddRedirectToFile(redirectToFile, shellCommandLines);
            }
            configuration.AddPreBuildCommands(shellCommandLines);
        }
    }
}
