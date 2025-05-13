using System;

namespace ModelContextProtocol
{
    [AttributeUsage(AttributeTargets.Class)]
    public class McpToolAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public McpToolAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
