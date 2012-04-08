// Automatically generated by Opus v0.00
namespace Test12
{
    // Define module classes here
    class MyOpenGLApplication : C.WindowsApplication
    {
        class CommonSourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public CommonSourceFiles()
            {
                this.Include(this, "source", "main.cpp");
            }
        }

        class WindowsSourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public WindowsSourceFiles()
            {
                this.Include(this, "source", "win", "win.cpp");
            }
        }

        class UnixSourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public UnixSourceFiles()
            {
                this.Include(this, "source", "unix", "unix.cpp");
            }
        }

        class OSXSourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public OSXSourceFiles()
            {
                this.Include(this, "source", "osx", "osx.cpp");
            }
        }
    
        [Opus.Core.SourceFiles]
        CommonSourceFiles commonSourceFiles = new CommonSourceFiles();
        [Opus.Core.SourceFiles(Platform=Opus.Core.EPlatform.Windows)]
        WindowsSourceFiles windowsSourceFiles = new WindowsSourceFiles();
        [Opus.Core.SourceFiles(Platform=Opus.Core.EPlatform.Unix)]
        UnixSourceFiles unixSourceFiles = new UnixSourceFiles();
        [Opus.Core.SourceFiles(Platform=Opus.Core.EPlatform.OSX)]
        OSXSourceFiles osxSourceFiles = new OSXSourceFiles();

        [Opus.Core.DependentModules(Platform=Opus.Core.EPlatform.Windows, Toolchains=new string[] { "visualc" })]
        Opus.Core.TypeArray windowsVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
        Opus.Core.StringArray windowsVCLibraries = new Opus.Core.StringArray(
            "KERNEL32.lib",
            "USER32.lib"
        );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Unix)]
        Opus.Core.StringArray unixLibraries = new Opus.Core.StringArray(
            "-lX11"
        );
    }
}
