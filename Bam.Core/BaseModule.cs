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
namespace Bam.Core
{
namespace V2
{
    using System.Linq;

    public class ExecuteReasoning
    {
        public enum EReason
        {
            Undefined,
            FileDoesNotExist,
            InputFileIsNewer
        }

        private ExecuteReasoning(
            EReason reason,
            TokenizedString outputFilePath = null,
            TokenizedString inputFilePath = null)
        {
            this.Reason = reason;
            this.OutputFilePath = outputFilePath;
            this.InputFilePath = inputFilePath;
        }

        public static ExecuteReasoning
        Undefined()
        {
            return new ExecuteReasoning(EReason.Undefined);
        }

        public static ExecuteReasoning
        FileDoesNotExist(
            TokenizedString path)
        {
            return new ExecuteReasoning(EReason.FileDoesNotExist, path);
        }

        public static ExecuteReasoning
        InputFileNewer(
            TokenizedString outputPath,
            TokenizedString inputPath)
        {
            return new ExecuteReasoning(EReason.InputFileIsNewer, outputPath, inputPath);
        }

        public override string
        ToString()
        {
            switch (this.Reason)
            {
                case EReason.Undefined:
                    return "of undefined reasons - therefore executing the module to err on the side of caution";

                case EReason.FileDoesNotExist:
                    return System.String.Format("{0} does not exist", this.OutputFilePath.Parse());

                case EReason.InputFileIsNewer:
                    {
                        if (this.InputFilePath == this.OutputFilePath)
                        {
                            return "member(s) of the module collection were updated";
                        }
                        return System.String.Format("{0} is newer than {1}", this.InputFilePath.Parse(), this.OutputFilePath.Parse());
                    }

                default:
                    throw new Exception("Unknown execute reasoning, {0}", this.Reason.ToString());
            }
        }

        public EReason Reason
        {
            get;
            private set;
        }

        public TokenizedString OutputFilePath
        {
            get;
            private set;
        }

        public TokenizedString InputFilePath
        {
            get;
            private set;
        }

        public TokenizedString ModuleFilePath
        {
            get;
            private set;
        }
    }

    public interface IModuleExecution
    {
        void
        Execute(
            ExecutionContext context);

        ExecuteReasoning ReasonToExecute
        {
            get;
        }

        System.Threading.Tasks.Task ExecutionTask
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Abstract concept of a module, the base class for all buildables in BAM
    /// </summary>
    public abstract class Module :
        IModuleExecution
    {
        static protected System.Collections.Generic.List<Module> AllModules = new System.Collections.Generic.List<Module>();

        // private so that the factory method must be used
        protected Module()
        {
            var graph = Graph.Instance;
            if (null == graph.BuildEnvironment)
            {
                throw new Bam.Core.Exception("No build environment for module {0}", this.GetType().ToString());
            }

            graph.AddModule(this);
            this.Macros = new MacroList();
            // TODO: Can this be generalized to be a collection of files?
            this.GeneratedPaths = new System.Collections.Generic.Dictionary<FileKey, TokenizedString>();

            // add the package root
            var packageNameSpace = graph.CommonModuleType.Peek().Namespace;
            // TODO: temporarily check whether a V2 has been used in the namespace- trim if so
            if (packageNameSpace.EndsWith(".V2"))
            {
                packageNameSpace = packageNameSpace.Replace(".V2", string.Empty);
            }
#if true
            var packageDefinition = graph.Packages.Where(item => item.Name == packageNameSpace).FirstOrDefault();
            if (null == packageDefinition)
            {
                var includeTests = CommandLineProcessor.Evaluate(new UseTests());
                if (includeTests && packageNameSpace.EndsWith(".tests"))
                {
                    packageNameSpace = packageNameSpace.Replace(".tests", string.Empty);
                    packageDefinition = graph.Packages.Where(item => item.Name == packageNameSpace).FirstOrDefault();
                }

                if (null == packageDefinition)
                {
                    throw new Exception("Unable to locate package for namespace '{0}'", packageNameSpace);
                }
            }
            this.PackageDefinition = packageDefinition;
            this.Macros.Add("pkgroot", packageDefinition.GetPackageDirectory());
            this.Macros.Add("modulename", this.GetType().Name);
            this.Macros.Add("packagename", packageDefinition.Name);
            this.Macros.Add("pkgbuilddir", packageDefinition.GetBuildDirectory());
#else
            var packageInfo = Core.State.PackageInfo[packageNameSpace];
            if (null == packageInfo)
            {
                var includeTests = CommandLineProcessor.Evaluate(new UseTests());
                if (includeTests && packageNameSpace.EndsWith(".tests"))
                {
                    packageNameSpace = packageNameSpace.Replace(".tests", string.Empty);
                    packageInfo = Core.State.PackageInfo[packageNameSpace];
                }

                if (null == packageInfo)
                {
                    throw new Exception("Unable to locate package for namespace '{0}'", packageNameSpace);
                }
            }
            this.Package = packageInfo;
            var packageRoot = packageInfo.Identifier.Location.AbsolutePath;
            this.Macros.Add("pkgroot", packageRoot);
            this.Macros.Add("modulename", this.GetType().Name);
            this.Macros.Add("packagename", packageInfo.Name);
            this.Macros.Add("pkgbuilddir", packageInfo.BuildDirectory);
#endif

            this.OwningRank = null;
            this.Tool = null;
            this.MetaData = null;
            this.BuildEnvironment = graph.BuildEnvironment;
            this.Macros.Add("config", this.BuildEnvironment.Configuration.ToString());
            this.ReasonToExecute = ExecuteReasoning.Undefined();
        }

        // TODO: is this virtual or abstract?
        protected virtual void
        Init(
            Module parent)
        { }

        public static bool
        CanCreate(
            System.Type moduleType)
        {
            var filters = moduleType.GetCustomAttributes(typeof(PlatformFilterAttribute), true) as PlatformFilterAttribute[];
            if (0 == filters.Length)
            {
                // unconditional
                return true;
            }
            if (filters[0].Platform.Includes(Graph.Instance.BuildEnvironment.Platform))
            {
                // platform is a match
                return true;
            }
            Log.DebugMessage("Cannot create module of type {0} as it does not satisfy the platform filter", moduleType.ToString());
            return false;
        }

        public delegate void ModulePreInitDelegate(Module module);

        public static T
        Create<T>(
            Module parent = null,
            ModulePreInitDelegate preInitCallback = null) where T : Module, new()
        {
            if (!CanCreate(typeof(T)))
            {
                return null;
            }

            if (null == Graph.Instance.Mode)
            {
                throw new Exception("Building mode has not been set");
            }
            var module = new T();
            if (preInitCallback != null)
            {
                preInitCallback(module);
            }
            module.Init(parent);
            module.GetExecutionPolicy(Graph.Instance.Mode);
            AllModules.Add(module);
            return module;
        }

        protected void RegisterGeneratedFile(FileKey key, TokenizedString path)
        {
            if (this.GeneratedPaths.ContainsKey(key))
            {
                Core.Log.DebugMessage("Key '{0}' already exists", key);
                return;
            }
            this.GeneratedPaths.Add(key, path);
        }

        private void RegisterGeneratedFile(FileKey key)
        {
            this.RegisterGeneratedFile(key, null);
        }

        private void InternalDependsOn(Module module)
        {
            if (this.DependentsList.Contains(module))
            {
                return;
            }
            this.DependentsList.Add(module);
            module.DependeesList.Add(this);
        }

        public void DependsOn(Module module, params Module[] moreModules)
        {
            this.InternalDependsOn(module);
            foreach (var m in moreModules)
            {
                this.InternalDependsOn(m);
            }
        }

        private void InternalRequires(Module module)
        {
            if (this.RequiredDependentsList.Contains(module))
            {
                return;
            }
            this.RequiredDependentsList.Add(module);
            module.RequiredDependeesList.Add(this);
        }

        public void Requires(Module module, params Module[] moreModules)
        {
            this.InternalRequires(module);
            foreach (var m in moreModules)
            {
                this.InternalRequires(m);
            }
        }

        public Settings Settings
        {
            get;
            set;
        }

#if true
        public PackageDefinitionFile PackageDefinition
        {
            get;
            private set;
        }
#else
        public PackageInformation Package
        {
            get;
            private set;
        }
#endif

        public delegate void PrivatePatchDelegate(Settings settings);
        public void PrivatePatch(PrivatePatchDelegate dlg)
        {
            this.PrivatePatches.Add(dlg);
        }

        public delegate void PublicPatchDelegate(Settings settings, Module appliedTo);
        public void PublicPatch(PublicPatchDelegate dlg)
        {
            this.PublicPatches.Add(dlg);
        }

        public void UsePublicPatches(Module module)
        {
            this.ExternalPatches.Add(module.PublicPatches);
            this.ExternalPatches.AddRange(module.ExternalPatches);
        }

        public bool HasPatches
        {
            get
            {
                return (this.PrivatePatches.Count() > 0) ||
                       (this.PublicPatches.Count() > 0) ||
                       (this.ExternalPatches.Count() > 0);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Dependents
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.DependentsList);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Dependees
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.DependeesList);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Requirements
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.RequiredDependentsList);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Children
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.DependentsList.Where(item => (item is IChildModule) && ((item as IChildModule).Parent == this)).ToList());
            }
        }

        private System.Collections.Generic.List<Module> DependentsList = new System.Collections.Generic.List<Module>();
        private System.Collections.Generic.List<Module> DependeesList = new System.Collections.Generic.List<Module>();

        private System.Collections.Generic.List<Module> RequiredDependentsList = new System.Collections.Generic.List<Module>();
        private System.Collections.Generic.List<Module> RequiredDependeesList = new System.Collections.Generic.List<Module>();

        private System.Collections.Generic.List<PrivatePatchDelegate> PrivatePatches = new System.Collections.Generic.List<PrivatePatchDelegate>();
        private System.Collections.Generic.List<PublicPatchDelegate> PublicPatches = new System.Collections.Generic.List<PublicPatchDelegate>();
        private System.Collections.Generic.List<System.Collections.Generic.List<PublicPatchDelegate>> ExternalPatches = new System.Collections.Generic.List<System.Collections.Generic.List<PublicPatchDelegate>>();

        public System.Collections.Generic.Dictionary<FileKey, TokenizedString> GeneratedPaths
        {
            get;
            private set;
        }

        public object MetaData
        {
            get;
            set;
        }

        protected abstract void
        ExecuteInternal(
            ExecutionContext context);

        void
        IModuleExecution.Execute(
            ExecutionContext context)
        {
            if (context.Evaluate)
            {
                if (null != this.EvaluationTask)
                {
                    this.EvaluationTask.Wait();
                }
                if (null == this.ReasonToExecute)
                {
                    Log.DebugMessage("Module {0} is up-to-date", this.ToString());
                    return;
                }
                Log.DebugMessage("Module {0} will change because {1}.", this.ToString(), this.ReasonToExecute.ToString());
            }
            this.ExecuteInternal(context);
        }

        public bool TopLevel
        {
            get
            {
                var isTopLevel = (0 == this.DependeesList.Count) && (0 == this.RequiredDependeesList.Count);
                return isTopLevel;
            }
        }

        public MacroList Macros
        {
            get;
            private set;
        }

        public ModuleCollection OwningRank
        {
            get;
            set;
        }

        protected abstract void GetExecutionPolicy(string mode);

        public Module Tool
        {
            get;
            protected set;
        }

        public void ApplySettingsPatches()
        {
            this.ApplySettingsPatches(this.Settings, true);
        }

        public void
        ApplySettingsPatches(
            Settings settings,
            bool honourParents)
        {
            if (null == settings)
            {
                return;
            }
            // Note: first private patches, followed by public patches
            // TODO: they could override each other - anyway to check?
            var parentModule = (this is IChildModule) && honourParents ? (this as IChildModule).Parent : null;
            if (parentModule != null)
            {
                foreach (var patch in parentModule.PrivatePatches)
                {
                    patch(settings);
                }
            }
            foreach (var patch in this.PrivatePatches)
            {
                patch(settings);
            }
            if (parentModule != null)
            {
                foreach (var patch in parentModule.PublicPatches)
                {
                    patch(settings, this);
                }
            }
            foreach (var patch in this.PublicPatches)
            {
                patch(settings, this);
            }
            if (parentModule != null)
            {
                foreach (var patchList in parentModule.ExternalPatches)
                {
                    foreach (var patch in patchList)
                    {
                        patch(settings, this);
                    }
                }
            }
            foreach (var patchList in this.ExternalPatches)
            {
                foreach (var patch in patchList)
                {
                    patch(settings, this);
                }
            }
        }

        public ExecuteReasoning ReasonToExecute
        {
            get;
            protected set;
        }

        public System.Threading.Tasks.Task ExecutionTask
        {
            get;
            set;
        }

        public System.Threading.Tasks.Task
        EvaluationTask
        {
            get;
            protected set;
        }

        public abstract void
        Evaluate();

        public Environment BuildEnvironment
        {
            get;
            private set;
        }

        public Module GetEncapsulatingReferencedModule()
        {
            if (Graph.Instance.IsReferencedModule(this))
            {
                return this;
            }
            if (this.DependeesList.Count > 1)
            {
                throw new Exception("More than one dependee attached to {0}, to uniquely identify the encapsulating module", this.ToString());
            }
            if (this.RequiredDependeesList.Count > 1)
            {
                throw new Exception("More than one requiree attached to {0}, to uniquely identify the encapsulating module", this.ToString());
            }
            Module encapsulating;
            if (0 == this.DependeesList.Count)
            {
                if (0 == this.RequiredDependeesList.Count)
                {
                    throw new Exception("No dependees or requirees attached to {0}. Cannot determine the encapsulating module", this.ToString());
                }
                encapsulating = this.RequiredDependeesList[0].GetEncapsulatingReferencedModule();
            }
            else
            {
                encapsulating = this.DependeesList[0].GetEncapsulatingReferencedModule();
            }
            this.Macros.Add("encapsulatingpkgbuilddir", encapsulating.Macros["pkgbuilddir"]);
            return encapsulating;
        }

        private void
        Complete()
        {
            var graph = Graph.Instance;
            var encapsulatingModule = this.GetEncapsulatingReferencedModule();
            this.Macros.Add("moduleoutputdir", System.IO.Path.Combine(encapsulatingModule.GetType().Name, this.BuildEnvironment.Configuration.ToString()));
        }

        static public void
        CompleteModules()
        {
            foreach (var module in AllModules.Reverse<Module>())
            {
                module.Complete();
            }
        }
    }
}
    /// <summary>
    /// BaseModules are the base class for all real modules in package scripts.
    /// These are constructed by the Bam Core when they are required.
    /// Nested modules that appear as fields are either constructed automatically by
    /// the default constructor of their parent, or in the custom construct required to be
    /// written by the package author. As such, there must always be a default constructor
    /// in BaseModule.
    /// </summary>
    public abstract class BaseModule :
        IModule
    {
        private readonly LocationKey PackageDirKey = new LocationKey("PackageDirectory", ScaffoldLocation.ETypeHint.Directory);

        private void
        StubOutputLocations(
            System.Type moduleType)
        {
            this.Locations[State.ModuleBuildDirLocationKey] = new ScaffoldLocation(ScaffoldLocation.ETypeHint.Directory);

            var toolAssignment = moduleType.GetCustomAttributes(typeof(ModuleToolAssignmentAttribute), true);
            // this is duplicating work, as the toolset is in the Target.Toolset, but passing a Target down to
            // the BaseModule constructor will break a lot of existing scripts, and their simplicity
            // TODO: it may be considered a change in a future version
            var toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            var toolAttr = toolAssignment[0] as ModuleToolAssignmentAttribute;
            if (!toolset.HasTool(toolAttr.ToolType))
            {
                return;
            }
            var tool = toolset.Tool(toolAttr.ToolType);
            if (null != tool)
            {
                foreach (var locationKey in tool.OutputLocationKeys(this))
                {
                    this.Locations[locationKey] = new ScaffoldLocation(locationKey.Type);
                }
            }
        }

        protected
        BaseModule()
        {
            this.ProxyPath = new ProxyModulePath();
            this.Locations = new LocationMap();
            this.Locations[State.BuildRootLocationKey] = State.BuildRootLocation;

            var moduleType = this.GetType();
            this.StubOutputLocations(moduleType);

            var packageName = moduleType.Namespace;
#if true
#else
            var package = State.PackageInfo[packageName];
            if (null != package)
            {
                var root = new ScaffoldLocation(package.Identifier.Location, this.ProxyPath, ScaffoldLocation.ETypeHint.Directory, Location.EExists.Exists);
                this.PackageLocation = root;
            }
#endif
        }

        /// <summary>
        /// Locations are only valid for named modules
        /// </summary>
        public LocationMap Locations
        {
            get;
            private set;
        }

        public Location PackageLocation
        {
            get
            {
                return this.Locations[PackageDirKey];
            }

            private set
            {
                this.Locations[PackageDirKey] = value;
            }
        }

        public event UpdateOptionCollectionDelegate UpdateOptions;

        public virtual BaseOptionCollection Options
        {
            get;
            set;
        }

        public ProxyModulePath ProxyPath
        {
            get;
            private set;
        }

        public void
        ExecuteOptionUpdate(
            Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this as IModule, target);
            }
        }

        private DependencyNode owningNode = null;
        public DependencyNode OwningNode
        {
            get
            {
                return this.owningNode;
            }

            set
            {
                if (null != this.owningNode)
                {
                    throw new Exception("Module {0} cannot have it's node reassigned to {1}", this.owningNode.UniqueModuleName, value.UniqueModuleName);
                }

                this.owningNode = value;
            }
        }
    }
}
