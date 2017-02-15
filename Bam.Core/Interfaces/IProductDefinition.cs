#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace Bam.Core
{
    /// <summary>
    /// Interface to allow users to define a 'product' in their package builds.
    /// </summary>
    public interface IProductDefinition
    {
        /// <summary>
        /// Name of the software product.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Major version number of the software product, assuming semantic versioning.
        /// </summary>
        int? MajorVersion
        {
            get;
        }

        /// <summary>
        /// Minor version number of the software product, assuming semantic versioning.
        /// </summary>
        int? MinorVersion
        {
            get;
        }

        /// <summary>
        /// Patch version number of the software product, assuming semantic versioning.
        /// </summary>
        int? PatchVersion
        {
            get;
        }

        /// <summary>
        /// Is the software product version a pre-release?
        /// </summary>
        bool IsPrerelease
        {
            get;
        }

        /// <summary>
        /// Copyright notice of the software product
        /// </summary>
        string CopyrightNotice
        {
            get;
        }

        /// <summary>
        /// Name of the company that wrote/owns the software product
        /// </summary>
        string CompanyName
        {
            get;
        }
    }
}