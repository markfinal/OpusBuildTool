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
namespace ClangCommon
{
    /// <summary>
    /// Abstract class for common Clang assembler settings
    /// </summary>
    [CommandLineProcessor.OutputPath(C.AssembledObjectFile.ObjectFileKey, "-o ")]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "-c ", max_file_count: 1)]
    abstract class CommonAssemblerSettings :
        C.SettingsBase,
        C.ICommonAssemblerSettings,
        C.ICommonAssemblerSettingsOSX,
        C.IAdditionalSettings,
        ICommonAssemblerSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected CommonAssemblerSettings()
            :
            base(ELayout.Cmds_Inputs_Outputs)
        {}

        protected override void
        ModifyDefaults()
        {
            base.ModifyDefaults();

            // not in the defaults in the C package to avoid a compile-time dependency on the Clang package
            (this as C.ICommonAssemblerSettingsOSX).MacOSXMinimumVersionSupported =
                Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang").MacOSXMinimumVersionSupported;
        }

        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-arch i386")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-arch x86_64")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.ThirtyTwo, "VALID_ARCHS", "i386", "ARCHS", "$(ARCHS_STANDARD_32_BIT)")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.SixtyFour, "VALID_ARCHS", "x86_64", "ARCHS", "$(ARCHS_STANDARD_64_BIT)")]
        C.EBit? C.ICommonAssemblerSettings.Bits { get; set; }

        [CommandLineProcessor.Bool("-g", "")]
        [XcodeProjectProcessor.UniqueBool("GCC_GENERATE_DEBUGGING_SYMBOLS", "YES", "NO")]
        bool C.ICommonAssemblerSettings.DebugSymbols { get; set; }

        [CommandLineProcessor.Bool("-Werror", "-Wno-error")]
        [XcodeProjectProcessor.UniqueBool("GCC_TREAT_WARNINGS_AS_ERRORS", "YES", "NO")]
        bool C.ICommonAssemblerSettings.WarningsAsErrors { get; set; }

        [CommandLineProcessor.PathArray("-I")]
        [XcodeProjectProcessor.PathArray("USER_HEADER_SEARCH_PATHS")]
        Bam.Core.TokenizedStringArray C.ICommonAssemblerSettings.IncludePaths { get; set; }

        [CommandLineProcessor.PreprocessorDefines("-D")]
        [XcodeProjectProcessor.PreprocessorDefines("GCC_PREPROCESSOR_DEFINITIONS")]
        C.PreprocessorDefinitions C.ICommonAssemblerSettings.PreprocessorDefines { get; set; }

        [CommandLineProcessor.String("-mmacosx-version-min=")]
        [XcodeProjectProcessor.String("", ignore: true)]
        string C.ICommonAssemblerSettingsOSX.MacOSXMinimumVersionSupported { get; set; }

        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.StringArray("OTHER_CFLAGS", spacesSeparate: true)]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }
    }
}
