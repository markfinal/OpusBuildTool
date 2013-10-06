// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/ICCompilerOptions.cs:ICCompilerOptions.cs -n=GccCommon -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=PrivateData

namespace GccCommon
{
    public partial class CCompilerOptionCollection
    {
        #region C.ICCompilerOptions Option delegates
        private static void DefinesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string define in definesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", define));
            }
        }
        private static void DefinesXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            configuration.Options["GCC_PREPROCESSOR_DEFINITIONS"].AddRangeUnique(definesOption.Value.ToStringArray());
        }
        private static void IncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.IToolset toolset = target.Toolset;
            C.ICompilerTool compiler = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            string switchPrefix = compiler.IncludePathCompilerSwitches[1];
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", switchPrefix, includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("{0}{1}", switchPrefix, includePath));
                }
            }
        }
        private static void IncludePathsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            configuration.Options["HEADER_SEARCH_PATHS"].AddRangeUnique(includePathsOption.Value.ToStringArray());
        }
        private static void SystemIncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.ICCompilerOptions optionCollection = sender as C.ICCompilerOptions;
            // TODO: this is a bit of a hack to cope with option collection deltas
            // since SystemIncludePaths refers to IgnoreStandardIncludePaths, just the former cannot be in a delta
            if ((sender as Opus.Core.BaseOptionCollection).Contains("IgnoreStandardIncludePaths"))
            {
                if (!optionCollection.IgnoreStandardIncludePaths)
                {
                    Opus.Core.Log.Full("System include paths not explicitly added to the build");
                    return;
                }
            }
            Opus.Core.IToolset toolset = target.Toolset;
            C.ICompilerTool compiler = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            string switchPrefix = compiler.IncludePathCompilerSwitches[0];
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", switchPrefix, includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("{0}{1}", switchPrefix, includePath));
                }
            }
        }
        private static void SystemIncludePathsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            configuration.Options["HEADER_SEARCH_PATHS"].AddRangeUnique(includePathsOption.Value.ToStringArray());
        }
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            if (null == options.OutputName)
            {
                options.ObjectFilePath = null;
                return;
            }
            Opus.Core.ValueTypeOption<C.ECompilerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    {
                        commandLineBuilder.Add("-c");
                        string objPathName = options.ObjectFilePath;
                        if (objPathName.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-o \"{0}\"", objPathName));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-o {0}", objPathName));
                        }
                    }
                    break;
                case C.ECompilerOutput.Preprocess:
                    {
                        commandLineBuilder.Add("-E");
                        string objPathName = options.PreprocessedFilePath;
                        if (objPathName.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-o \"{0}\" ", objPathName));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-o {0} ", objPathName));
                        }
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
            }
        }
        private static void OutputTypeXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            // TODO: not sure what this should do to preprocess files only
        }
        private static void DebugSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
        }
        private static void DebugSymbolsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var debugSymbols = option as Opus.Core.ValueTypeOption<bool>;
            var debugSymbolsOption = configuration.Options["GCC_GENERATE_DEBUGGING_SYMBOLS"];
            if (debugSymbols.Value)
            {
                debugSymbolsOption.AddUnique("YES");
            }
            else
            {
                debugSymbolsOption.AddUnique("NO");
            }
            if (debugSymbolsOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one debug symbol generation option has been set");
            }
        }
        private static void WarningsAsErrorsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (warningsAsErrorsOption.Value)
            {
                commandLineBuilder.Add("-Werror");
            }
        }
        private static void WarningsAsErrorsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var warningsAsErrors = option as Opus.Core.ValueTypeOption<bool>;
            var warningsAsErrorsOption = configuration.Options["GCC_TREAT_WARNINGS_AS_ERRORS"];
            if (warningsAsErrors.Value)
            {
                warningsAsErrorsOption.AddUnique("YES");
            }
            else
            {
                warningsAsErrorsOption.AddUnique("NO");
            }
            if (warningsAsErrorsOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one warnings as errors option has been set");
            }
        }
        private static void IgnoreStandardIncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardIncludePathsOption.Value)
            {
                commandLineBuilder.Add("-nostdinc");
                C.ICCompilerOptions options = sender as C.ICCompilerOptions;
                if (options.TargetLanguage == C.ETargetLanguage.Cxx)
                {
                    commandLineBuilder.Add("-nostdinc++");
                }
            }
        }
        private static void IgnoreStandardIncludePathsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var ignoreStandardIncludePaths = option as Opus.Core.ValueTypeOption<bool>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            if (ignoreStandardIncludePaths.Value)
            {
                otherCFlagsOption.AddUnique("-nostdinc");
                var cOptions = sender as C.ICCompilerOptions;
                // TODO: this is a bit of a hack to cope with option collection deltas
                // since SystemIncludePaths refers to IgnoreStandardIncludePaths, just the former cannot be in a delta
                if ((sender as Opus.Core.BaseOptionCollection).Contains("TargetLanguage"))
                {
                    if (cOptions.TargetLanguage == C.ETargetLanguage.Cxx)
                    {
                        otherCFlagsOption.AddUnique("-nostdinc++");
                    }
                }
            }
        }
        private static void OptimizationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EOptimization> optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
            switch (optimizationOption.Value)
            {
                case C.EOptimization.Off:
                    commandLineBuilder.Add("-O0");
                    break;
                case C.EOptimization.Size:
                    commandLineBuilder.Add("-Os");
                    break;
                case C.EOptimization.Speed:
                    commandLineBuilder.Add("-O1");
                    break;
                case C.EOptimization.Full:
                    commandLineBuilder.Add("-O3");
                    break;
                case C.EOptimization.Custom:
                    // do nothing
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized optimization option");
            }
        }
        private static void OptimizationXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var optimization = option as Opus.Core.ValueTypeOption<C.EOptimization>;
            var optimizationOption = configuration.Options["GCC_OPTIMIZATION_LEVEL"];
            switch (optimization.Value)
            {
            case C.EOptimization.Off:
                optimizationOption.AddUnique("0");
                break;
            case C.EOptimization.Size:
                optimizationOption.AddUnique("s");
                break;
            case C.EOptimization.Speed:
                optimizationOption.AddUnique("1");
                break;
            case C.EOptimization.Full:
                optimizationOption.AddUnique("3");
                break;
            case C.EOptimization.Custom:
                // nothing
                break;
            default:
                throw new Opus.Core.Exception("Unrecognized optimization option");
            }
            if (optimizationOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one optimization option has been set");
            }
        }
        private static void CustomOptimizationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(customOptimizationOption.Value);
        }
        private static void CustomOptimizationXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var customOptimizations = option as Opus.Core.ReferenceTypeOption<string>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            otherCFlagsOption.AddUnique(customOptimizations.Value);
        }
        private static void TargetLanguageCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ETargetLanguage> targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
            switch (targetLanguageOption.Value)
            {
                case C.ETargetLanguage.Default:
                    // do nothing
                    break;
                case C.ETargetLanguage.C:
                    commandLineBuilder.Add("-x c");
                    break;
                case C.ETargetLanguage.Cxx:
                    commandLineBuilder.Add("-x c++");
                    break;
                case C.ETargetLanguage.ObjectiveC:
                    commandLineBuilder.Add("-x objective-c");
                    break;
                case C.ETargetLanguage.ObjectiveCxx:
                    commandLineBuilder.Add("-x objective-c++");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized target language option");
            }
        }
        private static void TargetLanguageXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
            var inputFileType = configuration.Options["GCC_INPUT_FILETYPE"];
            switch (targetLanguageOption.Value)
            {
            case C.ETargetLanguage.Default:
                inputFileType.AddUnique("automatic");
                break;
            case C.ETargetLanguage.C:
                inputFileType.AddUnique("sourcecode.c.c");
                break;
            case C.ETargetLanguage.Cxx:
                inputFileType.AddUnique("sourcecode.cpp.cpp");
                break;
            case C.ETargetLanguage.ObjectiveC:
                inputFileType.AddUnique("sourcecode.c.objc");
                break;
            case C.ETargetLanguage.ObjectiveCxx:
                inputFileType.AddUnique("sourcecode.cpp.objcpp");
                break;
            default:
                throw new Opus.Core.Exception("Unrecognized target language option");
            }
            if (inputFileType.Count != 1)
            {
                throw new Opus.Core.Exception("More than one target language option has been set");
            }
        }
        private static void ShowIncludesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-H");
            }
        }
        private static void ShowIncludesXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var showIncludes = option as Opus.Core.ValueTypeOption<bool>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            if (showIncludes.Value)
            {
                otherCFlagsOption.AddUnique("-H");
            }
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static void AdditionalOptionsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var additionalOptions = option as Opus.Core.ReferenceTypeOption<string>;
            var splitArguments = additionalOptions.Value.Split(' ');
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            foreach (var argument in splitArguments)
            {
                otherCFlagsOption.AddUnique(argument);
            }
        }
        private static void OmitFramePointerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fomit-frame-pointer");
            }
            else
            {
                commandLineBuilder.Add("-fno-omit-frame-pointer");
            }
        }
        private static void OmitFramePointerXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var omitFramePointer = option as Opus.Core.ValueTypeOption<bool>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            if (omitFramePointer.Value)
            {
                otherCFlagsOption.AddUnique("-fomit-frame-pointer");
            }
            else
            {
                otherCFlagsOption.AddUnique("-fno-omit-frame-pointer");
            }
        }
        private static void DisableWarningsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> disableWarningsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string warning in disableWarningsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wno-{0}", warning));
            }
        }
        private static void DisableWarningsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var disableWarnings = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            var warningCFlagsOption = configuration.Options["WARNING_CFLAGS"];
            foreach (var warning in disableWarnings.Value)
            {
                // TODO: there are some named warnings, e.g.
                // -Wno-shadow = GCC_WARN_SHADOW = NO
                warningCFlagsOption.AddUnique(System.String.Format("-Wno-{0}", warning));
            }
        }
        private static void CharacterSetCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ECharacterSet> enumOption = option as Opus.Core.ValueTypeOption<C.ECharacterSet>;
            var cOptions = sender as C.ICCompilerOptions;
            switch (enumOption.Value)
            {
                case C.ECharacterSet.NotSet:
                    break;
                case C.ECharacterSet.Unicode:
                    cOptions.Defines.Add("_UNICODE");
                    cOptions.Defines.Add("UNICODE");
                    break;
                case C.ECharacterSet.MultiByte:
                    cOptions.Defines.Add("_MBCS");
                    break;
            }
        }
        private static void CharacterSetXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var characterSet = option as Opus.Core.ValueTypeOption<C.ECharacterSet>;
            var cOptions = sender as C.ICCompilerOptions;
            switch (characterSet.Value)
            {
                case C.ECharacterSet.NotSet:
                        break;
                case C.ECharacterSet.Unicode:
                    cOptions.Defines.Add("_UNICODE");
                    cOptions.Defines.Add("UNICODE");
                break;
                    case C.ECharacterSet.MultiByte:
                    cOptions.Defines.Add("_MBCS");
                break;
            }
        }
        private static void LanguageStandardCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var languageStandard = option as Opus.Core.ValueTypeOption<C.ELanguageStandard>;
            switch (languageStandard.Value)
            {
            case C.ELanguageStandard.NotSet:
                break;
            case C.ELanguageStandard.C89:
                commandLineBuilder.Add("-std=c89");
                break;
            case C.ELanguageStandard.C99:
                commandLineBuilder.Add("-std=c99");
                break;
            case C.ELanguageStandard.Cxx98:
                commandLineBuilder.Add("-std=c++98");
                break;
            case C.ELanguageStandard.Cxx11:
                commandLineBuilder.Add("-std=c++11");
                break;
            default:
                throw new Opus.Core.Exception("Unknown language standard");
            }
        }
        private static void LanguageStandardXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var languageStandard = option as Opus.Core.ValueTypeOption<C.ELanguageStandard>;
            var languageStandardOption = configuration.Options["GCC_C_LANGUAGE_STANDARD"];
            switch (languageStandard.Value)
            {
            case C.ELanguageStandard.NotSet:
                break;
            case C.ELanguageStandard.C89:
                languageStandardOption.AddUnique("c89");
                break;
            case C.ELanguageStandard.C99:
                languageStandardOption.AddUnique("c99");
                break;
            case C.ELanguageStandard.Cxx98:
                // nothing corresponding
                break;
            case C.ELanguageStandard.Cxx11:
                // nothing corresponding
                break;
            default:
                throw new Opus.Core.Exception("Unknown language standard");
            }
        }
        #endregion
        #region ICCompilerOptions Option delegates
        private static void AllWarningsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wall");
            }
        }
        private static void AllWarningsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var allWarnings = option as Opus.Core.ValueTypeOption<bool>;
            var warningCFlagsOption = configuration.Options["WARNING_CFLAGS"];
            if (allWarnings.Value)
            {
                warningCFlagsOption.AddUnique("-Wall");
            }
        }
        private static void ExtraWarningsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wextra");
            }
        }
        private static void ExtraWarningsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var extraWarnings = option as Opus.Core.ValueTypeOption<bool>;
            var warningCFlagsOption = configuration.Options["WARNING_CFLAGS"];
            if (extraWarnings.Value)
            {
                warningCFlagsOption.AddUnique("-Wextra");
            }
        }
        private static void StrictAliasingCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fstrict-aliasing");
            }
            else
            {
                commandLineBuilder.Add("-fno-strict-aliasing");
            }
        }
        private static void StrictAliasingXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var strictAliasing = option as Opus.Core.ValueTypeOption<bool>;
            var strictAliasingOption = configuration.Options["GCC_STRICT_ALIASING"];
            if (strictAliasing.Value)
            {
                strictAliasingOption.AddUnique("YES");
            }
            else
            {
                strictAliasingOption.AddUnique("NO");
            }
            if (strictAliasingOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one strict aliasing option has been set");
            }
        }
        private static void PositionIndependentCodeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fPIC");
            }
        }
        private static void PositionIndependentCodeXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var pic = option as Opus.Core.ValueTypeOption<bool>;
            // note that the logic is reversed here
            var noPICOption = configuration.Options["GCC_DYNAMIC_NO_PIC"];
            if (pic.Value)
            {
                noPICOption.AddUnique("NO");
            }
            else
            {
                noPICOption.AddUnique("YES");
            }
            if (noPICOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one no position independent code option has been set");
            }
        }
        private static void InlineFunctionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-finline-functions");
            }
            else
            {
                commandLineBuilder.Add("-fno-inline-functions");
            }
        }
        private static void InlineFunctionsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var inlineFunctions = option as Opus.Core.ValueTypeOption<bool>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            if (inlineFunctions.Value)
            {
                otherCFlagsOption.AddUnique("-finline-functions");
            }
            else
            {
                otherCFlagsOption.AddUnique("-fno-inline-functions");
            }
        }
        private static void PedanticCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-pedantic");
            }
        }
        private static void PedanticXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var pedanticWarnings = option as Opus.Core.ValueTypeOption<bool>;
            var pedanticWarningsOption = configuration.Options["GCC_WARN_PEDANTIC"];
            if (pedanticWarnings.Value)
            {
                pedanticWarningsOption.AddUnique("YES");
            }
            else
            {
                pedanticWarningsOption.AddUnique("NO");
            }
            if (pedanticWarningsOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one pedantic warnings option has been set");
            }
        }
        private static void SixtyFourBitCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> sixtyFourBitOption = option as Opus.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Add("-m64");
            }
            else
            {
                commandLineBuilder.Add("-m32");
            }
        }
        private static void SixtyFourBitXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var sixtyFourBit = option as Opus.Core.ValueTypeOption<bool>;
            var archOption = configuration.Options["ARCHS"];
            if (sixtyFourBit.Value)
            {
                archOption.AddUnique("$(NATIVE_ARCH_64_BIT)");
            }
            else
            {
                archOption.AddUnique("$(NATIVE_ARCH_32_BIT)");
            }
            if (archOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one architecture option has been set");
            }
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLineProcessor,DefinesXcodeProjectProcessor);
            this["IncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLineProcessor,IncludePathsXcodeProjectProcessor);
            this["SystemIncludePaths"].PrivateData = new PrivateData(SystemIncludePathsCommandLineProcessor,SystemIncludePathsXcodeProjectProcessor);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeXcodeProjectProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor,DebugSymbolsXcodeProjectProcessor);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLineProcessor,WarningsAsErrorsXcodeProjectProcessor);
            this["IgnoreStandardIncludePaths"].PrivateData = new PrivateData(IgnoreStandardIncludePathsCommandLineProcessor,IgnoreStandardIncludePathsXcodeProjectProcessor);
            this["Optimization"].PrivateData = new PrivateData(OptimizationCommandLineProcessor,OptimizationXcodeProjectProcessor);
            this["CustomOptimization"].PrivateData = new PrivateData(CustomOptimizationCommandLineProcessor,CustomOptimizationXcodeProjectProcessor);
            this["TargetLanguage"].PrivateData = new PrivateData(TargetLanguageCommandLineProcessor,TargetLanguageXcodeProjectProcessor);
            this["ShowIncludes"].PrivateData = new PrivateData(ShowIncludesCommandLineProcessor,ShowIncludesXcodeProjectProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsXcodeProjectProcessor);
            this["OmitFramePointer"].PrivateData = new PrivateData(OmitFramePointerCommandLineProcessor,OmitFramePointerXcodeProjectProcessor);
            this["DisableWarnings"].PrivateData = new PrivateData(DisableWarningsCommandLineProcessor,DisableWarningsXcodeProjectProcessor);
            this["CharacterSet"].PrivateData = new PrivateData(CharacterSetCommandLineProcessor,CharacterSetXcodeProjectProcessor);
            this["LanguageStandard"].PrivateData = new PrivateData(LanguageStandardCommandLineProcessor,LanguageStandardXcodeProjectProcessor);
            this["AllWarnings"].PrivateData = new PrivateData(AllWarningsCommandLineProcessor,AllWarningsXcodeProjectProcessor);
            this["ExtraWarnings"].PrivateData = new PrivateData(ExtraWarningsCommandLineProcessor,ExtraWarningsXcodeProjectProcessor);
            this["StrictAliasing"].PrivateData = new PrivateData(StrictAliasingCommandLineProcessor,StrictAliasingXcodeProjectProcessor);
            this["PositionIndependentCode"].PrivateData = new PrivateData(PositionIndependentCodeCommandLineProcessor,PositionIndependentCodeXcodeProjectProcessor);
            this["InlineFunctions"].PrivateData = new PrivateData(InlineFunctionsCommandLineProcessor,InlineFunctionsXcodeProjectProcessor);
            this["Pedantic"].PrivateData = new PrivateData(PedanticCommandLineProcessor,PedanticXcodeProjectProcessor);
            this["SixtyFourBit"].PrivateData = new PrivateData(SixtyFourBitCommandLineProcessor,SixtyFourBitXcodeProjectProcessor);
        }
    }
}
