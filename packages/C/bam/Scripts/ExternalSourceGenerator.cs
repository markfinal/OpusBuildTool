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
namespace C
{
    /// <summary>
    /// Generate source from an external program
    /// </summary>
    public class ExternalSourceGenerator :
        Bam.Core.Module
    {
        public ExternalSourceGenerator()
        {
            this.Arguments = new Bam.Core.TokenizedStringArray();
            this.InputFiles = new System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedString>();
            this.ExpectedOutputFiles = new Bam.Core.TokenizedStringArray();
        }

        /// <summary>
        /// Specify the path to the external executable to run.
        /// </summary>
        /// <value>The executable.</value>
        public Bam.Core.TokenizedString Executable
        {
            get;
            set;
        }

        /// <summary>
        /// Specify the list of arguments to invoke the executable with.
        /// </summary>
        /// <value>The arguments.</value>
        public Bam.Core.TokenizedStringArray Arguments
        {
            get;
            private set;
        }

        private System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedString>
        InputFiles
        {
            get;
            set;
        }

        public void
        AddInputFile(
            string name,
            Bam.Core.TokenizedString path)
        {
            if (this.InputFiles.ContainsKey(name))
            {
                throw new Bam.Core.Exception("Input file '{0}' has already been added", name);
            }
            this.InputFiles.Add(name, path);
            this.Macros.Add(name, path);
        }

        /// <summary>
        /// Gets the output directory.
        /// </summary>
        /// <value>The output directory.</value>
        public Bam.Core.TokenizedString OutputDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Specify the output directory to write files.
        /// Sets the macro 'ExternalSourceGenerator.OutputDir' to this path.
        /// </summary>
        /// <value>The output directory.</value>
        public void
        SetOutputDirectory(
            Bam.Core.TokenizedString dir_path)
        {
            this.OutputDirectory = dir_path;
            this.Macros.Add("ExternalSourceGenerator.OutputDir", dir_path);
        }

        /// <summary>
        /// Specify the output directory to write files.
        /// Sets the macro 'OutputDir_SourceGenerator' to this path.
        /// </summary>
        /// <value>The output directory.</value>
        public void
        SetOutputDirectory(
            string dir_path)
        {
            this.SetOutputDirectory(this.CreateTokenizedString(dir_path));
        }

        /// <summary>
        /// Specify the expected files to be written by running the executable.
        /// If these do not exist after running the executable, an exception
        /// will be thrown.
        /// </summary>
        /// <value>The expected output files.</value>
        public Bam.Core.TokenizedStringArray ExpectedOutputFiles
        {
            get;
            private set;
        }

        public override void Evaluate()
        {
            //throw new System.NotImplementedException();
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Executable)
            {
                throw new Bam.Core.Exception("No executable was specified");
            }
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(this.OutputDirectory.ToString());
            var program = this.Executable.ToStringQuoteIfNecessary();
            var arguments = this.Arguments.ToString(' ');
            Bam.Core.Log.Info("Running: {0} {1}", program, arguments);
            Bam.Core.OSUtilities.RunExecutable(program, arguments);
            foreach (var expected_file in this.ExpectedOutputFiles)
            {
                if (!System.IO.File.Exists(expected_file.ToString()))
                {
                    throw new Bam.Core.Exception("Expected '{0}' to exist, but it does not", expected_file.ToString());
                }
            }
        }

        protected override void GetExecutionPolicy(string mode)
        {
            //throw new System.NotImplementedException();
        }
    }
}
