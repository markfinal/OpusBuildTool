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
namespace C
{
    /// <summary>
    /// Common preprocessor settings available on all toolchains
    /// </summary>
    [Bam.Core.SettingsExtensions(typeof(C.DefaultSettings.DefaultSettingsExtensions))]
    interface ICommonPreprocessorSettings :
        Bam.Core.ISettingsBase
    {
        /// <summary>
        /// List of preprocessor defines, of the form key or key=value.
        /// </summary>
        /// <value>The preprocessor defines.</value>
        C.PreprocessorDefinitions PreprocessorDefines { get; set; }

        /// <summary>
        /// List of paths to search for user headers, i.e. those quoted with double quotes.
        /// </summary>
        /// <value>The include paths.</value>
        Bam.Core.TokenizedStringArray IncludePaths { get; set; }

        /// <summary>
        /// List of paths to search for system headers, i.e. those quoted with angle brackets.
        /// </summary>
        /// <value>The system include paths.</value>
        Bam.Core.TokenizedStringArray SystemIncludePaths { get; set; }

        /// <summary>
        /// List of preprocessor definitions to undefine during compilation.
        /// </summary>
        /// <value>The preprocessor undefines.</value>
        Bam.Core.StringArray PreprocessorUndefines { get; set; }

        /// <summary>
        /// Compile for a particular language, C, C++, ObjectiveC or ObjectiveC++.
        /// </summary>
        /// <value>The target language.</value>
        C.ETargetLanguage? TargetLanguage { get; set; }

        /// <summary>
        /// Suppress line markers generated into the preprocessed output.
        /// </summary>
        /// <value>Whether line markers are generated or not.</value>
        bool? SuppressLineMarkers { get; set; }
    }
}
