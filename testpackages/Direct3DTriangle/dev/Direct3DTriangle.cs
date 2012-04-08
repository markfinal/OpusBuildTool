// Automatically generated by Opus v0.00
namespace Direct3DTriangle
{
    // Define module classes here
    [Opus.Core.ModuleTargets(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
    class D3D9TriangleTest : C.WindowsApplication
    {
        public D3D9TriangleTest()
        {
            this.headerFiles.Include(this, "source", "*.h");
        }

        class SourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Include(this, "source", "*.cpp");

                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_VCDefines);
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_EnableException);
            }

            void SourceFiles_EnableException(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICPlusPlusCompilerOptions compilerOptions = module.Options as C.ICPlusPlusCompilerOptions;
                compilerOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Asynchronous;
            }

            void SourceFiles_VCDefines(Opus.Core.IModule module, Opus.Core.Target target)
            {
                if (module.Options is VisualCCommon.ICCompilerOptions)
                {
                    C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                    compilerOptions.Defines.Add("_CRT_SECURE_NO_WARNINGS");
                }
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Opus.Core.FileCollection headerFiles = new Opus.Core.FileCollection();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(DirectXSDK.Direct3D9),
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(Platform=Opus.Core.EPlatform.Windows, Toolchains=new string[] { "visualc" })]
        Opus.Core.StringArray winVCLibraries = new Opus.Core.StringArray(
            "KERNEL32.lib",
            "USER32.lib",
            "d3d9.lib",
            "dxerr.lib",
            "d3dx9.lib"
        );
    }
}
