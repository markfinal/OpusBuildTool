// <copyright file="QtCommon.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class QtCommon : C.ThirdPartyModule
    {
        protected static string installPath;
        protected Opus.Core.StringArray includePaths = new Opus.Core.StringArray();

        public string BinPath
        {
            get;
            protected set;
        }

        public string LibPath
        {
            get;
            protected set;
        }

        public QtCommon()
        {
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_LibraryPaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_VisualCWarningLevel);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtCommon_LibraryPaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }

            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.LibraryPaths.AddAbsoluteDirectory(this.LibPath, true);
        }

        [C.ExportCompilerOptionsDelegate]
        void QtCommon_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }

            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            foreach (string includePath in this.includePaths)
            {
                compilerOptions.IncludePaths.AddAbsoluteDirectory(includePath, true);
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtCommon_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            VisualCCommon.ICCompilerOptions compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != compilerOptions)
            {
                compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }
    }
}