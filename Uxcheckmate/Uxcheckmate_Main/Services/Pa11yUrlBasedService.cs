using System.Diagnostics;

public class Pa11yUrlBasedService
{
    public string RunPa11y(string url){
        try{
            ProcessStartInfo psi = new ProcessStartInfo{
                FileName = @"C:\Users\belen\AppData\Roaming\npm\pa11y.cmd",
                Arguments = url + " --reporter json",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"C:\Users\belen\OneDrive\Desktop\ux-20\deep-blue\Uxcheckmate\Uxcheckmate_Main" // Explicitly set working directory

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
