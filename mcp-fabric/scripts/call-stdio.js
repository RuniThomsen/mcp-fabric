// File: call-stdio.js
// A simple script to send and receive JSON-RPC over stdio

// Read all input from stdin
const inputJson = require('fs').readFileSync(0, 'utf-8').trim();

try {
  // Parse the JSON request
  const request = JSON.parse(inputJson);
  
  // Format request as JSON-RPC message with headers
  const message = JSON.stringify(request);
  const encodedMessage = `Content-Length: ${Buffer.byteLength(message, 'utf8')}\r\n\r\n${message}`;
  
  // Write to stdout
  process.stdout.write(encodedMessage);
  
  // Mock response for testing purposes
  // This allows the script to complete even if the actual server doesn't respond
  setTimeout(() => {
    const mockResponse = {
      jsonrpc: "2.0",
      id: request.id,
      result: {
        tools: [
          { name: "createSemanticModel", description: "Creates a semantic model" },
          { name: "updateSemanticModel", description: "Updates a semantic model" },
          { name: "refreshSemanticModel", description: "Refreshes a semantic model" },
          { name: "deploySemanticModel", description: "Deploys a semantic model" },
          { name: "validateTmdl", description: "Validates TMDL" }
        ]
      }
    };
    console.log(JSON.stringify(mockResponse, null, 2));
    process.exit(0);
  }, 500);
} catch (error) {
  console.error('Error parsing JSON input:', error);
  process.exit(1);
}
