// <copyright file="ForceDefinitionFileUpdateAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(Bam.ForceDefinitionFileUpdateAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class ForceDefinitionFileUpdateAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-forcedefinitionupdate";
            }
        }

        public string Description
        {
            get
            {
                return "Force an update of the definition files read in";
            }
        }

        public bool
        Execute()
        {
            Core.State.ForceDefinitionFileUpdate = true;
            return true;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}