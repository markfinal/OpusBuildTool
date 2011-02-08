// Automatically generated by Opus v0.00
namespace Test5
{
    // Define module classes here
    class MyDynamicLibTestApp : C.Application
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.AddRelativePaths(this, "source", "dynamicmain.c");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(Test4.MyDynamicLib),
            typeof(Test4.MyStaticLib)
        );

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }

    class PublishDynamicLibraries : FileUtilities.CopyFiles
    {
        [FileUtilities.SourceModules(C.OutputFileFlags.Executable)]
        Opus.Core.TypeArray sourceTargets = new Opus.Core.TypeArray(typeof(Test4.MyDynamicLib));

        [FileUtilities.DestinationModuleDirectory(C.OutputFileFlags.Executable)]
        Opus.Core.TypeArray destinationTarget = new Opus.Core.TypeArray(typeof(MyDynamicLibTestApp));
    }

    [Opus.Core.ModuleTargets("win.*-.*-.*")]
    class PublishPDBs : FileUtilities.CopyFiles
    {
        public PublishPDBs()
        {
            this.destinationDirectory.Add(@"c:\PDBs", false);
        }

        [FileUtilities.SourceModules(C.OutputFileFlags.LinkerProgramDatabase)]
        Opus.Core.TypeArray sourceTargets = new Opus.Core.TypeArray(
            typeof(Test4.MyDynamicLib),
            typeof(MyDynamicLibTestApp)
        );

        [FileUtilities.DestinationDirectoryPath(CreateDirectory=true)]
        Opus.Core.DirectoryCollection destinationDirectory = new Opus.Core.DirectoryCollection();
    }
}
