using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinTop.Core.Services;
using SpinTop.Core.ViewModels;

namespace SpinTop.Core.Server;

public class McpServer
{
    private IHost _app;
    
    public McpServer()
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);

        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();
        
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });
        _app = builder.Build();
    }

    public async Task StartAsync()
    {
        await _app.RunAsync();
    }

    public async Task StopAsync()
    {
        await _app.StopAsync();
    }

}