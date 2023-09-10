using System.Collections.Generic;

namespace SpreadSheetMaster
{
    /// <summary> CSVパーサー </summary>
    public interface ICsvParser
    {
        /// <summary> パース </summary>
        IReadOnlyList<IReadOnlyList<string>> Parse(string csv, bool excludeHeader);
    }
}