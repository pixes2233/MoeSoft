using System.Diagnostics;
using System.Text;
using System.IO;

namespace MoeSoft.Services;


public class PythonService
{
    private readonly string _basePath;


    public PythonService()
    {
        _basePath =
            AppDomain.CurrentDomain.BaseDirectory;
    }


    public async Task<string> RunPython(
        string script,
        params string[] arguments)
    {

        string pythonPath =
            Path.Combine(
                _basePath,
                "Scrap",
                "PythonRuntime",
                "python.exe"
            );


        string scriptPath =
            Path.Combine(
                _basePath,
                "Scrap",
                script
            );


        ProcessStartInfo psi = new()
        {
            FileName = pythonPath,

            RedirectStandardOutput = true,

            RedirectStandardError = true,

            UseShellExecute = false,

            CreateNoWindow = true,

            StandardOutputEncoding =
                Encoding.UTF8
        };

        psi.ArgumentList.Add(scriptPath);
        foreach (string argument in arguments)
        {
            psi.ArgumentList.Add(argument);
        }


        using Process process = new();

        process.StartInfo = psi;

        process.Start();


        // 同时读取两个重定向流，避免任一管道写满后阻塞子进程。
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        await Task.WhenAll(outputTask, errorTask);

        if (process.ExitCode != 0)
        {
            string error = await errorTask;
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(error)
                    ? $"Python 脚本退出，代码：{process.ExitCode}。"
                    : error);
        }

        return await outputTask;
    }
}