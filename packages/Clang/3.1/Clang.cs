// Automatically generated by Opus v0.50
[assembly: C.RegisterToolchain(
    "clang",
    typeof(Clang.ToolsetInfo),
    typeof(Clang.CCompiler), typeof(Clang.CCompilerOptionCollection),
    typeof(Clang.CxxCompiler), typeof(Clang.CxxCompilerOptionCollection),
    null, null,
    null, null,
    null, null)]

namespace Clang
{
    // Add modules here
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
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return @"D:\dev\Thirdparty\Clang\3.1\build\bin\Release";
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            return "3.1";
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
                return ".obj";
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
