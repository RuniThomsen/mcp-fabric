using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class TabularEditorRunner
{
    private readonly string _tabularEditorPath;

    public TabularEditorRunner(string tabularEditorPath)
    {
        _tabularEditorPath = tabularEditorPath;
    }

    public async Task<string> ExportToTmdlAsync(string pbixPath, string outputPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _tabularEditorPath,
            Arguments = $"export \"{pbixPath}\" --output \"{outputPath}\" --format TMDL",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Tabular Editor failed with error: {error}");
        }

        return output;
    }
}