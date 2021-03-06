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
namespace XcodeBuilder
{
    /// <summary>
    /// Class corresponding to the PBXBuildFile object in Xcode projects.
    /// </summary>
    sealed class BuildFile :
        Object
    {
        /// <summary>
        /// Construct an instance of a BuildFile.
        /// </summary>
        /// <param name="fileRef">FileReference corresponding to the BuildFile.</param>
        /// <param name="target">Target to which the BuildFile is added.</param>
        public BuildFile(
            FileReference fileRef,
            Target target)
            :
            base(target.Project, System.IO.Path.GetFileName(fileRef.Path.ToString()), "PBXBuildFile", fileRef.GUID, target.GUID)
        {
            this.FileRef = fileRef;
            this.OwningTarget = target;
        }

        /// <summary>
        /// Get the owning Target of the BuildFile.
        /// </summary>
        public Target OwningTarget { get; private set; }

        /// <summary>
        /// Get the FileReference of the BuildFile.
        /// </summary>
        public FileReference FileRef { get; private set; }

        /// <summary>
        /// Get or set the Settings on the BuildFile.
        /// </summary>
        public Bam.Core.StringArray Settings { get; set; }

        /// <summary>
        /// Get or set the parent object of this BuildFile.
        /// </summary>
        public Object Parent { get; set; }

        /// <summary>
        /// Serialize the BuildFile.
        /// </summary>
        /// <param name="text">The StringBuilder to write to.</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var parentName = (null != this.Parent) ? this.Parent.Name : "Unknown";
            text.Append($"{indent}{this.GUID} ");
            text.Append($"/* {this.Name} in {parentName} */ ");
            text.Append($"= {{isa = {this.IsA}; fileRef = {this.FileRef.GUID} /* {this.Name} */; ");
            if (this.Settings != null)
            {
                // any requirements for extra escape characters for escaped quotes are handled
                // in the native command line translation, by detecting the build mode is Xcode
                text.Append($"settings = {{COMPILER_FLAGS = \"{this.CleansePaths(this.Settings.ToString(' '))}\"; }}; ");
            }
            text.AppendLine($"{indent}}};");
        }
    }
}
