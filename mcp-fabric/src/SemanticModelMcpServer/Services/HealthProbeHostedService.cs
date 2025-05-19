using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SemanticModelMcpServer
{
    /// <summary>
    /// Tiny HTTP server that replies 200 OK on /health.
    /// Runs side-by-side with the JSON-RPC stdio transport.
    /// </summary>
    internal sealed class HealthProbeHostedService : BackgroundService
    {
        private readonly ILogger<HealthProbeHostedService> _logger;
        private readonly HttpListener _listener = new();
        private readonly int _port = 8080;
        private bool _isListening;

        public HealthProbeHostedService(ILogger<HealthProbeHostedService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var prefix = $"http://localhost:{_port}/health/";
                _listener.Prefixes.Add(prefix);
                _listener.Start();
                _isListening = true;
                _logger.LogInformation("Health probe listening at {Prefix}", prefix);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var ctx = await _listener.GetContextAsync(stoppingToken);
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.OutputStream.WriteAsync(
                        Encoding.UTF8.GetBytes("OK"), stoppingToken);
                    ctx.Response.Close();
                }
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 5) // Access denied
            {
                _logger.LogWarning(
                    "Health probe disabled: cannot register HTTP prefix (Access is denied). " +
                    "Run elevated or register a URL ACL to enable health checks.");

                try { await Task.Delay(Timeout.Infinite, stoppingToken); }
                catch (TaskCanceledException) { /* normal shutdown */ }
            }
        }

        public override void Dispose()
        {
            if (_isListening)
            {
                try { _listener.Stop(); _listener.Close(); }
                catch (ObjectDisposedException) { /* already disposed */ }
            }
            base.Dispose();
        }
    }
}