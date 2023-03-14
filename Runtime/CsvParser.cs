using UnityEngine;

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
		
		public IReadOnlyList<IReadOnlyList<string>> Perse(string csv, bool excludeHeader)
		{
			List<IReadOnlyList<string>> records = new List<IReadOnlyList<string>>();
			var reader = new StringReader(csv);

			if (excludeHeader && reader.Peek() != -1)
				reader.ReadLine();

			int row = 1;
			while (reader.Peek() != -1)
			{
				var line = reader.ReadLine();
				
				if (IsIgnoreRow(++row))
					continue;
				
				var columns = new List<string>();
				var elements = line.Split(',');
				for (int i = 0; i < elements.Length; i++)
				{
					if (elements[i] == "\"\"")
						continue;

					elements[i] = elements[i].TrimStart('"').TrimEnd('"');
					columns.Add(elements[i]);
				}
				records.Add(columns);
			}
			return records;
		}

		private bool IsIgnoreRow(int row)
		{
			if (_ignoreRowConditions == null || _ignoreRowConditions.Length == 0)
				return false;

			foreach (var ignoreRowCondition in _ignoreRowConditions)
				if (ignoreRowCondition.IsIgnore(row))
					return true;

			return false;
		}
	}
}
