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
        string argument)
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

            Arguments =
                $"\"{scriptPath}\" {argument}",

            RedirectStandardOutput = true,

            RedirectStandardError = true,

            UseShellExecute = false,

            CreateNoWindow = true,

            StandardOutputEncoding =
                Encoding.UTF8
        };


        using Process process = new();

        process.StartInfo = psi;

        process.Start();


        string output =
            await process.StandardOutput.ReadToEndAsync();


        string error =
            await process.StandardError.ReadToEndAsync();


        await process.WaitForExitAsync();


        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }


        return output;
    }
}