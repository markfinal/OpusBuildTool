// Automatically generated by Opus v0.00
namespace Test3
{
    // Define module classes here
    sealed class Library2 : C.StaticLibrary
    {
        public Library2()
        {
            this.headerFiles.Include(this.PackageLocation, "include", "*.h");
        }

        sealed class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library2.c");
                this.UpdateOptions += SetIncludePaths;
            }

            [C.ExportCompilerOptionsDelegate]
            public void SetIncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Opus.Core.FileCollection headerFiles = new Opus.Core.FileCollection();
    }
}
