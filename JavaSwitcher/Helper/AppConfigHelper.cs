using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace JavaSwitcher.Helper
{
    public static class AppSettingsHelper
    {
        private static readonly string _appSettingsPath = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "appsettings.json");

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 获取完整配置对象
        /// </summary>
        public static T GetConfig<T>() where T : class
        {
            if (!File.Exists(_appSettingsPath))
                return default;

            try
            {
                var jsonContent = File.ReadAllText(_appSettingsPath);
                return JsonSerializer.Deserialize<T>(jsonContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据键路径获取配置值
        /// </summary>
        public static T GetValue<T>(string keyPath)
        {
            if (string.IsNullOrWhiteSpace(keyPath))
                throw new ArgumentException("键路径不能为空", nameof(keyPath));

            if (!File.Exists(_appSettingsPath))
                return default;

            try
            {
                var jsonContent = File.ReadAllText(_appSettingsPath);
                var jsonNode = JsonNode.Parse(jsonContent);
                var targetNode = NavigateToNode(jsonNode, keyPath);

                return targetNode == null
                    ? default
                    : JsonSerializer.Deserialize<T>(targetNode.ToJsonString(), _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"获取配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新指定键路径的配置值
        /// </summary>
        public static void UpdateValue<T>(string keyPath, T value)
        {
            if (string.IsNullOrWhiteSpace(keyPath))
                throw new ArgumentException("键路径不能为空", nameof(keyPath));

            try
            {
                var jsonContent = File.ReadAllText(_appSettingsPath);
                var jsonNode = JsonNode.Parse(jsonContent);
                var pathSegments = keyPath.Split(':');
                var currentNode = jsonNode;

                // 导航到父节点，不存在则创建
                for (int i = 0; i < pathSegments.Length - 1; i++)
                {
                    currentNode[pathSegments[i]] ??= new JsonObject();
                    currentNode = currentNode[pathSegments[i]];
                }

                // 设置最终值
                currentNode[pathSegments[^1]] = JsonValue.Create(value);

                // 保存
                File.WriteAllText(_appSettingsPath, jsonNode.ToJsonString(_jsonOptions));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"更新配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保存整个配置对象
        /// </summary>
        public static void Save<T>(T config) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(_appSettingsPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"保存配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查配置文件是否存在
        /// </summary>
        public static bool ConfigFileExists() => File.Exists(_appSettingsPath);

        /// <summary>
        /// 导航到指定路径的节点
        /// </summary>
        private static JsonNode NavigateToNode(JsonNode rootNode, string keyPath)
        {
            var pathSegments = keyPath.Split(':');
            var currentNode = rootNode;

            foreach (var segment in pathSegments)
            {
                if (currentNode == null) return null;
                currentNode = currentNode[segment];
            }

            return currentNode;
        }
    }
}
