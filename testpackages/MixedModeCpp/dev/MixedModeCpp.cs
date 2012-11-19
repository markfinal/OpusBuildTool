// Automatically generated by Opus v0.00
namespace MixedModeCpp
{
    // Define module classes here
    [Opus.Core.ModuleTargets(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
    class TestApplication : C.Application
    {
        public TestApplication()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(TestApplication_UpdateOptions);
        }

        void TestApplication_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions options = module.Options as C.ILinkerOptions;
            options.DoNotAutoIncludeStandardLibraries = false;
        }

        class SourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Include(this, "source", "native.cpp");
            }
        }

        class ManagedSourceFiles : VisualCCommon.ManagedCPlusPlusObjectFileCollection
        {
            public ManagedSourceFiles()
            {
                this.Include(this, "source", "managed.cpp");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles nativeSourceFiles = new SourceFiles();

        [Opus.Core.SourceFiles]
        ManagedSourceFiles managedSourceFiles = new ManagedSourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependentModules = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib",
            "mscoree.lib"
        );
    }
}
