namespace SpreadSheetMaster
{
	using System.Collections.Generic;
	using System.IO;

	public class CsvParser
	{
		public IReadOnlyList<IReadOnlyList<string>> Perse(string csv, bool excludeHeader)
		{
			List<IReadOnlyList<string>> records = new List<IReadOnlyList<string>>();
			var reader = new StringReader(csv);

			if (excludeHeader && reader.Peek() != -1)
				reader.ReadLine();

			while (reader.Peek() != -1)
			{
				var line = reader.ReadLine();
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
	}
}
