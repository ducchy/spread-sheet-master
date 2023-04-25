namespace SpreadSheetMaster
{
	using System.Collections.Generic;

	public interface IImportableSpreadSheetMaster
	{
		string className { get; }
		string spreadSheetId { get; }
		string sheetId { get; }
		string sheetName { get; }

		void Import(IReadOnlyList<IReadOnlyList<string>> records, ImportMasterInfo importInfo);
		
		void OverwriteSpreadSheetId(string spreadSheetId);
		void ClearOverwriteSpreadSheetId();
	}
}
