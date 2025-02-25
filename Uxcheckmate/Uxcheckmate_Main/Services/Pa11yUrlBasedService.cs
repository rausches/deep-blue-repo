using System.Diagnostics;

public class Pa11yUrlBasedService
{
    public string RunPa11y(string url){
        try{
            ProcessStartInfo psi = new ProcessStartInfo{
                // Pa11y is installed globally, so we can run it from the command line
                // If you installed Pa11y locally, you would need to provide the full path to the pa11y.cmd file
                FileName = "node",
                // Add the --reporter json flag to get the output in JSON format
                Arguments = $"--no-warnings ../node_modules/pa11y/bin/pa11y.js {url} --reporter json",

                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,

            };
            using (Process process = new Process { StartInfo = psi }){
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                string errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (!string.IsNullOrWhiteSpace(errorOutput))
                {
                    throw new Exception(errorOutput);
                }
                return result;
            }
        }catch (Exception ex){
            return $"Error running Pa11y: {ex.Message}";
        }
    }
}