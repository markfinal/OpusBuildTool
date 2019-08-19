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
namespace Bam.Core
{
    /// <summary>
    /// Static utility class representing all statistics for the build
    /// </summary>
    public static class Statistics
    {
        private static double
        BytesToMegaBytes(
            long bytes) => bytes / 1024.0 / 1024.0;

        /// <summary>
        /// Display the statistics for the build.
        /// </summary>
        public static void
        Display()
        {
            Log.Info("\nBuildAMation Statistics");
            Log.Info("Memory Usage");
            Log.Info($"Peak working set size : {BytesToMegaBytes(System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64):N2}MB");
            Log.Info($"Peak virtual size     : {BytesToMegaBytes(System.Diagnostics.Process.GetCurrentProcess().PeakVirtualMemorySize64):N2}MB");
            Log.Info($"GC total memory       : {BytesToMegaBytes(System.GC.GetTotalMemory(false)):N2}MB (after GC, {BytesToMegaBytes(System.GC.GetTotalMemory(true)):N2}MB)");
            Log.Info("\nObject counts");
            Log.Info($"Tokenized strings     : {TokenizedString.Count} ({TokenizedString.UnsharedCount} unshared)");
            TokenizedString.DumpCache();
            Log.Info($"Modules               : {Module.Count}");
            Log.Info("\nModule creation times");
            foreach (var env in Graph.Instance.BuildEnvironments)
            {
                var encapsulatingModules = Graph.Instance.EncapsulatingModules(env);
                Log.Info($"Configuration {env.Configuration.ToString()} has {encapsulatingModules.Count()} named/encapsulating modules, with the following creation times (ms):");
                foreach (var module in encapsulatingModules.OrderByDescending(item => item.CreationTime))
                {
                    Log.Info($"\t{module.ToString()}\t{module.CreationTime.TotalMilliseconds.ToString()}");
                }
            }
            TimingProfileUtilities.DumpProfiles();
        }
    }
}
