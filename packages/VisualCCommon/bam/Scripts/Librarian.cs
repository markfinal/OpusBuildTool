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
namespace VisualCCommon
{
    /// <summary>
    /// Abstract class representing any librarian
    /// </summary>
    abstract class LibrarianBase :
        C.LibrarianTool
    {
        private string
        GetLibrarianPath(
            C.EBit depth)
        {
            const string executable = "lib.exe";
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

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="depth">Of the given bitdepth</param>
        protected LibrarianBase(
            C.EBit depth)
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            var discovery = meta as C.IToolchainDiscovery;
            discovery.Discover(depth);
            this.Version = meta.ToolchainVersion;
            this.Macros.Add("InstallPath", meta.InstallDir);
            this.EnvironmentVariables = meta.Environment(depth);
            var fullLibExePath = this.GetLibrarianPath(depth);
            this.Macros.Add("ArchiverPath", Bam.Core.TokenizedString.CreateVerbatim(fullLibExePath));
            this.Macros.AddVerbatim(C.ModuleMacroNames.LibraryPrefix, string.Empty);
            this.Macros.AddVerbatim(C.ModuleMacroNames.LibraryFileExtension, ".lib");
        }

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(VisualC.ArchiverSettings);

        /// <summary>
        /// Path to the executable for the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros["ArchiverPath"];

        /// <summary>
        /// Command line switch used for response files
        /// </summary>
        public override string UseResponseFileOption => "@";
    }

    /// <summary>
    /// Class representing a 32-bit VisualC librarian
    /// </summary>
    [C.RegisterLibrarian("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    sealed class Librarian32 :
        LibrarianBase
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        public Librarian32()
            :
            base(C.EBit.ThirtyTwo)
        { }
    }

    /// <summary>
    /// Class representing a 64-bit VisualC librarian
    /// </summary>
    [C.RegisterLibrarian("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    sealed class Librarian64 :
        LibrarianBase
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        public Librarian64()
            :
            base(C.EBit.SixtyFour)
        { }
    }
}
