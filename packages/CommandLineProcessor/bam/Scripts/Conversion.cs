#region License
// Copyright (c) 2010-2018, Mark Final
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
using System.Linq;
namespace CommandLineProcessor
{
#if BAM_V2
    public abstract class BaseAttribute :
        System.Attribute
    {
        protected BaseAttribute(
            string command_switch)
        {
            this.CommandSwitch = command_switch;
        }

        public string CommandSwitch
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class EnumAttribute :
        BaseAttribute
    {
        public EnumAttribute(
            object key,
            string command_switch)
            :
            base(command_switch)
        {
            this.Key = key as System.Enum;
        }

        public System.Enum Key
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PathAttribute :
        BaseAttribute
    {
        public PathAttribute(
            string command_switch)
            :
            base(command_switch)
        {}
    }

    public static class NativeConversion
    {
        public static Bam.Core.StringArray
        Convert(
            Bam.Core.Module module)
        {
            if (!(module as C.ObjectFileBase).PerformCompilation)
            {
                return null;
            }

            var commandLine = new Bam.Core.StringArray();
            //Bam.Core.Log.MessageAll("Module: {0}", module.ToString());
            //Bam.Core.Log.MessageAll("Settings: {0}", module.Settings.ToString());
            foreach (var settings_interface in module.Settings.Interfaces())
            {
                //Bam.Core.Log.MessageAll(settings_interface.ToString());
                foreach (var interface_property in settings_interface.GetProperties())
                {
                    var settings_property = module.Settings.Properties.First(item => item.Name.EndsWith(interface_property.Name));
                    //Bam.Core.Log.MessageAll("\t{0}", settings_property.ToString());
                    var attributeArray = settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
                        continue;
                    }
                    var property_value = settings_property.GetValue(module.Settings);
                    if (attributeArray.First() is EnumAttribute)
                    {
                        var matching_attribute_to_enum = attributeArray.FirstOrDefault(
                            item => (item as EnumAttribute).Key.Equals(property_value)
                        ) as EnumAttribute;
                        if (null == matching_attribute_to_enum)
                        {
                            throw new Bam.Core.Exception("Unable to locate enumeration mapping for {0}", interface_property.GetType().FullName);
                        }
                        commandLine.Add(matching_attribute_to_enum.CommandSwitch);
                    }
                    else if (attributeArray.First() is PathAttribute)
                    {
                        commandLine.Add(
                            System.String.Format(
                                "{0}{1}",
                                (attributeArray.First() as PathAttribute).CommandSwitch,
                                (property_value as Bam.Core.TokenizedString).ToStringQuoteIfNecessary()
                            )
                        );
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Unhandled attribute: {0}", attributeArray.First().ToString());
                    }
                }
            }
            //Bam.Core.Log.MessageAll("{0}: Executing '{1}'", module.ToString(), commandLine.ToString(' '));
            return commandLine;
        }
    }
#endif

    public static class Conversion
    {
        public static void
        Convert(
            System.Type conversionClass,
            Bam.Core.Settings toolSettings,
            Bam.Core.StringArray commandLine)
        {
            var stringArrayType = typeof(Bam.Core.StringArray);
            foreach (var i in toolSettings.Interfaces())
            {
                var method = conversionClass.GetMethod("Convert", new[] { i, stringArrayType });
                if (null == method)
                {
                    throw new Bam.Core.Exception("Unable to locate method {0}.Convert({1}, {2})",
                        conversionClass.ToString(),
                        i.ToString(),
                        stringArrayType);
                }
                var commands = new Bam.Core.StringArray();
                try
                {
                    method.Invoke(null, new object[] { toolSettings, commands });
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    throw new Bam.Core.Exception(exception.InnerException, "Command line conversion error:");
                }
                commandLine.AddRange(commands);
            }
        }
    }
}
