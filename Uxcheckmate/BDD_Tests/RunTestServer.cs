using System;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

public static class RunTestServer
{
    private static Process _server;
    public static void StartServer()
    {
        if (_server != null && !_server.HasExited){
            return;
        }
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Uxcheckmate_Main\Uxcheckmate_Main.csproj"));
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        _server = new Process { StartInfo = startInfo };
        _server.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        _server.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);
        _server.Start();
        _server.BeginOutputReadLine();
        _server.BeginErrorReadLine();
        WaitForServer("http://localhost:5000").Wait();
    }
    public static void StopServer()
    {
        if (_server != null && !_server.HasExited){
            _server.Kill();
            _server.Dispose();
            _server = null;
        }
    }
    private static async Task WaitForServer(string url, int timeoutSeconds = 15)
    {
        using var client = new HttpClient();
        var start = DateTime.Now;
        while ((DateTime.Now - start).TotalSeconds < timeoutSeconds){
            try{
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode){
                    return;
                }
            }catch{
                // Server started yet
            }
            await Task.Delay(500);
        }
        throw new Exception($"Timed out waiting for server to start at {url}");
    }
}