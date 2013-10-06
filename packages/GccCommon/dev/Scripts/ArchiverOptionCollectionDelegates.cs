// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/IArchiverOptions.cs:IArchiverOptions.cs -n=GccCommon -c=ArchiverOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=PrivateData

namespace GccCommon
{
    public partial class ArchiverOptionCollection
    {
        #region C.IArchiverOptions Option delegates
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EArchiverOutput> enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    ArchiverOptionCollection options = sender as ArchiverOptionCollection;
                    if (options.LibraryFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("\"{0}\"", options.LibraryFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(options.LibraryFilePath);
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }
        private static void OutputTypeXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static void AdditionalOptionsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        #endregion
        #region IArchiverOptions Option delegates
        private static void CommandCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EArchiverCommand> commandOption = option as Opus.Core.ValueTypeOption<EArchiverCommand>;
            switch (commandOption.Value)
            {
                case EArchiverCommand.Replace:
                    commandLineBuilder.Add("-r");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized command option");
            }
        }
        private static void CommandXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void DoNotWarnIfLibraryCreatedCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-c");
            }
        }
        private static void DoNotWarnIfLibraryCreatedXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeXcodeProjectProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsXcodeProjectProcessor);
            this["Command"].PrivateData = new PrivateData(CommandCommandLineProcessor,CommandXcodeProjectProcessor);
            this["DoNotWarnIfLibraryCreated"].PrivateData = new PrivateData(DoNotWarnIfLibraryCreatedCommandLineProcessor,DoNotWarnIfLibraryCreatedXcodeProjectProcessor);
        }
    }
}
