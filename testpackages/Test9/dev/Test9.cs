// Automatically generated by Opus v0.00
namespace Test9
{
    // Define module classes here
    class CFile : C.ObjectFile
    {
        public CFile()
            : base("source", "main_c.c")
        {
        }
    }

    class CFileCollection : C.ObjectFileCollection
    {
        public CFileCollection()
        {
            this.Add("source", "main_c.c");
        }
    }

    class CppFile : C.CPlusPlus.ObjectFile
    {
        public CppFile()
            : base("source", "main_cpp.c")
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(CppFile_UpdateOptions);
        }

        void CppFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICPlusPlusCompilerOptions compilerOptions = module.Options as C.ICPlusPlusCompilerOptions;
            compilerOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Synchronous;
        }
    }

    class CppFileCollection : C.CPlusPlus.ObjectFileCollection
    {
        public CppFileCollection()
        {
            this.Add("source", "main_cpp.c");

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(CppFileCollection_UpdateOptions);
        }

        void CppFileCollection_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICPlusPlusCompilerOptions compilerOptions = module.Options as C.ICPlusPlusCompilerOptions;
            compilerOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Synchronous;
        }
    }

    class MixedLanguageApplication : C.Application
    {
        public MixedLanguageApplication()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SetSystemLibraries);
        }

        static void SetSystemLibraries(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            if (Opus.Core.OSUtilities.IsWindows(target.Platform))
            {
                if (linkerOptions is VisualC.LinkerOptionCollection)
                {
                    linkerOptions.Libraries.Add("KERNEL32.lib");
                }
            }
        }

        static void IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
        }

        class CSourceFiles : C.ObjectFileCollection
        {
            public CSourceFiles()
            {
                this.Add("source", "library_c.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(IncludePaths);
            }
        }

        class CPlusPlusSourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public CPlusPlusSourceFiles()
            {
                this.Add("source", "library_cpp.c");
                this.Add("source", "appmain_cpp.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(CPlusPlusSourceFiles_UpdateOptions);
                this.UpdateOptions += IncludePaths;
            }

            void CPlusPlusSourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICPlusPlusCompilerOptions compilerOptions = module.Options as C.ICPlusPlusCompilerOptions;
                compilerOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Synchronous;
            }
        }

        [Opus.Core.SourceFiles]
        CSourceFiles cSourceFiles = new CSourceFiles();
        [Opus.Core.SourceFiles]
        CPlusPlusSourceFiles cppSourceFiles = new CPlusPlusSourceFiles();

        [Opus.Core.DependentModules(new string[] { "win.*-.*-visualc" })]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );
    }

    class CStaticLibraryFromFile : C.StaticLibrary
    {
        public CStaticLibraryFromFile()
        {
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
        }

        void sourceFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile("source", "library_c.c");
    }

    class CStaticLibraryFromCollection : C.StaticLibrary
    {
        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Add("source", "library_c.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();
    }

    class CppStaticLibraryFromFile : C.StaticLibrary
    {
        public CppStaticLibraryFromFile()
        {
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_ExceptionHandling);
        }

        void sourceFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
        }

        void sourceFile_ExceptionHandling(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICPlusPlusCompilerOptions compilerOptions = module.Options as C.ICPlusPlusCompilerOptions;
            compilerOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Synchronous;
        }

        [Opus.Core.SourceFiles]
        C.CPlusPlus.ObjectFile sourceFile = new C.CPlusPlus.ObjectFile("source", "library_cpp.c");
    }

    class CppStaticLibaryFromCollection : C.StaticLibrary
    {
        class SourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Add("source", "library_cpp.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_ExceptionHandling);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
            }

            void SourceFiles_ExceptionHandling(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICPlusPlusCompilerOptions compilerOptions = module.Options as C.ICPlusPlusCompilerOptions;
                compilerOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Synchronous;
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();
    }

    class CDynamicLibraryFromFile : C.DynamicLibrary
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        public CDynamicLibraryFromFile()
        {
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
        }

        void sourceFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile("source", "library_c.c");

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }

    class CDynamicLibraryFromCollection : C.DynamicLibrary
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Add("source", "library_c.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }

    class CppDynamicLibraryFromFile : C.DynamicLibrary
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        public CppDynamicLibraryFromFile()
        {
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_ExceptionHandling);
        }

        void sourceFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
        }

        void sourceFile_ExceptionHandling(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICPlusPlusCompilerOptions compilerOptions = module.Options as C.ICPlusPlusCompilerOptions;
            compilerOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Synchronous;
        }

        [Opus.Core.SourceFiles]
        C.CPlusPlus.ObjectFile sourceFile = new C.CPlusPlus.ObjectFile("source", "library_cpp.c");

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }

    class CppDynamicLibaryFromCollection : C.DynamicLibrary
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        class SourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Add("source", "library_cpp.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_ExceptionHandling);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test9"], @"include");
            }

            void SourceFiles_ExceptionHandling(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICPlusPlusCompilerOptions compilerOptions = module.Options as C.ICPlusPlusCompilerOptions;
                compilerOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Synchronous;
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }
}
