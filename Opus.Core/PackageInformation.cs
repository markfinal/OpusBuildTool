﻿// <copyright file="PackageInformation.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class PackageInformation : System.IComparable
    {
        private string directory;
        
        public static PackageInformation FromPath(string path, bool checkForPackageFiles)
        {
            PackageInformation packageInformation = null;
            
            string[] directories = path.Split(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
            if (directories.Length >= 2)
            {
                string packageName = directories[directories.Length - 2];
                string packageVersion = directories[directories.Length - 1];
                System.IO.DirectoryInfo parentDir = System.IO.Directory.GetParent(path);
                if (null == parentDir)
                {
                    Log.DebugMessage("No parent directory");
                    return null;
                }
                System.IO.DirectoryInfo parentParentDir = System.IO.Directory.GetParent(parentDir.FullName);
                if (null == parentParentDir)
                {
                    Log.DebugMessage("No parent of parent directory");
                    return null;
                }
                string root = parentParentDir.FullName;

                string basePackageFilename = System.IO.Path.Combine(path, packageName);
                string scriptFilename = basePackageFilename + ".cs";
                string xmlFilename = basePackageFilename + ".xml";
                if (System.IO.File.Exists(scriptFilename) &&
                    System.IO.File.Exists(xmlFilename))
                {
                    packageInformation = new PackageInformation(packageName, packageVersion, root);
                    Core.Log.DebugMessage("Path '{0}' refers to a valid package; fullname = '{1}', root = '{2}'", path, packageInformation.FullName, root);
                }
                else if (!checkForPackageFiles)
                {
                    Core.Log.DebugMessage("Path '{0}' is not a package, but can be a package directory.", path);
                    packageInformation = new PackageInformation(packageName, packageVersion, root);
                }

                if (null != packageInformation)
                {
                    State.PackageInfo.Add(packageInformation);
                    if (!State.PackageRoots.Contains(packageInformation.Root))
                    {
                        State.PackageRoots.Add(packageInformation.Root);
                    }
                    packageInformation.PackageDefinition.Read();
                }
                else
                {
                    Core.Log.DebugMessage("Path '{0}' is not a package directory. Perhaps some files are missing or misnamed?", path);
                }
            }
            else
            {
                throw new Exception(System.String.Format("Cannot determine package name and version from the path '{0}'. Expected format is 'root{1}packagename{1}version'", path, System.IO.Path.DirectorySeparatorChar), false);
            }
                        
            return packageInformation;
        }
        
        public static PackageInformation FindPackage(string name, string version)
        {
            if (null == name)
            {
                throw new Exception("Invalid name");
            }
            if (null == version)
            {
                throw new Exception("Invalid version");
            }

            // first check if it already been loaded
            {
                PackageInformationCollection packages = State.PackageInfo;
                foreach (PackageInformation package in packages)
                {
                    if ((package.Name == name) && (package.Version == version))
                    {
                        Log.MessageAll("Package '{0}' already in the system", package.FullName);
                        return package;
                    }
                }
            }

            StringArray packageRoots = State.PackageRoots;
            if (null == packageRoots)
            {
                throw new Exception("There are no package roots specified");
            }
            foreach (string packageRoot in packageRoots)
            {
                string packageDirectory = System.IO.Path.Combine(packageRoot, name);
                packageDirectory = System.IO.Path.Combine(packageDirectory, version);

                PackageInformation package = FromPath(packageDirectory, true);
                if (null != package)
                {
                    return package;
                }
            }

            return null;
        }

        private PackageInformation(string name, string version)
        {
            this.Name = name;
            this.Version = version;
            this.Root = null;
            this.IsBuilder = false;
            this.Directory = null;

            throw new Exception("Who calls this constructor?");
        }

        // restrict access to this, to emphasize that it's a heavyweight operation
        private PackageInformation(string name, string version, string root)
        {
            this.Name = name;
            this.Version = version;
            this.Root = root;
            this.IsBuilder = false;
            
            string directory = System.IO.Path.Combine(this.Root, this.Name);
            directory = System.IO.Path.Combine(directory, this.Version);
            this.Directory = directory;

            this.PackageDefinition = new PackageDependencyXmlFile(this.DependencyFile, true);
        }
        
        public string Name
        {
            get;
            private set;
        }
        
        public string Version
        {
            get;
            private set;
        }
        
        public string Root
        {
            get;
            private set;
        }
        
        public bool IsBuilder
        {
            get;
            set;
        }
        
        public string Directory
        {
            get
            {
                return this.directory;
            }
            private set
            {
                this.directory = value;
            }
        }
        
        public string DependencyFile
        {
            get
            {
                string dependencyFile = System.IO.Path.Combine(this.Directory, this.Name + ".xml");
                return dependencyFile;
            }
        }

        public PackageDependencyXmlFile PackageDefinition
        {
            get;
            private set;
        }
        
        public string ScriptFile
        {
            get
            {
                string scriptFile = System.IO.Path.Combine(this.Directory, this.Name + ".cs");
                return scriptFile;
            }
        }
        
        private string ScriptDirectory
        {
            get
            {
                string scriptDirectory = System.IO.Path.Combine(this.Directory, "Scripts");
                return scriptDirectory;
            }
        }
        
        public string OpusDirectory
        {
            get
            {
                string opusDirectory = System.IO.Path.Combine(this.Directory, "Opus");
                return opusDirectory;
            }
        }
        
        public string DebugProjectFilename
        {
            get
            {
                string debugProjectFilename = System.IO.Path.Combine(this.OpusDirectory, System.String.Format("{0}.csproj", this.FullName));
                return debugProjectFilename;
            }
        }
        
        public StringArray Scripts
        {
            get
            {
                StringArray scripts = new StringArray();
                if (System.IO.Directory.Exists(this.ScriptDirectory))
                {
                    string[] files = System.IO.Directory.GetFiles(this.ScriptDirectory, "*.cs", System.IO.SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        scripts.AddRange(files);
                    }
                    if (0 == scripts.Count)
                    {
                        scripts = null;
                    }
                }
                return scripts;
            }
        }
        
        public StringArray BuilderScripts
        {
            get
            {
                PackageInformation builderPackage = State.BuilderPackage;
                if (null == builderPackage)
                {
                    throw new Exception("No builder package found");
                }

                StringArray builderScripts = new StringArray();
                string builderDirectory = System.IO.Path.Combine(this.Directory, builderPackage.Name);
                if (System.IO.Directory.Exists(builderDirectory))
                {
                    string[] files = System.IO.Directory.GetFiles(builderDirectory, "*.cs");
                    if (files.Length > 0)
                    {
                        builderScripts.AddRange(files);
                    }
                }

                if (0 == builderScripts.Count)
                {
                    builderScripts = null;
                }

                return builderScripts;
            }
        }

        public string BuildDirectory
        {
            get
            {
                string buildRoot = Core.State.BuildRoot;
                string packageBuildDirectory = System.IO.Path.Combine(buildRoot, this.FullName);
                return packageBuildDirectory;
            }
        }

        public string FullName
        {
            get
            {
                string fullName = System.String.Format("{0}-{1}", this.Name, this.Version);
                return fullName;
            }
        }

        public override string ToString()
        {
            return this.FullName;
        }

        int System.IComparable.CompareTo(object obj)
        {
            PackageInformation objAs = obj as PackageInformation;
            int compared = this.FullName.CompareTo(objAs.FullName);
            return compared;
        }
    }
}