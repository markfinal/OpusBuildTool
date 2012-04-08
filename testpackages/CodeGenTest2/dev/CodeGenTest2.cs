// Automatically generated by Opus v0.00
namespace CodeGenTest2
{
    // Define module classes here
    class TestAppGeneratedSource : CodeGenModule
    {
        public TestAppGeneratedSource()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(TestAppGeneratedSource_UpdateOptions);
        }

        void TestAppGeneratedSource_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            CodeGenOptions options = module.Options as CodeGenOptions;
        }
    }

    class TestApp : C.Application
    {
        public TestApp()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(TestApp_UpdateOptions);
        }

        void TestApp_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions options = module.Options as C.ILinkerOptions;
            options.DoNotAutoIncludeStandardLibraries = false;
        }

        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Include(this, "source", "testapp", "main.c");
            }

            [Opus.Core.DependentModules]
            Opus.Core.TypeArray vcDependencies = new Opus.Core.TypeArray(typeof(TestAppGeneratedSource));
        }

        [Opus.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [Opus.Core.DependentModules(Platform=Opus.Core.EPlatform.Windows, Toolchains=new string[] { "visualc"})]
        Opus.Core.TypeArray vcDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
