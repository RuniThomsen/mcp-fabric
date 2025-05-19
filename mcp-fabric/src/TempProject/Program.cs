// See https://aka.ms/new-console-template for more information
using System;
using System.Reflection;
using ModelContextProtocol;

// Print out information about McpException
var mcpExceptionType = typeof(McpException);
Console.WriteLine($"McpException class found: {mcpExceptionType != null}");

Console.WriteLine("\nConstructors:");
foreach (var constructor in mcpExceptionType.GetConstructors())
{
    Console.WriteLine($"Constructor: {constructor}");
    foreach (var param in constructor.GetParameters())
    {
        Console.WriteLine($"  Parameter: {param.Name} ({param.ParameterType.Name})");
    }
}

Console.WriteLine("\nProperties:");
foreach (var property in mcpExceptionType.GetProperties())
{
    Console.WriteLine($"Property: {property.Name} ({property.PropertyType.Name})");
}

Console.WriteLine("\nStatic properties/fields:");
foreach (var field in mcpExceptionType.GetFields(BindingFlags.Public | BindingFlags.Static))
{
    Console.WriteLine($"Field: {field.Name} ({field.FieldType.Name})");
}

// Print out information about McpErrorCode
var mcpErrorCodeType = typeof(McpErrorCode);
if (mcpErrorCodeType != null)
{
    Console.WriteLine("\nMcpErrorCode type found");
    
    if (mcpErrorCodeType.IsEnum)
    {
        Console.WriteLine("McpErrorCode is an enum with values:");
        foreach (var value in Enum.GetValues(mcpErrorCodeType))
        {
            Console.WriteLine($"  {value} ({(int)value})");
        }
    }
}
else
{
    Console.WriteLine("McpErrorCode not found");
}

Console.WriteLine("Done!");
