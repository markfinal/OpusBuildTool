namespace CodeGenTest2
{
    public class ExportCodeGenOptionsDelegateAttribute : System.Attribute
    {
    }

    public class LocalCodeGenOptionsDelegateAttribute : System.Attribute
    {
    }

    public sealed class PrivateData : CommandLineProcessor.ICommandLineDelegate
    {
        public PrivateData(CommandLineProcessor.Delegate commandLineDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
        }
    

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }
    }

    public partial class CodeGenOptions : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, Opus.Core.IOutputPaths
    {
        public CodeGenOptions(Opus.Core.DependencyNode node)
            : base()
        {
            this.OutputSourceDirectory = node.GetTargettedModuleBuildDirectory("src");
            this.OutputName = "function";

            this["OutputSourceDirectory"].PrivateData = new PrivateData(OutputSourceDirectoryCommandLine);
            this["OutputName"].PrivateData = new PrivateData(OutputNameCommandLine);
        }

        protected static void OutputSourceDirectoryCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.AppendFormat("{0} ", stringOption.Value);
        }

        protected static void OutputNameCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.AppendFormat("{0} ", stringOption.Value);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(System.Text.StringBuilder commandLineStringBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection dirsToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.OutputSourceDirectory)
            {
                dirsToCreate.Add(null, this.OutputSourceDirectory);
            }

            return dirsToCreate;
        }

        public System.Collections.Generic.Dictionary<string, string> GetOutputPaths()
        {
            System.Collections.Generic.Dictionary<string, string> map = new System.Collections.Generic.Dictionary<string, string>();
            if (this.OutputSourceDirectory != null && this.OutputName != null)
            {
                map.Add("OutputSourceFile", System.IO.Path.Combine(this.OutputSourceDirectory, this.OutputName) + ".c");
            }
            return map;
        }
    }

    class CodeGeneratorTool : CSharp.Executable
    {
        public static string VersionString
        {
            get
            {
                return "dev";
            }
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File source = new Opus.Core.File("source", "codegentool", "main.cs");
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(CodeGenTool),
                                   typeof(ExportCodeGenOptionsDelegateAttribute),
                                   typeof(LocalCodeGenOptionsDelegateAttribute),
                                   typeof(CodeGenOptions))]
    public abstract class CodeGenModule : Opus.Core.IModule, Opus.Core.IInjectModules
    {
        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.ModuleCollection GetNestedDependents(Opus.Core.Target target)
        {
            return null;
        }

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        [Opus.Core.RequiredModules]
        protected Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(
            typeof(CodeGeneratorTool)
        );

        public Opus.Core.ModuleCollection GetInjectedModules(Opus.Core.Target target)
        {
            CodeGenOptions options = this.Options as CodeGenOptions;
            string outputPath = System.IO.Path.Combine(options.OutputSourceDirectory, options.OutputName) + ".c";
            C.ObjectFile injectedFile = new C.ObjectFile(outputPath);

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}