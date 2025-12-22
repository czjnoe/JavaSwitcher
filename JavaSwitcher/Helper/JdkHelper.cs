using JavaSwitcher.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JavaSwitcher.Helper
{
    public static class JdkHelper
    {
        public static List<Jdk> FindJdks(string? searchPath = null)
        {
            var jdks = new List<Jdk>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var possibleDirs = new[]
                {
                Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User),
                Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine),
                @"C:\\Program Files\\Java",
                @"C:\\Program Files (x86)\\Java"
            };
                foreach (var baseDir in possibleDirs.Where(d => !string.IsNullOrWhiteSpace(d)))
                {
                    if (Directory.Exists(baseDir))
                    {
                        foreach (var dir in Directory.GetDirectories(baseDir))
                        {
                            if (IsJdkPathValid(dir))
                                jdks.Add(new Jdk(Path.GetFileName(dir), dir));
                        }
                        if (IsJdkPathValid(baseDir) && !jdks.Any(s => s.Equals(baseDir)))
                            jdks.Add(new Jdk(Path.GetFileName(baseDir), baseDir));
                    }
                }
            }
            else
            {
                string path = searchPath ?? "/usr/lib/jvm";
                if (Directory.Exists(path))
                {
                    foreach (var dir in Directory.GetDirectories(path))
                    {
                        if (IsJdkPathValid(dir))
                            jdks.Add(new Jdk(Path.GetFileName(dir), dir));
                    }
                }
            }
            return jdks.Distinct().ToList();
        }

        public static bool IsJdkPathValid(string jdkPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return File.Exists(Path.Combine(jdkPath, "bin", "java.exe"));
            }
            else
            {
                return File.Exists(Path.Combine(jdkPath, "bin", "java"));
            }
        }

        public static string? GetCurrentJdk()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User);
            }
            else
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = "-c \"update-alternatives --query java | grep ^Value: | cut -d' ' -f2\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                if (string.IsNullOrWhiteSpace(output)) return null;
                var dir = Path.GetDirectoryName(Path.GetDirectoryName(output));
                return dir;
            }
        }

        public static string SetJdk(string jdkPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Environment.SetEnvironmentVariable("JAVA_HOME", jdkPath, EnvironmentVariableTarget.Machine);
                var path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) ?? "";
                var javaBin = Path.Combine(jdkPath, "bin");
                var pathParts = path.Split(';').Select(p => p.Trim()).ToList();
                if (!pathParts.Any(p => string.Equals(p, javaBin, StringComparison.OrdinalIgnoreCase)))
                {
                    path = javaBin + ";" + path;
                    Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Machine);
                    return $"JAVA_HOME: {jdkPath}";
                }
                return $"JAVA_HOME: {jdkPath}";
            }
            else
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"sudo update-alternatives --set java {jdkPath}/bin/java\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return string.IsNullOrWhiteSpace(error) ? output : output + "\n" + error;
            }
        }
    }
}
