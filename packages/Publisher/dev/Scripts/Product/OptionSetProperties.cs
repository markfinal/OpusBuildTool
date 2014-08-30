#region License
// Copyright 2010-2014 Mark Final
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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=Interfaces/IPublishOptions.cs
//     -n=Publisher
//     -c=OptionSet
//     -p
//     -d
//     -dd=../../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace Publisher
{
    public partial class OptionSet
    {
        #region IPublishOptions Option properties
        bool IPublishOptions.OSXApplicationBundle
        {
            get
            {
                return this.GetValueTypeOption<bool>("OSXApplicationBundle", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("OSXApplicationBundle", value);
                this.ProcessNamedSetHandler("OSXApplicationBundleSetHandler", this["OSXApplicationBundle"]);
            }
        }
        #endregion
    }
}
