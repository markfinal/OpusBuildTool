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
#if BAM_V2
using System.Linq;
#endif
namespace VisualStudioProcessor
{
#if BAM_V2
    public abstract class BaseAttribute :
        System.Attribute
    {
        public enum TargetGroup
        {
            Settings,
            Configuration
        }

        protected BaseAttribute(
            string property,
            bool inheritExisting,
            TargetGroup targetGroup)
        {
            this.Property = property;
            this.InheritExisting = inheritExisting;
            this.Target = targetGroup;
        }

        public string Property
        {
            get;
            private set;
        }

        public bool InheritExisting
        {
            get;
            private set;
        }

        public TargetGroup Target
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class EnumAttribute :
        BaseAttribute
    {
        public enum EMode
        {
            AsString,
            AsInteger,
            AsIntegerWithPrefix,
            VerbatimString,
            Empty,
            NoOp
        }

        public EnumAttribute(
            object key,
            string property,
            EMode mode,
            bool inheritExisting = false,
            string verbatimString = null,
            string prefix = null,
            TargetGroup target = TargetGroup.Settings)
            :
            base(property, inheritExisting, target)
        {
            this.Key = key as System.Enum;
            this.Mode = mode;
            this.VerbatimString = verbatimString;
            this.Prefix = prefix;
        }

        public System.Enum Key
        {
            get;
            private set;
        }

        public EMode Mode
        {
            get;
            private set;
        }

        public string VerbatimString
        {
            get;
            private set;
        }

        public string Prefix
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
            string command_switch,
            bool inheritExisting = false,
            bool ignored = false,
            TargetGroup target = TargetGroup.Settings)
            :
            base(command_switch, inheritExisting, target)
        {
            this.Ignored = ignored;
        }

        public bool Ignored
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PathArrayAttribute :
        BaseAttribute
    {
        public PathArrayAttribute(
            string command_switch,
            bool inheritExisting = false,
            TargetGroup target = TargetGroup.Settings)
            :
            base(command_switch, inheritExisting, target)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class StringAttribute :
        BaseAttribute
    {
        public StringAttribute(
            string command_switch,
            bool inheritExisting = false,
            TargetGroup target = TargetGroup.Settings)
            :
            base(command_switch, inheritExisting, target)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class StringArrayAttribute :
        BaseAttribute
    {
        public StringArrayAttribute(
            string command_switch,
            bool inheritExisting = false,
            TargetGroup target = TargetGroup.Settings)
            :
            base(command_switch, inheritExisting, target)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class BoolAttribute :
        BaseAttribute
    {
        public BoolAttribute(
            string property,
            bool inheritExisting = false,
            bool inverted = false,
            TargetGroup target = TargetGroup.Settings)
            :
            base(property, inheritExisting, target)
        {
            this.Inverted = inverted;
        }

        public BoolAttribute(
            string property,
            string truth,
            string falisy,
            bool inheritExisting = false,
            TargetGroup target = TargetGroup.Settings)
            :
            base(property, inheritExisting, target)
        {
            this.Inverted = false;
            this.Truth = truth;
            this.Falisy = falisy;
        }

        public bool Inverted
        {
            get;
            private set;
        }

        public string Truth
        {
            get;
            private set;
        }

        public string Falisy
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PreprocessorDefinesAttribute :
        BaseAttribute
    {
        public PreprocessorDefinesAttribute(
            string command_switch,
            bool inheritExisting = false)
            :
            base(command_switch, inheritExisting, TargetGroup.Settings)
        { }
    }

    public static class VSSolutionConversion
    {
        public static void
        Convert(
            Bam.Core.Settings settings,
            System.Type real_settings_type,
            Bam.Core.Module module, // cannot use settings.Module as this may be null for per-file settings
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            VSSolutionBuilder.VSProjectConfiguration vsConfig,
            string condition = null)
        {
            foreach (var settings_interface in settings.Interfaces())
            {
                foreach (var interface_property in settings_interface.GetProperties())
                {
                    // must use the fully qualified property name from the originating interface
                    // to look for the instance in the concrete settings class
                    // this is to allow for the same property leafname to appear in multiple interfaces
                    var full_property_interface_name = System.String.Join(
                        ".",
                        new[] { interface_property.DeclaringType.FullName, interface_property.Name }
                    );
                    var value_settings_property = settings.Properties.First(
                        item => full_property_interface_name == item.Name
                    );
                    var property_value = value_settings_property.GetValue(settings);
                    if (null == property_value)
                    {
                        continue;
                    }
                    var attribute_settings_property = Bam.Core.Settings.FindProperties(real_settings_type).First(
                        item => full_property_interface_name == item.Name
                    );
                    var attributeArray = attribute_settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
                        throw new Bam.Core.Exception(
                            "No VisualStudioProcessor attributes for property {0} in module {1}",
                            full_property_interface_name,
                            module.ToString()
                        );
                    }
                    if (attributeArray.First() is EnumAttribute)
                    {
                        var associated_attribute = attributeArray.First(
                            item => (item as EnumAttribute).Key.Equals(property_value)) as EnumAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }
                        System.Diagnostics.Debug.Assert(!associated_attribute.InheritExisting);
                        switch (associated_attribute.Mode)
                        {
                            case EnumAttribute.EMode.AsString:
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    property_value.ToString(),
                                    condition: condition
                                );
                                break;

                            case EnumAttribute.EMode.AsInteger:
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    ((int)property_value).ToString("D"),
                                    condition: condition
                                );
                                break;

                            case EnumAttribute.EMode.AsIntegerWithPrefix:
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    System.String.Format(
                                        "{0}{1}",
                                        associated_attribute.Prefix,
                                        ((int)property_value).ToString("D")
                                    ),
                                    condition: condition
                                );
                                break;

                            case EnumAttribute.EMode.VerbatimString:
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    associated_attribute.VerbatimString,
                                    condition: condition
                                );
                                break;

                            case EnumAttribute.EMode.Empty:
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    "",
                                    condition: condition
                                );
                                break;

                            case EnumAttribute.EMode.NoOp:
                                break;

                            default:
                                throw new Bam.Core.Exception(
                                    "Unhandled enum mode, {0}",
                                    associated_attribute.Mode.ToString()
                                );
                        }
                    }
                    else if (attributeArray.First() is PathAttribute)
                    {
                        var associated_attribute = attributeArray.First() as PathAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }

                        if (associated_attribute.Ignored)
                        {
                            continue;
                        }

                        // TODO: this will add an absolute path
                        // not sure how to set that it's relative to a VS macro
                        vsSettingsGroup.AddSetting(
                            associated_attribute.Property,
                            property_value as Bam.Core.TokenizedString,
                            condition: condition,
                            isPath: true
                        );

                        var prop = vsConfig.GetType().GetProperty(associated_attribute.Property);
                        if (null != prop)
                        {
                            var setter = prop.GetSetMethod();
                            if (null != setter)
                            {
                                setter.Invoke(vsConfig, new[] { property_value });
                            }
                        }
                    }
                    else if (attributeArray.First() is PathArrayAttribute)
                    {
                        var associated_attribute = attributeArray.First() as BaseAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }
                        vsSettingsGroup.AddSetting(
                            associated_attribute.Property,
                            property_value as Bam.Core.TokenizedStringArray,
                            condition: condition,
                            inheritExisting: associated_attribute.InheritExisting,
                            arePaths: true
                        );
                    }
                    else if (attributeArray.First() is StringAttribute)
                    {
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is StringArrayAttribute)
                    {
                        var associated_attribute = attributeArray.First() as BaseAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }
                        vsSettingsGroup.AddSetting(
                            associated_attribute.Property,
                            property_value as Bam.Core.StringArray,
                            condition: condition,
                            inheritExisting: associated_attribute.InheritExisting
                        );
                    }
                    else if (attributeArray.First() is BoolAttribute)
                    {
                        var value = (bool)property_value;
                        var associated_attribute = attributeArray.First() as BoolAttribute;
                        if (associated_attribute.Inverted)
                        {
                            value = !value;
                        }
                        if (associated_attribute.Target == BaseAttribute.TargetGroup.Settings)
                        {
                            if (null == associated_attribute.Truth && null == associated_attribute.Falisy)
                            {
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    value,
                                    condition: condition
                                );
                            }
                            else
                            {
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    value ? associated_attribute.Truth : associated_attribute.Falisy,
                                    condition: condition
                                );
                            }
                        }
                        else if (associated_attribute.Target == BaseAttribute.TargetGroup.Configuration)
                        {
                            var prop = vsConfig.GetType().GetProperty(associated_attribute.Property);
                            var setter = prop.GetSetMethod();
                            if (null == associated_attribute.Truth && null == associated_attribute.Falisy)
                            {
                                setter.Invoke(
                                    vsConfig,
                                    new[] { property_value }
                                );
                            }
                            else
                            {
                                setter.Invoke(
                                    vsConfig,
                                    new[] { value ? associated_attribute.Truth : associated_attribute.Falisy }
                                );
                            }
                        }
                    }
                    else if (attributeArray.First() is PreprocessorDefinesAttribute)
                    {
                        var associated_attribute = attributeArray.First() as BaseAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }
                        vsSettingsGroup.AddSetting(
                            associated_attribute.Property,
                            property_value as C.PreprocessorDefinitions,
                            condition: condition,
                            inheritExisting: associated_attribute.InheritExisting
                        );
                    }
                    else
                    {
                        throw new Bam.Core.Exception(
                            "Unhandled attribute {0} for property {1} in {2}",
                            attributeArray.First().ToString(),
                            attribute_settings_property.Name,
                            module.ToString()
                        );
                    }
                }
            }
        }
    }
#endif

    public static class Conversion
    {
        public static void
        Convert(
            System.Type conversionClass,
            Bam.Core.Settings settings,
            Bam.Core.Module module,
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition)
        {
            var moduleType = typeof(Bam.Core.Module);
            var vsSettingsGroupType = typeof(VSSolutionBuilder.VSSettingsGroup);
            var stringType = typeof(string);
            foreach (var i in settings.Interfaces())
            {
                var method = conversionClass.GetMethod("Convert", new[] { i, moduleType, vsSettingsGroupType, stringType });
                if (null == method)
                {
                    throw new Bam.Core.Exception("Unable to locate method {0}.Convert({1}, {2}, {3})",
                        conversionClass.ToString(),
                        i.ToString(),
                        moduleType,
                        vsSettingsGroupType,
                        stringType);
                }
                try
                {
                    method.Invoke(null, new object[] { settings, module, vsSettingsGroup, condition });
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    throw new Bam.Core.Exception(exception.InnerException, "VisualStudio conversion error:");
                }
            }
        }
    }
}
