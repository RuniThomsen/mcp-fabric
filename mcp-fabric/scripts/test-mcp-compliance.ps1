# File: test-mcp-compliance.ps1
# Tests MCP server compliance by running with --diagnostics-only mode

[Console]::Error.WriteLine("Running compliance test for MCP Server...")

# Run the server in diagnostics-only mode
[Console]::Error.WriteLine("Running MCP Server in diagnostics-only mode...")
$output = & "./publish/SemanticModelMcpServer.exe" --diagnostics-only 2>&1

# Check for successful tool registration
$toolRegistrationSuccess = $output | Select-String "Found 5 registered tools" -Quiet
$sdkComplianceSuccess = $output | Select-String "Key compliance checks" -Quiet

if ($toolRegistrationSuccess -and $sdkComplianceSuccess) {
    [Console]::Error.WriteLine("Success! MCP Server passed compliance checks.")
    [Console]::Error.WriteLine("Tool registration and SDK compliance verified.")
    
    # Print the diagnostics summary
    $output | Select-String "====== Diagnostic Summary ======" -Context 0,10 | ForEach-Object {
        [Console]::Error.WriteLine($_.Line)
        $_.Context.PostContext | ForEach-Object {
            [Console]::Error.WriteLine($_)
        }
    }
    
    exit 0
} else {
    [Console]::Error.WriteLine("Error: MCP Server failed compliance checks.")
    [Console]::Error.WriteLine("Diagnostics output:")
    $output | ForEach-Object { [Console]::Error.WriteLine($_) }
    exit 1
}
