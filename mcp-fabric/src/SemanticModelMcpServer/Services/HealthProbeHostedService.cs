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

        public HealthProbeHostedService(ILogger<HealthProbeHostedService> logger)
        {
            _logger = logger;
            _listener.Prefixes.Add("http://*:8080/health/");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener.Start();
            _logger.LogInformation("Health probe listening at /health");
            while (!stoppingToken.IsCancellationRequested)
            {
                var ctx = await _listener.GetContextAsync();
                ctx.Response.StatusCode = 200;
                await ctx.Response.OutputStream.WriteAsync(
                    Encoding.UTF8.GetBytes("OK"), stoppingToken);
                ctx.Response.Close();
            }
        }

        public override void Dispose()
        {
            _listener.Stop();
            _listener.Close();
            base.Dispose();
        }
    }
}