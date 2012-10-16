// <copyright file="Gcc.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "gcc", "Gcc.Toolchain.VersionString")]

[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.ArchiverTool, typeof(Gcc.Archiver), typeof(Gcc.ArchiverOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.CCompilerTool, typeof(Gcc.CCompiler), typeof(Gcc.CCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.CPlusPlusCompilerTool, typeof(Gcc.CPlusPlusCompiler), typeof(Gcc.CPlusPlusCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.LinkerTool, typeof(Gcc.Linker), typeof(Gcc.LinkerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.Toolchain, typeof(Gcc.Toolchain), typeof(Gcc.ToolchainOptionCollection))]

#if false
[assembly: C.RegisterToolchain(
    "gcc",
    typeof(Gcc.ToolsetInfo),
    typeof(Gcc.CCompiler), typeof(Gcc.CCompilerOptionCollection),
    typeof(Gcc.CPlusPlusCompiler), typeof(Gcc.CPlusPlusCompilerOptionCollection),
    typeof(Gcc.Linker), typeof(Gcc.LinkerOptionCollection),
    typeof(Gcc.Archiver), typeof(Gcc.ArchiverOptionCollection),
    null,null)]
#endif

namespace Gcc
{
    public class ToolsetInfo : Opus.Core.IToolsetInfo, C.ICompilerInfo
    {
        #region IToolsetInfo Members

        string Opus.Core.IToolsetInfo.BinPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            return "4.4";
        }

        #endregion

        #region ICompilerInfo Members

        string C.ICompilerInfo.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerInfo.ObjectFileSuffix
        {
            get
            {
                return ".o";
            }
        }

        string C.ICompilerInfo.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerInfo.IncludePaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
