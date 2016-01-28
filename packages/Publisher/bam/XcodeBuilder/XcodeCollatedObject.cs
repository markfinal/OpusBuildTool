#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace Publisher
{
    public sealed class XcodeCollatedObject :
        ICollatedObjectPolicy
    {
        void
        ICollatedObjectPolicy.Collate(
            CollatedObject sender,
            Bam.Core.ExecutionContext context)
        {
            var sourcePath = sender.SourcePath;
            if (null == sender.Reference)
            {
                // the main file is not copied anywhere, as we copy required files around it where Xcode wrote the main file
                // this is managed by the Collation class, querying the build mode for where publishing is relative to
                // ignore any subdirectory on this module

                // convert the executable into an app bundle, if EPublishingType.WindowedApplication has been used as the type
                if ((sender.SubDirectory != null) && sender.SubDirectory.Parse().Contains(".app/"))
                {
                    var target = sender.SourceModule.MetaData as XcodeBuilder.Target;
                    target.MakeApplicationBundle();
                }

                return;
            }

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            var destinationPath = sender.Macros["CopyDir"].Parse();
            var commands = new Bam.Core.StringArray();
            commands.Add(System.String.Format("[[ ! -d {0} ]] && mkdir -p {0}", destinationPath));

            if (sender.SourceModule != null && sender.SourceModule.MetaData != null)
            {
                if ((null != sender.Reference) &&
                    (sender.SourceModule.PackageDefinition == sender.Reference.PackageDefinition) &&
                    (null == sender.Reference.SubDirectory) &&
                    (sender.SubDirectory.Parse() == "."))
                {
                    // special case that the module output is already in the right directory at build
                    return;
                }

                commands.Add(System.String.Format("{0} {1} $CONFIGURATION_BUILD_DIR/$EXECUTABLE_NAME {2} {3}",
                    CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                    commandLine.ToString(' '),
                    destinationPath,
                    CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));

                var target = sender.SourceModule.MetaData as XcodeBuilder.Target;
                var configuration = target.GetConfiguration(sender.SourceModule);
                target.AddPostBuildCommands(commands, configuration);
            }
            else
            {
                var isSymlink = (sender is CollatedSymbolicLink);

                var destinationFolder = "$CONFIGURATION_BUILD_DIR";
                if (sender.Reference != null)
                {
                    destinationFolder = "$CONFIGURATION_BUILD_DIR/$EXECUTABLE_FOLDER_PATH";
                }

                if (isSymlink)
                {
                    commands.Add(System.String.Format("{0} {1} {2} {3}/{4} {5}",
                        CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                        commandLine.ToString(' '),
                        sender.Macros["LinkTarget"].Parse(),
                        destinationFolder,
                        sender.CreateTokenizedString("$(0)/@filename($(1))", sender.SubDirectory, sender.SourcePath).Parse(),
                        CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                }
                else
                {
                    if (sender is CollatedDirectory)
                    {
                        var copySource = sourcePath.Parse();
                        if (sender.Macros["CopiedFilename"].IsAliased)
                        {
                            copySource = System.String.Format("{0}/*", copySource);
                        }

                        commands.Add(System.String.Format("{0} {1} {2} {3}/{4}/ {5}",
                            CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                            commandLine.ToString(' '),
                            copySource,
                            destinationFolder,
                            sender.CreateTokenizedString("$(0)/@ifnotempty($(CopiedFilename),$(CopiedFilename),@filename($(1)))", sender.SubDirectory, sourcePath).Parse(),
                            CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                    }
                    else
                    {
                        commands.Add(System.String.Format("{0} {1} {2} {3}/{4}/ {5}",
                            CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                            commandLine.ToString(' '),
                            sourcePath.Parse(),
                            destinationFolder,
                            sender.SubDirectory.Parse(),
                            CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                    }
                }

                var target = sender.Reference.SourceModule.MetaData as XcodeBuilder.Target;
                var configuration = target.GetConfiguration(sender.Reference.SourceModule);
                target.AddPostBuildCommands(commands, configuration);
            }
        }
    }
}
