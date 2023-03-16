namespace SpreadSheetMaster
{
    public class SheetUrlBuilder
    {
        public SheetUrlBuilder()
        {
        }
        
        public string BuildExportUrl(string spreadSheetId, string sheetId)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadSheetId}/export?format=csv&gid={sheetId}";
        }
        
        public string BuildExportUrlBySheetName(string spreadSheetId, string sheetName)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadSheetId}/gviz/tq?tqx=out:csv&sheet={sheetName}";
        }
        
        public string BuildEditUrl(string spreadSheetId, string sheetId)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadSheetId}/edit?gid={sheetId}";
        }
    }
}