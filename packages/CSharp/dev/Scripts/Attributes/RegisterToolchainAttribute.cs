namespace CSharp
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class RegisterToolchainAttribute : Opus.Core.RegisterToolsetAttribute
    {
        public RegisterToolchainAttribute(string name, System.Type toolsetType)
            : base(name, toolsetType)
        {
            if (!typeof(IOptions).IsAssignableFrom(typeof(OptionCollection)))
            {
                throw new Opus.Core.Exception(System.String.Format("C Compiler option type '{0}' does not implement the interface {1}", typeof(OptionCollection).ToString(), typeof(IOptions).ToString()), false);
            }

            {
                System.Collections.Generic.Dictionary<System.Type, ToolAndOptions> map = new System.Collections.Generic.Dictionary<System.Type, ToolAndOptions>();
                // TODO: there is a 1-1 mapping from tool to tool? Is that ok? We could simplify that to just the optionset type
                map[typeof(Csc)] = new ToolAndOptions(typeof(Csc), typeof(OptionCollection));

                if (!Opus.Core.State.HasCategory("ToolchainTypeMap"))
                {
                    Opus.Core.State.AddCategory("ToolchainTypeMap");
                }
                Opus.Core.State.Add("ToolchainTypeMap", name, map);
            }
        }
    }
}
