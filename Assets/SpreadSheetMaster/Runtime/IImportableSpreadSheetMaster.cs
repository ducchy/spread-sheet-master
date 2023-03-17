namespace SpreadSheetMaster
{
	using System.Collections.Generic;

	public interface IImportableSpreadSheetMaster
	{
		string spreadSheetId { get; }
		string sheetId { get; }
		string sheetName { get; }

		void Import(IReadOnlyList<IReadOnlyList<string>> records);
		
		void OverwriteSpreadSheetId(string spreadSheetId);
		void ClearOverwriteSpreadSheetId();
	}
}
