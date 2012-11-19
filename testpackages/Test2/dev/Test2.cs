// Automatically generated by Opus v0.00
namespace Test2
{
    // Define module classes here
    sealed class Library : C.StaticLibrary
    {
        sealed class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Include(this, "source", "library.c");
                this.UpdateOptions += SetIncludePaths;
            }

            [C.ExportCompilerOptionsDelegate]
            public void SetIncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this, "include");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();
    }

    sealed class Application : C.Application
    {
        sealed class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Include(this, "source", "application.c");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(Library),
            typeof(Test3.Library2)
        );

        [Opus.Core.DependentModules(Platform=Opus.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }
}
