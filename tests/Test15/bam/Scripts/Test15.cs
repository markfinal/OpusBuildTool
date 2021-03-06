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
namespace Test15
{
    sealed class StaticLibrary2 :
        C.StaticLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateHeaderCollection("$(packagedir)/include/staticlibrary2.h");
            var source = this.CreateCSourceCollection("$(packagedir)/source/staticlibrary2.c");
            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                    }
                });

            this.CompileAgainstPublicly<Test14.StaticLibrary1>(source);
        }
    }

    sealed class DynamicLibrary2 :
        C.DynamicLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetSemanticVersion(Bam.Core.Graph.Instance.ProcessState as Bam.Core.ISemanticVersion);
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("Test15: Example dynamic library");

            this.CreateHeaderCollection("$(packagedir)/include/dynamiclibrary2.h");
            var source = this.CreateCSourceCollection("$(packagedir)/source/dynamiclibrary2.c");
            this.PublicPatch((settings, appliedTo) =>
            {
                if (settings is C.ICommonPreprocessorSettings preprocessor)
                {
                    preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                    preprocessor.PreprocessorDefines.Add("D_PUBLIC_FORWARDING");
                }
            });

            // because DynamicLibrary1 pokes out of the public API of DynamicLibrary2 (see D_PUBLIC_FORWARDING),
            // the dependency has to be marked as 'public' so that forwarding occurs
            this.CompilePubliclyAndLinkAgainst<Test14.DynamicLibrary1>(source);
        }
    }

    sealed class DynamicLibrary2NonPublicForwarder :
        C.DynamicLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetSemanticVersion(Bam.Core.Graph.Instance.ProcessState as Bam.Core.ISemanticVersion);
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("Test15: Example dynamic library (non public forwarder)");

            this.CreateHeaderCollection("$(packagedir)/include/dynamiclibrary2.h");
            var source = this.CreateCSourceCollection("$(packagedir)/source/dynamiclibrary2.c");
            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                    }
                });

            // DynamicLibrary1 DOES NOT poke out of the public API of DynamicLibrary2, hence no Public in the
            // CompileAndLinkAgainst dependency below
            this.CompileAndLinkAgainst<Test14.DynamicLibrary1>(source);
        }
    }
}
