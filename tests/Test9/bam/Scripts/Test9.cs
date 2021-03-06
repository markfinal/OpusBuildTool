#region License
// Copyright (c) 2010-2019, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Test9
{
    sealed class CFile :
        C.ObjectFile
    {
        protected override void
        Init()
        {
            base.Init();

            (this as C.IRequiresSourceModule).Source = Bam.Core.Module.Create<C.SourceFile>(preInitCallback: module =>
            {
                module.InputPath = this.CreateTokenizedString("$(packagedir)/source/main_c.c");
            });
        }
    }

    sealed class CFileCollection :
        C.CObjectFileCollection
    {
        protected override void
        Init()
        {
            base.Init();

            this.AddFile("$(packagedir)/source/main_c.c");
        }
    }

    sealed class CppFile :
        C.Cxx.ObjectFile
    {
        protected override void
        Init()
        {
            base.Init();

            (this as C.IRequiresSourceModule).Source = Bam.Core.Module.Create<C.SourceFile>(preInitCallback: module =>
            {
                module.InputPath = this.CreateTokenizedString("$(packagedir)/source/main_cpp.c");
            });
            this.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICxxOnlyCompilerSettings;
                    compiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });
        }
    }

    // Note: Uses the C++ application module, in order to use the C++ linker, in order to link in C++ runtimes
    sealed class MixedLanguageApplication :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateHeaderCollection("$(packagedir)/include/*.h");

            var cSource = this.CreateCSourceCollection("$(packagedir)/source/library_c.c");
            cSource.PrivatePatch(settings =>
                {
                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    preprocessor.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                });

            var cxxSource = this.CreateCxxSourceCollection();
            cxxSource.AddFile("$(packagedir)/source/library_cpp.c");
            cxxSource.AddFile("$(packagedir)/source/appmain_cpp.c");
            cxxSource.PrivatePatch(settings =>
                {
                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    preprocessor.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                    var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });
        }
    }

    sealed class CStaticLibraryFromCollection :
        C.StaticLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateHeaderCollection("$(packagedir)/include/library_c.h");

            var source = this.CreateCSourceCollection("$(packagedir)/source/library_c.c");
            source.PrivatePatch(settings =>
                {
                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    preprocessor.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                });
        }
    }

    sealed class CppStaticLibaryFromCollection :
        C.StaticLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateHeaderCollection("$(packagedir)/include/library_cpp.h");

            var source = this.CreateCxxSourceCollection("$(packagedir)/source/library_cpp.c");
            source.PrivatePatch(settings =>
            {
                var preprocessor = settings as C.ICommonPreprocessorSettings;
                preprocessor.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));

                var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            });
        }
    }

    sealed class CDynamicLibraryFromCollection :
        C.DynamicLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetSemanticVersion(Bam.Core.Graph.Instance.ProcessState as Bam.Core.ISemanticVersion);
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("Test9: Example C dynamic library");

            this.CreateHeaderCollection("$(packagedir)/include/library_c.h");

            var source = this.CreateCSourceCollection("$(packagedir)/source/library_c.c");
            source.PrivatePatch(settings =>
                {
                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    preprocessor.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                });
        }
    }

    sealed class CppDynamicLibaryFromCollection :
        C.Cxx.DynamicLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetSemanticVersion(Bam.Core.Graph.Instance.ProcessState as Bam.Core.ISemanticVersion);
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("Test9: Example C++ dynamic library");

            this.CreateHeaderCollection("$(packagedir)/include/library_cpp.h");

            var source = this.CreateCxxSourceCollection("$(packagedir)/source/library_cpp.c");
            source.PrivatePatch(settings =>
            {
                var preprocessor = settings as C.ICommonPreprocessorSettings;
                preprocessor.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));

                var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            });
        }
    }
}
