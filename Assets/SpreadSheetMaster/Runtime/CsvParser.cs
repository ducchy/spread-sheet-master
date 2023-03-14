using System.Linq;

namespace SpreadSheetMaster
{
    using System.Collections.Generic;
    using System.IO;

    public class CsvParser
    {
        private readonly IgnoreRowCondition[] _ignoreRowConditions;

        public CsvParser(IgnoreRowCondition[] ignoreRowConditions)
        {
            _ignoreRowConditions = ignoreRowConditions;
        }

        public IReadOnlyList<IReadOnlyList<string>> Parse(string csv, bool excludeHeader)
        {
            var records = new List<IReadOnlyList<string>>();
            var reader = new StringReader(csv);

            if (excludeHeader && reader.Peek() != -1)
                reader.ReadLine();

            var row = excludeHeader ? 2 : 1;
            while (reader.Peek() != -1)
            {
                var line = reader.ReadLine();

                if (IsIgnoreRow(row++))
                    continue;

                var columns = new List<string>();
                if (line != null)
                {
                    var elements = line.Split(',');
                    for (var i = 0; i < elements.Length; i++)
                    {
                        if (elements[i] == "\"\"")
                            continue;

                        elements[i] = elements[i].TrimStart('"').TrimEnd('"');
                        columns.Add(elements[i]);
                    }
                }

                records.Add(columns);
            }

            return records;
        }

        private bool IsIgnoreRow(int row)
        {
            if (_ignoreRowConditions == null || _ignoreRowConditions.Length == 0)
                return false;

            return _ignoreRowConditions.Any(ignoreRowCondition => ignoreRowCondition.IsIgnore(row));
        }
    }
}