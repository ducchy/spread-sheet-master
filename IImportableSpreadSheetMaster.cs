namespace SpreadSheetMaster
{
	using System.Collections.Generic;

	public interface IImportableSpreadSheetMaster
	{
		string spreadSheetId { get; }
		string sheetName { get; }

		void PreImport();
		void Import(IReadOnlyList<IReadOnlyList<string>> records);
		void PostImport();
	}
}
