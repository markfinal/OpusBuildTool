// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(QtCommon.MocFileCollection mocFileCollection, out bool success)
        {
            Opus.Core.IModule mocFileCollectionModule = mocFileCollection as Opus.Core.IModule;
            Opus.Core.DependencyNode node = mocFileCollectionModule.OwningNode;
            Opus.Core.Target target = node.Target;

            MakeFileVariableDictionary dependents = new MakeFileVariableDictionary();
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                MakeFileData data = childNode.Data as MakeFileData;
                // TODO: handle this better for more dependents
                dependents.Append(data.VariableDictionary);
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            // no output paths because this rule has no recipe
            MakeFileRule rule = new MakeFileRule(null, QtCommon.OutputFileFlags.MocGeneratedSourceFileCollection, node.UniqueModuleName, null, dependents, null, null);
            if (null == node.Parent)
            {
                // phony target
                rule.TargetIsPhony = true;
            }
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);        
            }

            success = true;

            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            return returnData;
        }
    }
}