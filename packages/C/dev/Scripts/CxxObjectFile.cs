#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace C.Cxx
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this C.V2.ICxxOnlyCompilerOptions settings,
            Bam.Core.V2.Module module)
        {
            settings.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
            settings.LanguageStandard = ELanguageStandard.Cxx98;
        }
        public static void
        Delta(
            this C.V2.ICxxOnlyCompilerOptions settings,
            C.V2.ICxxOnlyCompilerOptions delta,
            C.V2.ICxxOnlyCompilerOptions other)
        {
            if (settings.ExceptionHandler != other.ExceptionHandler)
            {
                delta.ExceptionHandler = settings.ExceptionHandler;
            }
            if (settings.LanguageStandard != other.LanguageStandard)
            {
                delta.LanguageStandard = settings.LanguageStandard;
            }
        }
        public static void
        Clone(
            this C.V2.ICxxOnlyCompilerOptions settings,
            C.V2.ICxxOnlyCompilerOptions other)
        {
            settings.ExceptionHandler = other.ExceptionHandler;
            settings.LanguageStandard = other.LanguageStandard;
        }
    }
}
    public class ObjectFile :
        C.V2.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            // TODO: shouldn't attempt to find the default C compiler if only C++ is of interest
            this.Compiler = C.V2.DefaultToolchain.Cxx_Compiler(this.BitDepth);
        }
    }
}
    /// <summary>
    /// C++ object file
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(ICxxCompilerTool))]
    public class ObjectFile :
        C.ObjectFile
    {}
}
