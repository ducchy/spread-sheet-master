using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpreadSheetMaster {
	/// <summary> CSVパーサー </summary>
	public class CsvParser : ICsvParser {
		#region Variables

		/// <summary> 行無視条件 </summary>
		private readonly IgnoreRowCondition[] _ignoreRowConditions;

		#endregion

		#region Methods

		/// <summary> コンストラクタ </summary>
		public CsvParser(IgnoreRowCondition[] ignoreRowConditions) {
			_ignoreRowConditions = ignoreRowConditions;
		}

		/// <summary> パース </summary>
		IReadOnlyList<IReadOnlyList<string>> ICsvParser.Parse(string csv, bool excludeHeader) {
			var records = new List<IReadOnlyList<string>>();
			var reader = new StringReader(csv);

			if (excludeHeader && reader.Peek() != -1) {
				reader.ReadLine();
			}

			var row = excludeHeader ? 2 : 1;
			while (reader.Peek() != -1) {
				var line = reader.ReadLine();

				var columns = new List<string>();
				if (line != null) {
					var empty = true;
					var elements = line.Split(',');
					for (var i = 0; i < elements.Length; i++) {
						if (elements[i] == "\"\"") {
							columns.Add(string.Empty);
							continue;
						}

						elements[i] = elements[i].TrimStart('"').TrimEnd('"');
						columns.Add(elements[i]);
						empty = false;
					}

					if (empty) {
						break;
					}
				}

				if (IsIgnoreRow(row++, columns)) {
					continue;
				}

				records.Add(columns);
			}

			return records;
		}

		/// <summary> 行無視か </summary>
		private bool IsIgnoreRow(int row, List<string> columns) {
			if (_ignoreRowConditions == null || _ignoreRowConditions.Length == 0) {
				return false;
			}

			return _ignoreRowConditions.Any(ignoreRowCondition => ignoreRowCondition.IsIgnore(row, columns));
		}

		#endregion
	}
}