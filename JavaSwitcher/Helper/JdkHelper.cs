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
                @"C:\Program Files\Java",
                @"C:\Program Files (x86)\Java",
                @"C:\Program Files\Microsoft\jdk",
                @"C:\Program Files (x86)\Microsoft\jdk"
            };
                foreach (var baseDir in possibleDirs.Where(d => !string.IsNullOrWhiteSpace(d)))
                {
                    if (Directory.Exists(baseDir))
                    {
                        foreach (var dir in Directory.GetDirectories(baseDir))
                        {
                            if (IsJdkPathValid(dir) && !jdks.Any(s => s.JavaPath.Equals(dir)))
                                jdks.Add(new Jdk(Path.GetFileName(dir), dir));
                        }
                        if (IsJdkPathValid(baseDir) && !jdks.Any(s => s.JavaPath.Equals(baseDir)))
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
                        if (IsJdkPathValid(dir) && !jdks.Any(s => s.JavaPath.Equals(dir)))
                            jdks.Add(new Jdk(Path.GetFileName(dir), dir));
                    }
                }
                // 检查其他可能的JDK位置
                var additionalPaths = new[] { "/opt/java/openjdk", "/usr/local/openjdk", "/usr/java" };
                foreach (var additionalPath in additionalPaths.Where(Directory.Exists))
                {
                    foreach (var dir in Directory.GetDirectories(additionalPath))
                    {
                        if (IsJdkPathValid(dir) && !jdks.Any(s => s.JavaPath.Equals(dir)))
                            jdks.Add(new Jdk(Path.GetFileName(dir), dir));
                    }
                }
            }
            return jdks.Distinct().OrderByDescending(j => ParseVersion(j.Name)).ToList();
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
                return Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine) ??
                       Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User);
            }
            else
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = "-c \"readlink -f $(which java) | xargs dirname | xargs dirname\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                if (string.IsNullOrWhiteSpace(output)) return null;
                return output;
            }
        }

        /// <summary>
        /// 获取当前正在使用的Java版本
        /// </summary>
        /// <returns>Java版本信息字符串</returns>
        public static string? GetCurrentJavaVersion()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows系统通过JAVA_HOME环境变量找到java.exe执行版本查询命令
                var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine) ??
                               Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User);

                if (!string.IsNullOrEmpty(javaHome))
                {
                    var javaExePath = Path.Combine(javaHome, "bin", "java.exe");
                    if (File.Exists(javaExePath))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = javaExePath,
                            Arguments = "-version",
                            RedirectStandardOutput = false, // Java的版本信息输出到标准错误流
                            RedirectStandardError = true,
                            UseShellExecute = false
                        };

                        using var process = Process.Start(psi);
                        string output = process.StandardError.ReadToEnd().Trim();
                        process.WaitForExit();
                        return ExtractVersionFromOutput(output);
                    }
                }

                // 如果无法通过JAVA_HOME找到，尝试直接使用PATH中的java命令
                var psiFallback = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c java -version",
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using var processFallback = Process.Start(psiFallback);
                string outputFallback = processFallback.StandardError.ReadToEnd().Trim();
                processFallback.WaitForExit();
                return ExtractVersionFromOutput(outputFallback);
            }
            else
            {
                // Linux/Mac系统直接使用java命令
                var psi = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = "-c \"java -version 2>&1\"",
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                };

                using var process = Process.Start(psi);
                string output = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();
                return ExtractVersionFromOutput(output);
            }
        }

        /// <summary>
        /// 从Java版本输出中提取版本号
        /// </summary>
        /// <param name="output">Java -version命令的输出</param>
        /// <returns>提取的版本号字符串</returns>
        private static string? ExtractVersionFromOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return null;

            // 示例输出："openjdk version \"17.0.2\" 2022-01-18" 或 "java version \"1.8.0_321\""
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("version"))
                {
                    // 使用正则表达式匹配版本号，例如 "17.0.2" 或 "1.8.0_321"
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"""([^""]+)""");
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            return null;
        }

        public static string SetJdk(string jdkPath, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var currentJavaHome = Environment.GetEnvironmentVariable("JAVA_HOME", target);
                    var currentPath = Environment.GetEnvironmentVariable("Path", target) ?? "";
                    var javaBin = Path.Combine(jdkPath, "bin");

                    // 分割路径并移除旧的JDK/JRE路径
                    var pathParts = currentPath.Split(';').Where(p => !string.IsNullOrEmpty(p)).ToList();
                    var newPathParts = new List<string>();

                    foreach (var part in pathParts)
                    {
                        var trimmedPart = part.Trim();
                        // 检查是否是JDK/JRE的bin目录
                        if (trimmedPart.Contains(currentJavaHome))
                        {
                            continue; // 跳过旧的JDK/JRE路径
                        }
                        newPathParts.Add(trimmedPart);
                    }

                    // 在最前面添加新的JDK路径以确保优先级
                    newPathParts.Insert(0, javaBin);

                    var newPath = string.Join(";", newPathParts);
                    Environment.SetEnvironmentVariable("JAVA_HOME", jdkPath, target);
                    Environment.SetEnvironmentVariable("Path", newPath, target);

                    // 验证设置是否成功
                    var verifyJavaHome = Environment.GetEnvironmentVariable("JAVA_HOME", target);
                    var verifyPath = Environment.GetEnvironmentVariable("Path", target);

                    if (verifyJavaHome == jdkPath && verifyPath.Contains(javaBin))
                    {
                        return $"Successfully set JDK/JRE to: {jdkPath}\nJAVA_HOME updated and PATH adjusted.";
                    }
                    else
                    {
                        return $"Warning: Failed to properly set JDK/JRE to: {jdkPath}\nCurrent JAVA_HOME: {verifyJavaHome}";
                    }
                }
                catch (Exception ex)
                {
                    return $"Error setting JDK/JRE: {ex.Message}";
                }
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

        public static string SetAllJdk(string jdkPath)
        {
            SetJdk(jdkPath, EnvironmentVariableTarget.User);
            return SetJdk(jdkPath, EnvironmentVariableTarget.Machine);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg,
UIntPtr wParam, string lParam);
        const int HWND_BROADCAST = 0xffff;
        const uint WM_SETTINGCHANGE = 0x001a;

        public static void RefreshEnvironmentVariables()
        {
            SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
        }

        private static Version ParseVersion(string versionString)
        {
            try
            {
                // 提取数字部分
                var numericVersion = System.Text.RegularExpressions.Regex.Replace(versionString, @"[^\d\.]", "");
                if (string.IsNullOrEmpty(numericVersion))
                    return new Version(0, 0);

                // 确保版本格式正确
                var parts = numericVersion.Split('.');
                if (parts.Length >= 2)
                {
                    if (int.TryParse(parts[0], out int major) && int.TryParse(parts[1], out int minor))
                    {
                        return new Version(major, minor);
                    }
                }
                return new Version(0, 0);
            }
            catch
            {
                return new Version(0, 0);
            }
        }
    }
}
