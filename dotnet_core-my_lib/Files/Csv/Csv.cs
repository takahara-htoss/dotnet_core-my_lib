using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_core_my_lib.Files.Csv
{
    public class Csv
    {
        private string _path;

        public Csv(string path)
        {
            _path = path;
        }

        public bool IsExist => System.IO.File.Exists(_path);

        /// <summary>
        /// 1列目をキーにして、行全体を値として辞書に格納して返す
        /// 重複があれば例外をスローする
        /// </summary>
        /// <param name="encodingName"></param>
        /// <returns>1列目を</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Dictionary<string, string[]> 一列目をキーにして行全体を値として辞書に格納して返す(string encodingName = "UTF-8")
        {
            if (!IsExist)
                throw new FileNotFoundException($"CSV file not found: {_path}");
            var dict = new Dictionary<string, string[]>();
            using (var reader = new StreamReader(_path, Encoding.GetEncoding(encodingName)))
            {
                int lineCount = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine() ?? "";
                    var fields = line.Split(',').Select(field => field.Trim()).ToArray();
                    if (fields.Length >= 2)
                    {
                        // キーの重複があれば例外をスローする
                        if (dict.ContainsKey(fields[0]))
                        {
                            throw new InvalidOperationException($"重複のキーが見つかりました CSV: {fields[0]} at line {lineCount + 1}");
                        }

                        // 1列目をキー、行を値として辞書に追加
                        dict[fields[0]] = fields;
                    }
                    lineCount++;
                }
            }
            return dict;
        }

        /// <summary>
        /// 親が行、子が列の二次元配列として返す
        /// </summary>
        /// <param name="encodingName"></param>
        /// <param name="maxLines">読み込む最大行数。nullの場合は制限なし。</param>
        /// <param name="行内の全列を1列としてソートする">あまりに列数が多い場合は行全体を1列としてソートしなるべくユニークに近い形にする</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public ImmutableArray<ImmutableArray<string>> 親が行で子が列の二次元配列として返す(string encodingName = "UTF-8", int? maxLines = null, bool 行内の全列を1列としてソートする = false)
        {
            if (!IsExist)
                throw new FileNotFoundException($"CSV file not found: {_path}");
            var lines = new List<string>();
            using (var reader = new StreamReader(_path, Encoding.GetEncoding(encodingName)))
            {
                int lineCount = 0;
                while (!reader.EndOfStream)
                {
                    if (maxLines.HasValue && lineCount >= maxLines.Value)
                        break;

                    string line = reader.ReadLine() ?? "";

                    // 以下の処理は「行内の全列を1列としてソートする」用です、それ以外は末尾で再度トリムされます。
                    // 全体ソートする前にトリムする必要があるからです。
                    // 行をセルに分割しトリムをして再度結合することで、余分な空白を削除する
                    string[] cells = line.Split(',').Select(field => field.Trim()).ToArray();
                    line = string.Join(",", cells);

                    lines.Add(line);
                    lineCount++;
                }
            }

            if (行内の全列を1列としてソートする)
            {
                lines.Sort();
            }

            var result = new List<ImmutableArray<string>>();
            // 「行群列」の一次元配列を「行群セル群」の二次元配列にして返します。
            foreach (var line in lines)
            {
                ImmutableArray<string> fields = line.Split(',').Select(field => field.Trim()).ToImmutableArray();
                result.Add(fields);
            }

            return result.ToImmutableArray();
        }
    }
}
