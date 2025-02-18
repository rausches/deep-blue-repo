using System.Diagnostics;

public class Pa11yUrlBasedService
{
    public string RunPa11y(string url){
        try{
            ProcessStartInfo psi = new ProcessStartInfo{
                FileName = "pa11y",
                Arguments = url + " --json",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = new Process { StartInfo = psi }){
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result;
            }
        }catch (Exception ex){
            return $"Error running Pa11y: {ex.Message}";
        }
    }
}
