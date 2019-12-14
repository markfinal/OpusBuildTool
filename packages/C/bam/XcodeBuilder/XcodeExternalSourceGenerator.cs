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
using System.Linq;
namespace C
{
    /// <summary>
    /// Utility class offering support for Xcode project generation
    /// </summary>
    static partial class XcodeSupport
    {
        /// <summary>
        /// Add pre build commands to an Xcode target for generating source
        /// </summary>
        /// <param name="module">Module that generates source</param>
        public static void
        GenerateSource(
            ExternalSourceGenerator module)
        {
            var encapsulating = module.EncapsulatingModule;

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var project = workspace.EnsureProjectExists(encapsulating, encapsulating.PackageDefinition.FullName);
            var target = workspace.EnsureTargetExists(encapsulating, project);
            if (encapsulating == module)
            {
                target.SetType(XcodeBuilder.Target.EProductType.Utility);
            }
            var configuration = target.GetConfiguration(encapsulating);
            if (encapsulating == module)
            {
                configuration.SetProductName(Bam.Core.TokenizedString.CreateVerbatim("${TARGET_NAME}"));
            }

            var cmd_line = $"{module.Executable.ToStringQuoteIfNecessary()} {module.Arguments.ToString(' ')}";
            XcodeBuilder.Support.AddPreBuildCommands(
                module,
                configuration,
                cmd_line,
                new Bam.Core.TokenizedStringArray(module.ExpectedOutputFiles.Values)
            );
        }
    }
}
