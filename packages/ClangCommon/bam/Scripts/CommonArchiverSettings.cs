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
    [CommandLineProcessor.OutputPath(C.StaticLibrary.LibraryKey, "")]
    [CommandLineProcessor.InputPaths(C.ObjectFileBase.ObjectFileKey, "")]
    public abstract class CommonArchiverSettings :
        C.SettingsBase,
        C.IAdditionalSettings,
        ICommonArchiverSettings
    {
        /// <summary>
        /// Create a settings instance
        /// </summary>
        /// <param name="module">for this Module</param>
        protected CommonArchiverSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        [CommandLineProcessor.Bool("-s", "")]
        [XcodeProjectProcessor.UniqueBool("", "", "", ignore: true)]
        bool ICommonArchiverSettings.Ranlib { get; set; }

        [CommandLineProcessor.Bool("-c", "")]
        [XcodeProjectProcessor.UniqueBool("", "", "", ignore: true)]
        bool ICommonArchiverSettings.DoNotWarnIfLibraryCreated { get; set; }

        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.StringArray("", ignore: true)]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }

        [CommandLineProcessor.Enum(EArchiverCommand.Replace, "-r")]
        [XcodeProjectProcessor.UniqueEnum(EArchiverCommand.Replace, "", "", ignore: true)]
        EArchiverCommand ICommonArchiverSettings.Command { get; set; }

        /// <summary>
        /// Set the layout how command lines are constructed
        /// </summary>
        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}
