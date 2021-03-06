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
namespace Bam.Core
{
    /// <summary>
    /// Utility class to handle hashing of files and or general strings.
    /// </summary>
    public static class Hash
    {
        private static string
        GenerateHash(
            System.Collections.Generic.IEnumerable<string> filenames,
            System.Collections.Generic.IEnumerable<string> strings)
        {
            var provider = System.Security.Cryptography.SHA1.Create();

            if (null != filenames)
            {
                foreach (var path in filenames)
                {
                    using (var reader = new System.IO.StreamReader(path))
                    {
                        var contents = reader.ReadToEnd();
                        var contentsAsBytes = System.Text.Encoding.UTF8.GetBytes(contents);
                        provider.TransformBlock(
                            contentsAsBytes,
                            0,
                            contentsAsBytes.Length,
                            null,
                            0
                        );
                    }
                }
            }

            foreach (var str in strings)
            {
                var defineAsBytes = System.Text.Encoding.UTF8.GetBytes(str);
                provider.TransformBlock(
                    defineAsBytes,
                    0,
                    defineAsBytes.Length,
                    null,
                    0
                );
            }

            provider.TransformFinalBlock(new byte[0], 0, 0);

            var hash = provider.Hash;
            var hashAsString = System.Convert.ToBase64String(hash);
            return hashAsString;
        }

        private static string
        GenerateHash(
            string str)
        {
            System.Collections.Generic.IEnumerable<string> singleEnumerator()
            {
                yield return str;
            }
            return GenerateHash(null, singleEnumerator());
        }

        /// <summary>
        /// Result of hash comparisons.
        /// </summary>
        public enum EHashCompareResult
        {
            /// <summary>
            /// File containing the hash of the data does not exist. Hash file will be generated.
            /// </summary>
            HashFileDoesNotExist,

            /// <summary>
            /// Current and saved hash are different. Data has changed. Hash file will be written.
            /// </summary>
            HashesAreDifferent,

            /// <summary>
            /// Current and saved hash are identical. Data has not changed.
            /// </summary>
            HashesAreIdentical
        }

        /// <summary>
        /// Generate a hash from a single string, and compare with the hash stored in the specified file.
        /// If the hash file needs refreshing, it will be.
        /// </summary>
        /// <param name="hashFile">Path to the file containing the previous hash.</param>
        /// <param name="str">Single string to generate a hash for.</param>
        /// <returns>Result of the comparison.</returns>
        public static EHashCompareResult
        CompareAndUpdateHashFile(
            string hashFile,
            string str)
        {
            System.Collections.Generic.IEnumerable<string> singleEnumerator()
            {
                yield return str;
            }
            return CompareAndUpdateHashFile(
                hashFile,
                null,
                singleEnumerator()
            );
        }

        /// <summary>
        /// Generate a hash from a list of filenames and strings, and compare with the hash stored in the specified file.
        /// If the hash file needs refreshing, it will be.
        /// </summary>
        /// <param name="hashFile">Path to the file containing the previous hash.</param>
        /// <param name="filenames">List of filenames. Can be null.</param>
        /// <param name="strings">List of strings.</param>
        /// <returns></returns>
        public static EHashCompareResult
        CompareAndUpdateHashFile(
            string hashFile,
            System.Collections.Generic.IEnumerable<string> filenames,
            System.Collections.Generic.IEnumerable<string> strings)
        {
            var currentHash = GenerateHash(filenames, strings);
            EHashCompareResult result;
            if (!System.IO.File.Exists(hashFile))
            {
                result = EHashCompareResult.HashFileDoesNotExist;
            }
            else
            {
                using (var reader = new System.IO.StreamReader(hashFile))
                {
                    var diskHash = reader.ReadToEnd().TrimEnd();
                    Log.DebugMessage($"Disk {hashFile}: {diskHash}, current: {currentHash}");
                    if (diskHash.Equals(currentHash, System.StringComparison.Ordinal))
                    {
                        result = EHashCompareResult.HashesAreIdentical;
                    }
                    else
                    {
                        result = EHashCompareResult.HashesAreDifferent;
                    }
                }
            }
            if (EHashCompareResult.HashesAreIdentical != result)
            {
                // this will create the build root directory as necessary
                IOWrapper.CreateDirectory(System.IO.Path.GetDirectoryName(hashFile));
                using (var writer = new System.IO.StreamWriter(hashFile))
                {
                    writer.WriteLine(currentHash);
                }
            }
            Log.DebugMessage(result.ToString());
            return result;
        }
    }
}
