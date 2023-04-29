namespace SpreadSheetMaster
{
    using System;
    using System.Collections.Generic;

    public abstract class ImportableSpreadSheetMasterDataBase : IImportableSpreadSheetMasterData
    {
        public abstract int GetKey();

        private ImportMasterLogBuilder _importLogBuilder;

        public void SetData(IReadOnlyList<string> record, ImportMasterLogBuilder importLogBuilder)
        {
            _importLogBuilder = importLogBuilder;

            SetDataInternal(record);
            SetCustomData(record);

            _importLogBuilder = null;
        }

        protected abstract void SetDataInternal(IReadOnlyList<string> record);

        protected virtual void SetCustomData(IReadOnlyList<string> record)
        {
        }

        private bool IsIndexOutOfRange(IReadOnlyList<string> record, int index)
        {
            return record == null || index < 0 || record.Count <= index;
        }

        protected string GetString(IReadOnlyList<string> record, int index)
        {
            if (!IsIndexOutOfRange(record, index))
                return record[index];

            _importLogBuilder.OutOfRangeIndex(index);
            return string.Empty;
        }

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
            return default(float);
        }

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