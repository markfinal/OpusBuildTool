// Automatically generated by BuildAMation v0.60
[assembly:Bam.Core.GlobalOptionCollectionOverride(typeof(Cxx11Test1.GlobalSettings))]

namespace Cxx11Test1
{
    class GlobalSettings : Bam.Core.IGlobalOptionCollectionOverride
    {
        #region IGlobalOptionCollectionOverride implementation
        void
        Bam.Core.IGlobalOptionCollectionOverride.OverrideOptions(
            Bam.Core.BaseOptionCollection optionCollection,
            Bam.Core.Target target)
        {
            var cOptions = optionCollection as C.ICCompilerOptions;
            if (null != cOptions)
            {
                cOptions.LanguageStandard = C.ELanguageStandard.Cxx11;

                var cxxOptions = optionCollection as C.ICxxCompilerOptions;
                if (null != cxxOptions)
                {
                    cxxOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
                }
            }
        }
        #endregion
    }

    class TestProg : C.Application
    {
        class Source : C.Cxx.ObjectFileCollection
        {
            public Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows)]
        Bam.Core.TypeArray windowsDeps = new Bam.Core.TypeArray() {
            typeof(WindowsSDK.WindowsSDK)
        };
    }
}
