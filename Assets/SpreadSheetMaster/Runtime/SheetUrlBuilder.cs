namespace SpreadSheetMaster
{
    /// <summary> スプレッドシートのURLビルダー </summary>
    public class SheetUrlBuilder
    {
        /// <summary> csv出力URLを生成 </summary>
        public string BuildExportCsvUrl(string spreadSheetId, string sheetId)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadSheetId}/export?format=csv&gid={sheetId}";
        }

        /// <summary> 編集ページへのURLを生成 </summary>
        public string BuildEditUrl(string spreadSheetId, string sheetId)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadSheetId}/edit?gid={sheetId}";
        }
    }
}