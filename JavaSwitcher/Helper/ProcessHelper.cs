using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaSwitcher.Helper
{
    public class ProcessHelper
    {
        public static string GetJdkVersionFromRegistry()
        {
            string version = "";
            try
            {
                // 注册表路径，通常指向最新安装的JDK
                RegistryKey? rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft\Java Development Kit");
                if (rk != null)
                {
                    // 获取"CurrentVersion"的值，如"1.8"
                    version = "JDK " + rk.GetValue("CurrentVersion")?.ToString();
                    rk.Close();
                }
                else
                {
                    version = "未在注册表中找到JDK";
                }
            }
            catch (Exception ex)
            {
                version = $"从注册表获取JDK版本时出错: {ex.Message}";
            }
            return version;
        }
    }
}
