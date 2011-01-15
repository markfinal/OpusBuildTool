// Automatically generated by Opus v0.00
namespace Test10
{
    class MyStaticLibrary : C.StaticLibrary
    {
        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile("source", "stlib.c");
    }

    class MyDynamicLibrary : C.DynamicLibrary
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile("source", "dylib.c");

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }

    class MyStandaloneApp : C.Application
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile("source", "standaloneapp.c");

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(MyStaticLibrary)
        );

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray windowsDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }

    class DllDependentApp : C.Application
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile("source", "dlldependentapp.c");

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(MyDynamicLibrary)
        );

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray windowsDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }
}
