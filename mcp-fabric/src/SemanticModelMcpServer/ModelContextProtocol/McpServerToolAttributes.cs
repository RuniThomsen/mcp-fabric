using System;

namespace ModelContextProtocol
{
    [AttributeUsage(AttributeTargets.Class)]
    public class McpServerToolTypeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class McpServerToolAttribute : Attribute
    {
        public string Name { get; }
        public McpServerToolAttribute(string name)
        {
            Name = name;
        }
    }
}
