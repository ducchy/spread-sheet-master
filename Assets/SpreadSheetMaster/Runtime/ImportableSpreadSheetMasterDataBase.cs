using System;
using System.Collections.Generic;

namespace SpreadSheetMaster
{
    /// <summary> インポート可能なマスタデータ基底 </summary>
    public abstract class ImportableSpreadSheetMasterDataBase : IImportableSpreadSheetMasterData
    {
        /// <summary> キー取得 </summary>
        public abstract int GetKey();

        /// <summary> インポート時のログビルダー </summary>
        private ImportMasterLogBuilder _importLogBuilder;

        /// <summary> データ設定 </summary>
        public void SetData(IReadOnlyList<string> record, ImportMasterLogBuilder importLogBuilder)
        {
            _importLogBuilder = importLogBuilder;

            SetDataInternal(record);
            SetCustomData(record);

            _importLogBuilder = null;
        }

        /// <summary> データ設定 </summary>
        protected abstract void SetDataInternal(IReadOnlyList<string> record);

        /// <summary> カスタムデータ設定 </summary>
        protected virtual void SetCustomData(IReadOnlyList<string> record)
        {
        }

        /// <summary> 範囲外インデックスか </summary>
        private bool IsIndexOutOfRange(IReadOnlyList<string> record, int index)
        {
            return record == null || index < 0 || record.Count <= index;
        }

        /// <summary> 文字列取得 </summary>
        protected string GetString(IReadOnlyList<string> record, int index)
        {
            if (!IsIndexOutOfRange(record, index))
                return record[index];

            _importLogBuilder.OutOfRangeIndex(index);
            return string.Empty;
        }

        /// <summary> int取得 </summary>
        protected int GetInt(IReadOnlyList<string> record, int index)
        {
            if (IsIndexOutOfRange(record, index))
            {
                _importLogBuilder.OutOfRangeIndex(index);
                return default;
            }

            var str = record[index];
            if (int.TryParse(str, out var result))
                return result;

            if (float.TryParse(str, out var floatResult))
            {
                var intResult = (int)floatResult;
                _importLogBuilder.CastOnParse($"floatからintへのキャストが発生 (index={index}, {str} -> {intResult}");
                return intResult;
            }

            _importLogBuilder.FailedParse("int", index, str);
            return default;
        }

        /// <summary> float取得 </summary>
        protected float GetFloat(IReadOnlyList<string> record, int index)
        {
            if (IsIndexOutOfRange(record, index))
            {
                _importLogBuilder.OutOfRangeIndex(index);
                return default;
            }

            var str = record[index];
            if (float.TryParse(str, out var result))
                return result;

            _importLogBuilder.FailedParse("float", index, str);
            return default;
        }

        /// <summary> bool取得 </summary>
        protected bool GetBool(IReadOnlyList<string> record, int index)
        {
            if (IsIndexOutOfRange(record, index))
            {
                _importLogBuilder.OutOfRangeIndex(index);
                return default;
            }

            var str = record[index];
            if (bool.TryParse(str, out var result))
                return result;

            _importLogBuilder.FailedParse("bool", index, str);
            return default(bool);
        }

        /// <summary> enum取得 </summary>
        protected T GetEnum<T>(IReadOnlyList<string> record, int index, T @default = default(T)) where T : struct
        {
            if (IsIndexOutOfRange(record, index))
            {
                _importLogBuilder.OutOfRangeIndex(index);
                return default;
            }

            var str = record[index];
            if (Enum.TryParse(str, out T result))
                return result;

            _importLogBuilder.FailedParse($"enum({typeof(T).Name})", index, str);
            return @default;
        }
    }
}