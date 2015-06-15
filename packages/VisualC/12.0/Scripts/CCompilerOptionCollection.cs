#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
using C.V2.DefaultSettings;
namespace VisualC
{
    public static partial class VSSolutionImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            System.Xml.XmlElement groupElement,
            string configuration)
        {
            var project = groupElement.OwnerDocument as VSSolutionBuilder.V2.VSProject;

            // write nothing for disabled debug symbols, otherwise the source files rebuild continually
            // and reports a warning that the pdb file does not exist
            // the IDE can write None into the .vcxproj, but this has the same behaviour
            // https://connect.microsoft.com/VisualStudio/feedback/details/833494/project-with-debug-information-disabled-always-rebuilds
            if (null != options.DebugSymbols && options.DebugSymbols == true)
            {
                project.AddToolSetting(groupElement, "DebugInformationFormat", options.DebugSymbols, configuration,
                    (setting, attributeName, builder) =>
                    {
                        builder.Append("OldStyle");
                    });
            }

            if (options.DisableWarnings.Count > 0)
            {
                project.AddToolSetting(groupElement, "DisableSpecificWarnings", options.DisableWarnings, configuration,
                    (setting, attributeName, builder) =>
                    {
                        foreach (var warning in setting)
                        {
                            builder.AppendFormat("{0};", warning);
                        }
                        builder.AppendFormat("%({0})", attributeName);
                    });
            }

            if (options.IncludePaths.Count > 0)
            {
                project.AddToolSetting(groupElement, "AdditionalIncludeDirectories", options.IncludePaths, configuration,
                    (setting, attributeName, builder) =>
                    {
                        foreach (var path in setting)
                        {
                            builder.AppendFormat("{0};", path);
                        }
                        builder.AppendFormat("%({0})", attributeName);
                    });
            }

            if (null != options.OmitFramePointer)
            {
                project.AddToolSetting(groupElement, "OmitFramePointers", options.OmitFramePointer, configuration,
                    (setting, attributeName, builder) =>
                    {
                        builder.Append(true == setting ? "true" : "false");
                    });
            }

            if (null != options.Optimization)
            {
                project.AddToolSetting(groupElement, "Optimization", options.Optimization, configuration,
                    (setting, attributeName, builder) =>
                    {
                        switch (setting)
                        {
                            case C.EOptimization.Off:
                                builder.Append("Disabled");
                                break;

                            case C.EOptimization.Size:
                                builder.Append("MinSpace");
                                break;

                            case C.EOptimization.Speed:
                                builder.Append("MaxSpeed");
                                break;

                            case C.EOptimization.Full:
                                builder.Append("Full");
                                break;
                        }
                    });
            }

            if (options.PreprocessorDefines.Count > 0)
            {
                project.AddToolSetting(groupElement, "PreprocessorDefinitions", options.PreprocessorDefines, configuration,
                    (setting, attributeName, builder) =>
                    {
                        foreach (var define in setting)
                        {
                            if (System.String.IsNullOrEmpty(define.Value))
                            {
                                builder.AppendFormat("{0};", define.Key);
                            }
                            else
                            {
                                builder.AppendFormat("{0}={1};", define.Key, define.Value);
                            }
                        }
                        builder.AppendFormat("%({0})", attributeName);
                    });
            }

            if (options.PreprocessorUndefines.Count > 0)
            {
                project.AddToolSetting(groupElement, "UndefinePreprocessorDefinitions", options.PreprocessorUndefines, configuration,
                    (setting, attributeName, builder) =>
                    {
                        foreach (var define in setting)
                        {
                            builder.AppendFormat("{0};", define);
                        }
                        builder.AppendFormat("%({0})", attributeName);
                    });
            }

            if (null != options.TargetLanguage)
            {
                project.AddToolSetting(groupElement, "CompileAs", options.TargetLanguage, configuration,
                    (setting, attributeName, builder) =>
                    {
                        switch (setting)
                        {
                            case C.ETargetLanguage.C:
                                builder.Append("CompileAsC");
                                break;

                            case C.ETargetLanguage.Cxx:
                                builder.Append("CompileAsCpp");
                                break;

                            case C.ETargetLanguage.Default:
                                builder.Append("Default");
                                break;
                        }
                    });
            }

            if (null != options.WarningsAsErrors)
            {
                project.AddToolSetting(groupElement, "TreatWarningAsError", options.WarningsAsErrors, configuration,
                    (setting, attributeName, builder) =>
                    {
                        builder.Append(true == setting ? "true" : "false");
                    });
            }

            if (null != options.OutputType)
            {
                project.AddToolSetting(groupElement, "PreprocessToFile", options.OutputType, configuration,
                    (setting, attributeName, builder) =>
                    {
                        builder.Append(setting == C.ECompilerOutput.Preprocess ? "true" : "false");
                    });
                if (!(module is Bam.Core.V2.IModuleGroup))
                {
                    project.AddToolSetting(groupElement, "ObjectFileName", options.OutputType, configuration,
                        (setting, attributeName, builder) =>
                        {
                            builder.Append(module.GeneratedPaths[C.V2.ObjectFile.Key].ToString());
                        });
                }
            }
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            var objectFile = module as C.V2.ObjectFile;
            if (true == options.DebugSymbols)
            {
                commandLine.Add("-Z7");
            }
            foreach (var warning in options.DisableWarnings)
            {
                commandLine.Add(warning);
            }
            foreach (var path in options.IncludePaths)
            {
                var formatString = path.ContainsSpace ? "-I\"{0}\"" : "-I{0}";
                commandLine.Add(System.String.Format(formatString, path));
            }
            if (true == options.OmitFramePointer)
            {
                commandLine.Add("-Oy");
            }
            else
            {
                commandLine.Add("-Oy-");
            }
            switch (options.Optimization)
            {
                case C.EOptimization.Off:
                    commandLine.Add("-Od");
                    break;
                case C.EOptimization.Size:
                    commandLine.Add("-Os");
                    break;
                case C.EOptimization.Speed:
                    commandLine.Add("-O1");
                    break;
                case C.EOptimization.Full:
                    commandLine.Add("-Ox");
                    break;
            }
            foreach (var define in options.PreprocessorDefines)
            {
                if (System.String.IsNullOrEmpty(define.Value))
                {
                    commandLine.Add(System.String.Format("-D{0}", define.Key));
                }
                else
                {
                    commandLine.Add(System.String.Format("-D{0}={1}", define.Key, define.Value));
                }
            }
            foreach (var undefine in options.PreprocessorUndefines)
            {
                commandLine.Add(System.String.Format("-U{0}", undefine));
            }
            foreach (var path in options.SystemIncludePaths)
            {
                var formatString = path.ContainsSpace ? "-I\"{0}\"" : "-I{0}";
                commandLine.Add(System.String.Format(formatString, path));
            }
            switch (options.TargetLanguage)
            {
                case C.ETargetLanguage.C:
                    commandLine.Add(System.String.Format("-Tc {0}", objectFile.InputPath.ToString()));
                    break;
                case C.ETargetLanguage.Cxx:
                    commandLine.Add(System.String.Format("-Tp {0}", objectFile.InputPath.ToString()));
                    break;
                default:
                    throw new Bam.Core.Exception("Unsupported target language");
            }
            if (true == options.WarningsAsErrors)
            {
                commandLine.Add("-WX");
            }
            switch (options.OutputType)
            {
                case C.ECompilerOutput.CompileOnly:
                    commandLine.Add("-c");
                    commandLine.Add(System.String.Format("-Fo{0}", module.GeneratedPaths[C.V2.ObjectFile.Key].ToString()));
                    break;
                case C.ECompilerOutput.Preprocess:
                    commandLine.Add("-E");
                    commandLine.Add(System.String.Format("-Fo{0}", module.GeneratedPaths[C.V2.ObjectFile.Key].ToString()));
                    break;
            }
        }

        public static void
        Convert(
            this C.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this VisualCCommon.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this VisualCCommon.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this VisualC.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this VisualC.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }
    }

namespace V2
{
    public interface ICommonCompilerOptions
    {
        bool VC12Common
        {
            get;
            set;
        }
    }

    public interface ICOnlyCompilerOptions
    {
        int VC12COnly
        {
            get;
            set;
        }
    }

    public interface ICxxOnlyCompilerOptions
    {
        Bam.Core.EPlatform VC12CxxOnly
        {
            get;
            set;
        }
    }
}
    // V2
    public class CompilerSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonCompilerOptions,
        C.V2.ICOnlyCompilerOptions,
        VisualCCommon.V2.ICommonCompilerOptions,
        VisualCCommon.V2.ICOnlyCompilerOptions,
        VisualC.V2.ICommonCompilerOptions,
        VisualC.V2.ICOnlyCompilerOptions,
        CommandLineProcessor.V2.IConvertToCommandLine,
        VisualStudioProcessor.V2.IConvertToProject
    {
        public CompilerSettings(Bam.Core.V2.Module module)
            : this(module, true)
        {
        }

        public CompilerSettings(Bam.Core.V2.Module module, bool useDefaults)
        {
            (this as C.V2.ICommonCompilerOptions).Empty();
            if (useDefaults)
            {
                (this as C.V2.ICommonCompilerOptions).Defaults(module);
            }
        }

        C.V2.EBit? C.V2.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions C.V2.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? C.V2.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? C.V2.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? C.V2.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        C.ELanguageStandard? C.V2.ICommonCompilerOptions.LanguageStandard
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.PreprocessorUndefines
        {
            get;
            set;
        }

        bool VisualCCommon.V2.ICommonCompilerOptions.VCCommonCommon
        {
            get;
            set;
        }

        int VisualCCommon.V2.ICOnlyCompilerOptions.VCCommonCOnly
        {
            get;
            set;
        }

        bool V2.ICommonCompilerOptions.VC12Common
        {
            get;
            set;
        }

        int V2.ICOnlyCompilerOptions.VC12COnly
        {
            get;
            set;
        }

        void
        CommandLineProcessor.V2.IConvertToCommandLine.Convert(
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
            (this as VisualCCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as VisualCCommon.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
            (this as VisualC.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as VisualC.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
        }

        void VisualStudioProcessor.V2.IConvertToProject.Convert(Bam.Core.V2.Module module, System.Xml.XmlElement groupElement, string configuration)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, groupElement, configuration);
        }
    }

    public sealed class CxxCompilerSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonCompilerOptions,
        C.V2.ICxxOnlyCompilerOptions,
        VisualCCommon.V2.ICommonCompilerOptions,
        VisualCCommon.V2.ICxxOnlyCompilerOptions,
        VisualC.V2.ICommonCompilerOptions,
        VisualC.V2.ICxxOnlyCompilerOptions
    {

        C.V2.EBit? C.V2.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions C.V2.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? C.V2.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? C.V2.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? C.V2.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        C.ELanguageStandard? C.V2.ICommonCompilerOptions.LanguageStandard
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.PreprocessorUndefines
        {
            get;
            set;
        }

        C.Cxx.EExceptionHandler C.V2.ICxxOnlyCompilerOptions.ExceptionHandler
        {
            get;
            set;
        }

        bool VisualCCommon.V2.ICommonCompilerOptions.VCCommonCommon
        {
            get;
            set;
        }

        string VisualCCommon.V2.ICxxOnlyCompilerOptions.VCCommonCxxOnly
        {
            get;
            set;
        }

        bool V2.ICommonCompilerOptions.VC12Common
        {
            get;
            set;
        }

        Bam.Core.EPlatform V2.ICxxOnlyCompilerOptions.VC12CxxOnly
        {
            get;
            set;
        }
    }

    public static class Configure
    {
        static Configure()
        {
            InstallPath = Bam.Core.V2.TokenizedString.Create(@"C:\Program Files (x86)\Microsoft Visual Studio 12.0", null);
        }

        public static Bam.Core.V2.TokenizedString InstallPath
        {
            get;
            private set;
        }
    }

    public abstract class CompilerBase :
        C.V2.CompilerTool
    {
        protected CompilerBase()
        {
            this.InheritedEnvironmentVariables.Add("SystemRoot");
            // temp environment variables avoid generation of _CL_<hex> temporary files in the current directory
            this.InheritedEnvironmentVariables.Add("TEMP");
            this.InheritedEnvironmentVariables.Add("TMP");

            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("BinPath", Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\VC\bin", this));
            this.Macros.Add("objext", ".obj");

            this.EnvironmentVariables.Add("PATH", new Bam.Core.V2.TokenizedStringArray(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\Common7\IDE", this)));

            this.PublicPatch(settings =>
                {
                    var compilation = settings as C.V2.ICommonCompilerOptions;
                    compilation.SystemIncludePaths.Add(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\VC\include", this));
                });
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return this.Macros["CompilerPath"];
            }
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            if (typeof(C.Cxx.V2.ObjectFile).IsInstanceOfType(module) ||
                typeof(C.Cxx.V2.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new CxxCompilerSettings();
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.V2.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.V2.CObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new CompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else
            {
                throw new Bam.Core.Exception("Could not determine type of module {0}", typeof(T).ToString());
            }
        }

        protected abstract void OverrideDefaultSettings(Bam.Core.V2.Settings settings);
    }

    public sealed class Compiler32 :
        CompilerBase
    {
        public Compiler32()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create(@"$(BinPath)\cl.exe", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.ThirtyTwo;
        }
    }

    [C.V2.RegisterCompiler("VisualC", Bam.Core.EPlatform.Windows)]
    public sealed class Compiler64 :
        CompilerBase
    {
        public Compiler64()
            : base()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create(@"$(BinPath)\x86_amd64\cl.exe", this));
            // some DLLs exist only in the 32-bit bin folder
            this.EnvironmentVariables["PATH"].Add(this.Macros["BinPath"]);
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.SixtyFour;
        }
    }
    // -V2
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        VisualCCommon.CCompilerOptionCollection
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
