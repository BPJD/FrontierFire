using System;
using System.Collections.Generic;
using System.Text;

public static class CsvUtil
{
    /// <summary>
    /// RFC4180에 준하는 간단 CSV 파서:
    /// - 콤마 구분
    /// - 따옴표(")로 감싸면 콤마/줄바꿈 포함 가능
    /// - 따옴표 이스케이프는 "" (두 개) 처리
    /// </summary>
    public static List<string[]> Parse(string csvText)
    {
        var rows = new List<string[]>();
        if (string.IsNullOrEmpty(csvText)) return rows;

        var row = new List<string>();
        var field = new StringBuilder();

        bool inQuotes = false;

        for (int i = 0; i < csvText.Length; i++)
        {
            char c = csvText[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // "" => "
                    if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (c == '\r')
                {
                    // ignore \r (windows newline)
                    continue;
                }
                else if (c == '\n')
                {
                    row.Add(field.ToString());
                    field.Clear();

                    rows.Add(row.ToArray());
                    row.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
        }

        // 마지막 필드/로우 처리
        row.Add(field.ToString());
        rows.Add(row.ToArray());

        return rows;
    }
}
