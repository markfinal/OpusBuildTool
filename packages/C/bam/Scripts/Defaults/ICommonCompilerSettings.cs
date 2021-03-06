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
using Bam.Core;
namespace C.DefaultSettings
{
    /// <summary>
    /// Extension class for handling defaults and operations for C.ICommonCompilerSettings
    /// </summary>
    static partial class DefaultSettingsExtensions
    {
        /// <summary>
        /// Set default property values of C.ICommonCompilerSettings
        /// </summary>
        /// <param name="settings">C.ICommonCompilerSettings instance</param>
        public static void
        Defaults(
            this C.ICommonCompilerSettings settings)
        {
            var module = (settings as Bam.Core.Settings).Module;
            settings.Bits = (module as CModule).BitDepth;
            settings.DebugSymbols = (0 != (module.BuildEnvironment.Configuration & (Bam.Core.EConfiguration.Debug | Bam.Core.EConfiguration.Profile)));
            settings.Optimization = (0 != (module.BuildEnvironment.Configuration & Bam.Core.EConfiguration.NotDebug)) ? EOptimization.Speed : EOptimization.Off;
            settings.OmitFramePointer = (0 != (module.BuildEnvironment.Configuration & Bam.Core.EConfiguration.NotDebug));
            settings.WarningsAsErrors = true;
            settings.DisableWarnings = new Bam.Core.StringArray();
            settings.NamedHeaders = new Bam.Core.StringArray();
            settings.PreprocessOnly = false;
        }

        /// <summary>
        /// Intersection of two C.ICommonCompilerSettings
        /// </summary>
        /// <param name="shared">C.ICommonCompilerSettings instance for the shared properties</param>
        /// <param name="other">C.ICommonCompilerSettings instance to intersect with</param>
        public static void
        Intersect(
            this C.ICommonCompilerSettings shared,
            C.ICommonCompilerSettings other)
        {
            shared.Bits = shared.Bits.Intersect(other.Bits);
            shared.DebugSymbols = shared.DebugSymbols.Intersect(other.DebugSymbols);
            shared.WarningsAsErrors = shared.WarningsAsErrors.Intersect(other.WarningsAsErrors);
            shared.Optimization = shared.Optimization.Intersect(other.Optimization);
            shared.OmitFramePointer = shared.OmitFramePointer.Intersect(other.OmitFramePointer);
            shared.DisableWarnings = shared.DisableWarnings.Intersect(other.DisableWarnings);
            shared.NamedHeaders = shared.NamedHeaders.Intersect(other.NamedHeaders);
            shared.PreprocessOnly = shared.PreprocessOnly.Intersect(other.PreprocessOnly);
        }

        /// <summary>
        /// Delta between two C.ICommonCompilerSettings
        /// </summary>
        /// <param name="delta">C.ICommonCompilerSettings to write the delta to</param>
        /// <param name="lhs">C.ICommonCompilerSettings to diff against</param>
        /// <param name="rhs">C.ICommonCompilerSettings to diff with</param>
        public static void
        Delta(
            this C.ICommonCompilerSettings delta,
            C.ICommonCompilerSettings lhs,
            C.ICommonCompilerSettings rhs)
        {
            delta.Bits = lhs.Bits.Complement(rhs.Bits);
            delta.DebugSymbols = lhs.DebugSymbols.Complement(rhs.DebugSymbols);
            delta.WarningsAsErrors = lhs.WarningsAsErrors.Complement(rhs.WarningsAsErrors);
            delta.Optimization = lhs.Optimization.Complement(rhs.Optimization);
            delta.OmitFramePointer = lhs.OmitFramePointer.Complement(rhs.OmitFramePointer);
            delta.DisableWarnings = lhs.DisableWarnings.Complement(rhs.DisableWarnings);
            delta.NamedHeaders = lhs.NamedHeaders.Complement(rhs.NamedHeaders);
            delta.PreprocessOnly = lhs.PreprocessOnly.Complement(rhs.PreprocessOnly);
        }

        /// <summary>
        /// Clone a C.ICommonCompilerSettings
        /// </summary>
        /// <param name="settings">C.ICommonCompilerSettings containing the cloned properties</param>
        /// <param name="other">Source C.ICommonCompilerSettings</param>
        public static void
        Clone(
            this C.ICommonCompilerSettings settings,
            C.ICommonCompilerSettings other)
        {
            settings.Bits = other.Bits;
            settings.DebugSymbols = other.DebugSymbols;
            settings.WarningsAsErrors = other.WarningsAsErrors;
            settings.Optimization = other.Optimization;
            settings.OmitFramePointer = other.OmitFramePointer;
            settings.DisableWarnings = new Bam.Core.StringArray();
            foreach (var path in other.DisableWarnings)
            {
                settings.DisableWarnings.AddUnique(path);
            }
            settings.NamedHeaders = new Bam.Core.StringArray();
            foreach (var header in other.NamedHeaders)
            {
                settings.NamedHeaders.AddUnique(header);
            }
            settings.PreprocessOnly = other.PreprocessOnly;
        }
    }
}
