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
namespace WindowsSDK
{
    /// <summary>
    /// Base class for all versions of the compiler tool
    /// </summary>
    abstract class WinResourceCompilerBase :
        C.WinResourceCompilerTool
    {
        /// <summary>
        /// Configure the tool.
        /// </summary>
        /// <param name="architecture">Architecture in use</param>
        protected void
        Configure(
            string architecture)
        {
            // WindowsSDK 10 has a bin folder in different places depending on the version (pre or post VS2017)
            // If the envvar WindowsSdkVerBinPath exists, then the bin folder is versioned (VS2017+)
            // If not, then it's just the bin folder (VS2015)
            if (this.EnvironmentVariables.ContainsKey("WindowsSdkVerBinPath"))
            {
                var tokenised_strings = new Bam.Core.TokenizedStringArray();
                tokenised_strings.AddRangeUnique(this.EnvironmentVariables["WindowsSdkVerBinPath"]);
                this.Macros.Add(
                    "CompilerPath",
                    Bam.Core.TokenizedString.Create(
                        $"$(0)/{architecture}/rc.exe",
                        null,
                        tokenised_strings
                    )
                );
            }
            else if (this.EnvironmentVariables.ContainsKey("WindowsSdkDir"))
            {
                var tokenised_strings = new Bam.Core.TokenizedStringArray();
                tokenised_strings.AddRangeUnique(this.EnvironmentVariables["WindowsSdkDir"]);
                this.Macros.Add(
                    "CompilerPath",
                    Bam.Core.TokenizedString.Create(
                        $"$(0)/bin/{architecture}/rc.exe",
                        null,
                        tokenised_strings
                    )
                );
            }
            else
            {
                throw new Bam.Core.Exception(
                    "Unable to determine resource compiler path, as neither %WindowsSdkVerBinPath% nor %WindowsSdkDir% were defined"
                );
            }
            this.Macros.AddVerbatim(C.ModuleMacroNames.ObjectFileExtension, ".res");
        }

        /// <summary>
        /// Get the executable path to the compiler
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros["CompilerPath"];

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(WinResourceCompilerSettings);
    }

    /// <summary>
    /// 32-bit resource compiler
    /// </summary>
    [C.RegisterWinResourceCompiler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    sealed class WinResourceCompiler32 :
        WinResourceCompilerBase
    {
        /// <summary>
        /// Initialize the module
        /// </summary>
        protected override void
        Init()
        {
            var vcMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.EnvironmentVariables = vcMeta.Environment(C.EBit.ThirtyTwo);
            this.Configure("x86");
            // now check the executable exists
            base.Init();
        }
    }

    /// <summary>
    /// 64-bit resource compiler
    /// </summary>
    [C.RegisterWinResourceCompiler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    sealed class WinResourceCompiler64 :
        WinResourceCompilerBase
    {
        /// <summary>
        /// Initialize the module
        /// </summary>
        protected override void
        Init()
        {
            var vcMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.EnvironmentVariables = vcMeta.Environment(C.EBit.SixtyFour);
            this.Configure("x64");
            // now check the executable exists
            base.Init();
        }
    }
}
