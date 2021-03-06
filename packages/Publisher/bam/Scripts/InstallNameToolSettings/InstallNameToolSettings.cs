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
namespace Publisher
{
    /// <summary>
    /// Class representing install_name_tool tool settings
    /// </summary>
    [CommandLineProcessor.InputPaths(C.ConsoleApplication.ExecutableKey, "")]
    sealed class InstallNameToolSettings :
        Bam.Core.Settings,
        IInstallNameToolSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InstallNameToolSettings()
            :
            base(ELayout.Cmds_Inputs_Outputs)
        {}

        [CommandLineProcessor.Enum(EInstallNameToolMode.UpdateIDName, "-id")]
        [CommandLineProcessor.Enum(EInstallNameToolMode.ChangeIDName, "-change")]
        EInstallNameToolMode IInstallNameToolSettings.Mode { get; set; }

        [CommandLineProcessor.String("")]
        string IInstallNameToolSettings.OldName { get; set; }

        [CommandLineProcessor.String("")]
        string IInstallNameToolSettings.NewName { get; set; }

        public override void
        Validate()
        {
            base.Validate();

            var install_name_tool = this as IInstallNameToolSettings;
            switch (install_name_tool.Mode)
            {
                case EInstallNameToolMode.UpdateIDName:
                    {
                        if (null != install_name_tool.OldName)
                        {
                            throw new Bam.Core.Exception("Must not specify an old name for Id mode");
                        }
                        if (null == install_name_tool.NewName)
                        {
                            throw new Bam.Core.Exception("Must specify a new name for Id mode");
                        }
                    }
                    break;

                case EInstallNameToolMode.ChangeIDName:
                    {
                        if (null == install_name_tool.OldName)
                        {
                            throw new Bam.Core.Exception("Must specify an old name for Change mode");
                        }
                        if (null == install_name_tool.NewName)
                        {
                            throw new Bam.Core.Exception("Must specify an old name for Change mode");
                        }
                    }
                    break;
            }
        }
    }
}
