namespace SpreadSheetMaster
{
	using System.Collections.Generic;

	public interface IImportableSpreadSheetMaster
	{
		string spreadSheetId { get; }
		string sheetId { get; }

		void Import(IReadOnlyList<IReadOnlyList<string>> records);
	}
}
