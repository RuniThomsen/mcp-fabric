namespace SemanticModelMcpServer.Diagnostics
{
    internal static class ConsoleHelper
    {
        public static void Log(string message) => System.Console.Error.WriteLine(message);
    }
}
