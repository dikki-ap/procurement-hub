using System.Diagnostics;

namespace ProcureHub.API.Services;

public sealed class ViteDevServerService : IHostedService, IDisposable
{
    private readonly ILogger<ViteDevServerService> _logger;
    private Process? _process;

    public ViteDevServerService(ILogger<ViteDevServerService> logger) => _logger = logger;

    public Task StartAsync(CancellationToken ct)
    {
        var frontendPath = ResolveFrontendPath();
        if (frontendPath is null)
        {
            _logger.LogWarning("frontend/ directory not found — Vite dev server will not start");
            return Task.CompletedTask;
        }

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = OperatingSystem.IsWindows() ? "cmd" : "npm",
                Arguments              = OperatingSystem.IsWindows() ? "/c npm run dev" : "run dev",
                WorkingDirectory       = frontendPath,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
            },
            EnableRaisingEvents = true,
        };

        _process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                _logger.LogInformation("[vite] {Line}", e.Data);
        };
        _process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                _logger.LogWarning("[vite] {Line}", e.Data);
        };

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        _logger.LogInformation("Vite dev server started (PID {Pid}) at {Path}", _process.Id, frontendPath);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct)
    {
        if (_process is { HasExited: false })
        {
            _process.Kill(entireProcessTree: true);
            _logger.LogInformation("Vite dev server stopped");
        }
        return Task.CompletedTask;
    }

    public void Dispose() => _process?.Dispose();

    private static string? ResolveFrontendPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "frontend");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }
}
