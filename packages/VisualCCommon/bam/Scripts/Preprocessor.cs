#region License
// Copyright (c) 2010-2018, Mark Final
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
using System.Linq;
namespace VisualCCommon
{
    public abstract class PreprocessorBase :
        C.PreprocessorTool
    {
        private string
        GetCompilerPath(
            C.EBit depth)
        {
            const string executable = "cl.exe";
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                var installLocation = Bam.Core.OSUtilities.GetInstallLocation(
                    executable,
                    path.ToString(),
                    this.GetType().Name,
                    throwOnFailure: false
                );
                if (null != installLocation)
                {
                    return installLocation.First();
                }
            }
            var message = new System.Text.StringBuilder();
            message.AppendLine($"Unable to locate {executable} for {(int)depth}-bit on these search locations:");
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                message.AppendLine($"\t{path.ToString()}");
            }
            throw new Bam.Core.Exception(message.ToString());
        }

        protected PreprocessorBase(
            C.EBit depth)
        {
            this.Macros.AddVerbatim("objext", ".obj");

            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            var discovery = meta as C.IToolchainDiscovery;
            discovery.discover(depth);
            this.Version = meta.ToolchainVersion;
            this.Macros.Add("InstallPath", meta.InstallDir);
            this.EnvironmentVariables = meta.Environment(depth);
            var fullCompilerExePath = this.GetCompilerPath(depth);
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.CreateVerbatim(fullCompilerExePath));

            this.InheritedEnvironmentVariables.Add("SystemRoot");
            // temp environment variables avoid generation of _CL_<hex> temporary files in the current directory
            this.InheritedEnvironmentVariables.Add("TEMP");
            this.InheritedEnvironmentVariables.Add("TMP");
        }

        public override Bam.Core.TokenizedString Executable => this.Macros["CompilerPath"];

        public override string UseResponseFileOption => "@";
    }

    [C.RegisterPreprocessor("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public class Preprocessor32 :
        PreprocessorBase
    {
        public Preprocessor32()
            :
            base(C.EBit.ThirtyTwo)
        {}

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new VisualC.PreprocessorSettings(module);
    }

    [C.RegisterPreprocessor("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    public class Preprocessor64 :
        PreprocessorBase
    {
        public Preprocessor64()
            :
            base(C.EBit.SixtyFour)
        { }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new VisualC.PreprocessorSettings(module);
    }
}