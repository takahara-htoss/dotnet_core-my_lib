using System.Text;

namespace dotnet_core_my_lib.Files.Ini
{
    /// <summary>
    /// INIファイルの制御をします（UTF8）。
    /// Win32APIはUTF8に未対応なので使っていません。
    /// </summary>
    public static class IniFile
    {
        public static string Read(string section, string key, string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            bool inSection = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                // コメント行をスキップ
                if (line.StartsWith(";") || line.StartsWith("#") || line.Length == 0)
                    continue;

                // セクション判定
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    var currentSection = line[1..^1].Trim();
                    inSection = string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                // キー=値の解析
                if (inSection)
                {
                    var separatorIndex = line.IndexOf('=');
                    if (separatorIndex < 0) continue;

                    var currentKey = line[..separatorIndex].Trim();
                    if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        return line[(separatorIndex + 1)..].Trim();
                    }
                }
            }

            return string.Empty;
        }

        public static void Write(string section, string key, string value, string filePath)
        {
            var lines = File.Exists(filePath)
                ? new List<string>(File.ReadAllLines(filePath, Encoding.UTF8))
                : new List<string>();

            bool inSection = false;
            bool keyWritten = false;
            bool sectionFound = false;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (inSection && !keyWritten)
                    {
                        lines.Insert(i, $"{key}={value}");
                        keyWritten = true;
                        break;
                    }
                    var currentSection = line[1..^1].Trim();
                    inSection = string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase);
                    if (inSection) sectionFound = true;
                    continue;
                }

                if (inSection)
                {
                    var separatorIndex = line.IndexOf('=');
                    if (separatorIndex < 0) continue;

                    var currentKey = line[..separatorIndex].Trim();
                    if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"{key}={value}";
                        keyWritten = true;
                        break;
                    }
                }
            }

            if (!keyWritten)
            {
                if (!sectionFound) lines.Add($"[{section}]");
                lines.Add($"{key}={value}");
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }
    }
}
