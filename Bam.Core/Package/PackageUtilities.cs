#region License
// Copyright (c) 2010-2015, Mark Final
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
    public static class PackageUtilities
    {
        public static readonly string BamSubFolder = "bam";
        public static readonly string ScriptsSubFolder = "Scripts";

        public static void
        MakePackage()
        {
            var packageDir = Graph.Instance.ProcessState.WorkingDirectory;
            var bamDir = System.IO.Path.Combine(packageDir, BamSubFolder);
            if (System.IO.Directory.Exists(bamDir))
            {
                throw new Exception("A Bam package already exists at {0}", packageDir);
            }

            var packageNameArgument = new PackageName();
            var packageName = CommandLineProcessor.Evaluate(packageNameArgument);
            if (null == packageName)
            {
                throw new Exception("No name was defined. Use {0} on the command line to specify it.", (packageNameArgument as ICommandLineArgument).LongName);
            }

            var packageVersion = CommandLineProcessor.Evaluate(new PackageVersion());
            var definition = new PackageDefinition(bamDir, packageName, packageVersion);

            System.IO.Directory.CreateDirectory(bamDir);
            definition.Write();

            var scriptsDir = System.IO.Path.Combine(bamDir, ScriptsSubFolder);
            System.IO.Directory.CreateDirectory(scriptsDir);

            var initialScriptFile = System.IO.Path.Combine(scriptsDir, packageName) + ".cs";
            using (System.IO.TextWriter writer = new System.IO.StreamWriter(initialScriptFile))
            {
                writer.WriteLine("using Bam.Core;");
                writer.WriteLine("namespace {0}", packageName);
                writer.WriteLine("{");
                writer.WriteLine("    // write modules here ...");
                writer.WriteLine("}");
            }
        }

        public static void
        AddDependentPackage()
        {
            var packageNameArgument = new PackageName();
            var packageName = CommandLineProcessor.Evaluate(packageNameArgument);
            if (null == packageName)
            {
                throw new Exception("No name was defined. Use {0} on the command line to specify it.", (packageNameArgument as ICommandLineArgument).LongName);
            }

            var packageVersion = CommandLineProcessor.Evaluate(new PackageVersion());

            var masterPackage = GetMasterPackage();
            // TODO: no checking if this package exists
            // TODO: is this dependent already present?
            if (null != masterPackage.Dependents.Where(item => item.Item1 == packageName && item.Item2 == packageVersion).FirstOrDefault())
            {
                if (null != packageVersion)
                {
                    throw new Exception("Dependency {0}, version {1}, already exists", packageName, packageVersion);
                }
                else
                {
                    throw new Exception("Dependency {0} already exists", packageName);
                }
            }

            masterPackage.Dependents.Add(new System.Tuple<string, string, bool?>(packageName, packageVersion, null));
            masterPackage.Write();
        }

        public static string VersionDefineForCompiler
        {
            get
            {
                var coreVersion = Graph.Instance.ProcessState.Version;
                var coreVersionDefine = System.String.Format("BAM_CORE_VERSION_{0}_{1}_{2}",
                    coreVersion.Major,
                    coreVersion.Minor,
                    coreVersion.Revision);
                return coreVersionDefine;
            }
        }

        public static string HostPlatformDefineForCompiler
        {
            get
            {
                var platformString = Platform.ToString(OSUtilities.CurrentPlatform, '\0', "BAM_HOST_", true);
                return platformString;
            }
        }

        public static bool
        IsPackageDirectory(
            string packagePath)
        {
            var bamDir = System.IO.Path.Combine(packagePath, BamSubFolder);
            if (!System.IO.Directory.Exists(bamDir))
            {
                throw new Exception("Path {0} does not form a BAM! package: missing '{1}' subdirectory", packagePath, BamSubFolder);
            }

            return true;
        }

        public static string
        GetPackageDefinitionPathname(
            string packagePath)
        {
            var bamDir = System.IO.Path.Combine(packagePath, BamSubFolder);
            var xmlFiles = System.IO.Directory.GetFiles(bamDir, "*.xml", System.IO.SearchOption.AllDirectories);
            if (0 == xmlFiles.Length)
            {
                throw new Exception("No package definition .xml files found under {0}", bamDir);
            }
            if (xmlFiles.Length > 1)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("Too many .xml files found under {0}", bamDir);
                message.AppendLine();
                foreach (var file in xmlFiles)
                {
                    message.AppendFormat("\t{0}", file);
                    message.AppendLine();
                }
                throw new Exception(message.ToString());
            }
            return xmlFiles[0];
        }

        public static PackageDefinition
        GetMasterPackage(
            bool enforceBamAssemblyVersions = true)
        {
            var workingDir = Graph.Instance.ProcessState.WorkingDirectory;
            var isWorkingPackageWellDefined = IsPackageDirectory(workingDir);
            if (!isWorkingPackageWellDefined)
            {
                throw new Exception("Working directory package is not well defined");
            }

            var masterDefinitionFile = new PackageDefinition(GetPackageDefinitionPathname(workingDir), !Graph.Instance.ForceDefinitionFileUpdate);
            masterDefinitionFile.Read(true, enforceBamAssemblyVersions);
            return masterDefinitionFile;
        }

        public static void
        IdentifyAllPackages(
            bool allowDuplicates = false,
            bool enforceBamAssemblyVersions = true)
        {
            var packageRepos = new System.Collections.Generic.Queue<string>();
            foreach (var repo in Graph.Instance.PackageRepositories)
            {
                if (packageRepos.Contains(repo))
                {
                    continue;
                }
                packageRepos.Enqueue(repo);
            }

            var masterDefinitionFile = GetMasterPackage(enforceBamAssemblyVersions: enforceBamAssemblyVersions);
            foreach (var repo in masterDefinitionFile.PackageRepositories)
            {
                if (packageRepos.Contains(repo))
                {
                    continue;
                }
                packageRepos.Enqueue(repo);
            }

            // read the definition files of any package found in the package roots
            var candidatePackageDefinitions = new Array<PackageDefinition>();
            candidatePackageDefinitions.Add(masterDefinitionFile);
            while (packageRepos.Count > 0)
            {
                var repo = packageRepos.Dequeue();
                if (!System.IO.Directory.Exists(repo))
                {
                    throw new Exception("Package repository directory {0} does not exist", repo);
                }
                var candidatePackageDirs = System.IO.Directory.GetDirectories(repo, BamSubFolder, System.IO.SearchOption.AllDirectories);

                Graph.Instance.PackageRepositories.Add(repo);

                foreach (var bamDir in candidatePackageDirs)
                {
                    var packageDir = System.IO.Path.GetDirectoryName(bamDir);
                    var packageDefinitionPath = GetPackageDefinitionPathname(packageDir);

                    // ignore any duplicates (can be found due to nested repositories)
                    if (null != candidatePackageDefinitions.Where(item => item.XMLFilename == packageDefinitionPath).FirstOrDefault())
                    {
                        continue;
                    }

                    var definitionFile = new PackageDefinition(packageDefinitionPath, !Graph.Instance.ForceDefinitionFileUpdate);
                    definitionFile.Read(true, enforceBamAssemblyVersions);
                    candidatePackageDefinitions.Add(definitionFile);

                    foreach (var newRepo in definitionFile.PackageRepositories)
                    {
                        if (Graph.Instance.PackageRepositories.Contains(newRepo))
                        {
                            continue;
                        }
                        packageRepos.Enqueue(newRepo);
                    }
                }
            }

            // defaults come from
            // - the master definition file
            // - command line args (these trump the mdf)
            // and only requires resolving when referenced
            var packageDefinitions = new Array<PackageDefinition>();
            PackageDefinition.ResolveDependencies(masterDefinitionFile, packageDefinitions, candidatePackageDefinitions);

            // now resolve any duplicate names using defaults
            // unless duplicates are allowed
            var duplicatePackageNames = packageDefinitions.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key);
            if ((duplicatePackageNames.Count() > 0) && !allowDuplicates)
            {
                var versionSpeciferArgs = new PackageDefaultVersion();
                var packageVersionSpecifiers = CommandLineProcessor.Evaluate(versionSpeciferArgs);
                var toRemove = new Array<PackageDefinition>();

                foreach (var dupName in duplicatePackageNames)
                {
                    var duplicates = packageDefinitions.Where(item => item.Name == dupName);
                    PackageDefinition resolvedDuplicate = null;
                    // command line specifications take precedence to resolve a duplicate
                    foreach (var specifier in packageVersionSpecifiers)
                    {
                        if (!specifier.Contains(dupName))
                        {
                            continue;
                        }

                        foreach (var dupPackage in duplicates)
                        {
                            if (specifier[1] == dupPackage.Version)
                            {
                                resolvedDuplicate = dupPackage;
                                break;
                            }
                        }

                        if (resolvedDuplicate != null)
                        {
                            break;
                        }

                        var noMatchMessage = new System.Text.StringBuilder();
                        noMatchMessage.AppendFormat("Command line version specified, {0}, could not resolve to one of the available versions of package {1}:", specifier[1], duplicates.First().Name);
                        noMatchMessage.AppendLine();
                        foreach (var dup in duplicates)
                        {
                            noMatchMessage.AppendFormat("\t{0}", dup.Version);
                            noMatchMessage.AppendLine();
                        }
                        throw new Exception(noMatchMessage.ToString());
                    }

                    if (resolvedDuplicate != null)
                    {
                        toRemove.AddRange(packageDefinitions.Where(item => (item.Name == dupName) && (item != resolvedDuplicate)));
                        continue;
                    }

                    // now look at the master dependency file, for any 'default' specifications
                    var masterDependency = masterDefinitionFile.Dependents.Where(item => item.Item1 == dupName && item.Item3.HasValue && item.Item3.Value).FirstOrDefault();
                    if (null != masterDependency)
                    {
                        toRemove.AddRange(packageDefinitions.Where(item => (item.Name == dupName) && (item.Version != masterDependency.Item2)));
                        continue;
                    }

                    var resolveErrorMessage = new System.Text.StringBuilder();
                    resolveErrorMessage.AppendFormat("Unable to resolve to a single version of package {0}. Use --{0}.version=<version> to resolve. Available versions of the package are:", duplicates.First().Name);
                    resolveErrorMessage.AppendLine();
                    foreach (var dup in duplicates)
                    {
                        resolveErrorMessage.AppendFormat("\t{0}", dup.Version);
                        resolveErrorMessage.AppendLine();
                    }
                    throw new Exception(resolveErrorMessage.ToString());
                }

                packageDefinitions.RemoveAll(toRemove);
            }

            Graph.Instance.SetPackageDefinitions(packageDefinitions);
        }

        private static string
        GetPackageHash(
            StringArray sourceCode,
            StringArray definitions,
            Array<BamAssemblyDescription> bamAssemblies)
        {
            int hashCode = 0;
            foreach (var source in sourceCode)
            {
                hashCode ^= source.GetHashCode();
            }
            foreach (var define in definitions)
            {
                hashCode ^= define.GetHashCode();
            }
            foreach (var assembly in bamAssemblies)
            {
                var assemblyPath = System.IO.Path.Combine(Graph.Instance.ProcessState.ExecutableDirectory, assembly.Name) + ".dll";
                var lastModifiedDate = System.IO.File.GetLastWriteTime(assemblyPath);
                hashCode ^= lastModifiedDate.GetHashCode();
            }
            var hash = hashCode.ToString();
            return hash;
        }

        public static bool
        CompilePackageAssembly(
            bool enforceBamAssemblyVersions = true)
        {
            // validate build root
            if (null == Graph.Instance.BuildRoot)
            {
                throw new Exception("Build root has not been specified");
            }

            var gatherSourceProfile = new TimeProfile(ETimingProfiles.GatherSource);
            gatherSourceProfile.StartProfile();

            IdentifyAllPackages(enforceBamAssemblyVersions: enforceBamAssemblyVersions);

            var cleanFirst = CommandLineProcessor.Evaluate(new CleanFirst());
            if (cleanFirst && System.IO.Directory.Exists(Graph.Instance.BuildRoot))
            {
                Log.Info("Deleting build root '{0}'", Graph.Instance.BuildRoot);
                try
                {
                    System.IO.Directory.Delete(Graph.Instance.BuildRoot, true);
                }
                catch (System.IO.IOException ex)
                {
                    Log.Info("Failed to delete build root, because {0}. Continuing", ex.Message);
                }
            }

            if (!System.IO.Directory.Exists(Graph.Instance.BuildRoot))
            {
                System.IO.Directory.CreateDirectory(Graph.Instance.BuildRoot);
            }

            BuildModeUtilities.SetBuildModePackage();

            var definitions = new StringArray();

            // gather source files
            var sourceCode = new StringArray();
            int packageIndex = 0;
            foreach (var package in Graph.Instance.Packages)
            {
                Log.DebugMessage("{0}: '{1}' @ '{2}'", packageIndex, package.Version, (package.PackageRepositories.Count > 0) ? package.PackageRepositories[0] : "Not in a repository");

                // to compile with debug information, you must compile the files
                // to compile without, we need to file contents to hash the source
                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    var scripts = package.GetScriptFiles();
                    sourceCode.AddRange(scripts);
                    Log.DebugMessage(scripts.ToString("\n\t"));
                }
                else
                {
                    foreach (var scriptFile in package.GetScriptFiles())
                    {
                        using (var reader = new System.IO.StreamReader(scriptFile))
                        {
                            sourceCode.Add(reader.ReadToEnd());
                        }
                        Log.DebugMessage("\t'{0}'", scriptFile);
                    }
                }

                foreach (var define in package.Definitions)
                {
                    if (!definitions.Contains(define))
                    {
                        definitions.Add(define);
                    }
                }

                ++packageIndex;
            }

            // add/remove other definitions
            definitions.Add(VersionDefineForCompiler);
            definitions.Add(HostPlatformDefineForCompiler);
            definitions.Sort();

            gatherSourceProfile.StopProfile();

            var assemblyCompileProfile = new TimeProfile(ETimingProfiles.AssemblyCompilation);
            assemblyCompileProfile.StartProfile();

            // assembly is written to the build root
            var cachedAssemblyPathname = System.IO.Path.Combine(Graph.Instance.BuildRoot, "CachedPackageAssembly");
            cachedAssemblyPathname = System.IO.Path.Combine(cachedAssemblyPathname, Graph.Instance.MasterPackage.Name) + ".dll";
            var hashPathName = System.IO.Path.ChangeExtension(cachedAssemblyPathname, "hash");
            string thisHashCode = null;

            var cacheAssembly = !CommandLineProcessor.Evaluate(new DisableCacheAssembly());

            string compileReason = null;
            if (Graph.Instance.CompileWithDebugSymbols)
            {
                compileReason = "debug symbols were enabled";
            }
            else
            {
                // can an existing assembly be reused?
                thisHashCode = GetPackageHash(sourceCode, definitions, Graph.Instance.MasterPackage.BamAssemblies);
                if (cacheAssembly)
                {
                    if (System.IO.File.Exists(hashPathName))
                    {
                        using (var reader = new System.IO.StreamReader(hashPathName))
                        {
                            var diskHashCode = reader.ReadLine();
                            if (diskHashCode.Equals(thisHashCode))
                            {
                                Log.DebugMessage("Cached assembly used '{0}', with hash {1}", cachedAssemblyPathname, diskHashCode);
                                Log.Detail("Re-using existing package assembly");
                                Graph.Instance.ScriptAssemblyPathname = cachedAssemblyPathname;

                                assemblyCompileProfile.StopProfile();

                                return true;
                            }
                            else
                            {
                                compileReason = "package source has changed since the last compile";
                            }
                        }
                    }
                    else
                    {
                        compileReason = "no previously compiled package assembly exists";
                    }
                }
                else
                {
                    compileReason = "user has disabled package assembly caching";
                }
            }

            // use the compiler in the current runtime version to build the assembly of packages
            var clrVersion = System.Environment.Version;
            var compilerVersion = System.String.Format("v{0}.{1}", clrVersion.Major, clrVersion.Minor);

            Log.Detail("Compiling package assembly (C# compiler {0}{1}), because {2}.",
                compilerVersion,
                Graph.Instance.ProcessState.TargetFrameworkVersion != null ? (", targetting " + Graph.Instance.ProcessState.TargetFrameworkVersion) : string.Empty,
                compileReason);

            var providerOptions = new System.Collections.Generic.Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", compilerVersion);

            if (Graph.Instance.ProcessState.RunningMono)
            {
                Log.DebugMessage("Compiling assembly for Mono");
            }

            using (var provider = new Microsoft.CSharp.CSharpCodeProvider(providerOptions))
            {
                var compilerParameters = new System.CodeDom.Compiler.CompilerParameters();
                compilerParameters.TreatWarningsAsErrors = true;
                compilerParameters.WarningLevel = 4;
                compilerParameters.GenerateExecutable = false;
                compilerParameters.GenerateInMemory = false;

                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    compilerParameters.OutputAssembly = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Graph.Instance.MasterPackage.Name) + ".dll";
                }
                else
                {
                    compilerParameters.OutputAssembly = cachedAssemblyPathname;
                }

                var compilerOptions = "/checked+ /unsafe-";
                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    compilerParameters.IncludeDebugInformation = true;
                    compilerOptions += " /optimize-";
                }
                else
                {
                    compilerOptions += " /optimize+";
                }
                compilerOptions += " /platform:anycpu";

                // define strings
                compilerOptions += " /define:" + definitions.ToString(';');

                compilerParameters.CompilerOptions = compilerOptions;

                if (provider.Supports(System.CodeDom.Compiler.GeneratorSupport.Resources))
                {
                    // Bam assembly
                    // TODO: Q: why is it only for the master package? Why not all of them, which may have additional dependencies?
                    foreach (var assembly in Graph.Instance.MasterPackage.BamAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", assembly.Name);
                        var assemblyPathName = System.IO.Path.Combine(Graph.Instance.ProcessState.ExecutableDirectory, assemblyFileName);
                        compilerParameters.ReferencedAssemblies.Add(assemblyPathName);
                    }

                    // DotNet assembly
                    foreach (var desc in Graph.Instance.MasterPackage.DotNetAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", desc.Name);
                        compilerParameters.ReferencedAssemblies.Add(assemblyFileName);
                    }

                    if (Graph.Instance.ProcessState.RunningMono)
                    {
                        compilerParameters.ReferencedAssemblies.Add("Mono.Posix.dll");
                    }
                }
                else
                {
                    throw new Exception("C# compiler does not support Resources");
                }

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(compilerParameters.OutputAssembly));

                var results = Graph.Instance.CompileWithDebugSymbols ?
                    provider.CompileAssemblyFromFile(compilerParameters, sourceCode.ToArray()) :
                    provider.CompileAssemblyFromSource(compilerParameters, sourceCode.ToArray());

                if (results.Errors.HasErrors || results.Errors.HasWarnings)
                {
                    Log.ErrorMessage("Failed to compile package '{0}'. There are {1} errors.", Graph.Instance.MasterPackage.FullName, results.Errors.Count);
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        Log.ErrorMessage("\t{0}({1}): {2} {3}", error.FileName, error.Line, error.ErrorNumber, error.ErrorText);
                    }
                    return false;
                }

                if (!Graph.Instance.CompileWithDebugSymbols)
                {
                    if (cacheAssembly)
                    {
                        using (var writer = new System.IO.StreamWriter(hashPathName))
                        {
                            writer.WriteLine(thisHashCode);
                        }
                    }
                    else
                    {
                        // will not throw if the file doesn't exist
                        System.IO.File.Delete(hashPathName);
                    }
                }

                Log.DebugMessage("Written assembly to '{0}'", compilerParameters.OutputAssembly);
                Graph.Instance.ScriptAssemblyPathname = compilerParameters.OutputAssembly;
            }

            assemblyCompileProfile.StopProfile();

            return true;
        }

        public static void
        LoadPackageAssembly()
        {
            var assemblyLoadProfile = new TimeProfile(ETimingProfiles.LoadAssembly);
            assemblyLoadProfile.StartProfile();

            System.Reflection.Assembly scriptAssembly = null;

            try
            {
                // this code works from an untrusted location, and debugging IS available when
                // the pdb (.NET)/mdb (Mono) resides beside the assembly
                byte[] asmBytes = System.IO.File.ReadAllBytes(Graph.Instance.ScriptAssemblyPathname);
                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    var debugInfoFilename = Graph.Instance.ProcessState.RunningMono ?
                        Graph.Instance.ScriptAssemblyPathname + ".mdb" :
                        System.IO.Path.ChangeExtension(Graph.Instance.ScriptAssemblyPathname, ".pdb");
                    if (System.IO.File.Exists(debugInfoFilename))
                    {
                        byte[] pdbBytes = System.IO.File.ReadAllBytes(debugInfoFilename);
                        scriptAssembly = System.Reflection.Assembly.Load(asmBytes, pdbBytes);
                    }
                }

                if (null == scriptAssembly)
                {
                    scriptAssembly = System.Reflection.Assembly.Load(asmBytes);
                }
            }
            catch (System.IO.FileNotFoundException exception)
            {
                Log.ErrorMessage("Could not find assembly '{0}'", Graph.Instance.ScriptAssemblyPathname);
                throw exception;
            }
            catch (System.Exception exception)
            {
                throw exception;
            }

            Graph.Instance.ScriptAssembly = scriptAssembly;

            assemblyLoadProfile.StopProfile();
        }
    }
}
