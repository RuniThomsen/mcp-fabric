# GitHub Copilot Instructions

## Command Execution Standards

### PowerShell Command Execution

When generating PowerShell commands:

- Use semicolons (`;`) as command separators instead of ampersands (`&&`) 
- Example: `command1; command2; command3` (correct) vs `command1 && command2 && command3` (incorrect for PowerShell)
- For conditional execution:
  - Use `if ($?) { command2 }` instead of `command1 && command2`
  - Use `command1; if (-not $?) { command2 }` instead of `command1 || command2`
- For background processes, use `Start-Process` or `&` operator
- For pipeline commands, use the pipe operator (`|`) as normal

### Bash/CMD Command Execution

When generating bash or CMD commands:

- Use `&&` for conditional execution (execute second command only if first succeeds)
- Use `||` for fallback execution (execute second command only if first fails)
- Use `;` for sequential execution (execute all commands in order)

### Documentation Format

- Always include clear explanations of what each command does
- For complex commands, break them down with comments
- For commands that might have side effects, include warnings

## Best Practices

- Always consider the specific shell environment when generating commands
- Escape special characters appropriately for the target shell
- Use absolute paths when file locations are known
- Consider error handling appropriate to the shell
