namespace SpreadSheetMaster
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class ImportableSpreadSheetMasterDataBase : IImportableSpreadSheetMasterData
    {
        public abstract int GetKey();

        public void SetData(IReadOnlyList<string> record)
        {
            SetDataInternal(record);
            SetCustomData(record);
        }

        protected abstract void SetDataInternal(IReadOnlyList<string> record);

        protected virtual void SetCustomData(IReadOnlyList<string> record)
        {
        }

        protected string GetString(IReadOnlyList<string> record, int column)
        {
            if (record != null && 0 <= column && column < record.Count)
                return record[column];

            Debug.LogErrorFormat("Index out of range. index={0}", column);
            return string.Empty;
        }

        protected int GetInt(IReadOnlyList<string> record, int column)
        {
            var str = GetString(record, column);
            if (int.TryParse(str, out int result))
                return result;

            Debug.LogErrorFormat("Could not parse \"{0}\" to int.", str);
            return default(int);
        }

        protected float GetFloat(IReadOnlyList<string> record, int column)
        {
            var str = GetString(record, column);
            if (float.TryParse(str, out float result))
                return result;

            Debug.LogErrorFormat("Could not parse \"{0}\" to float.", str);
            return default(float);
        }

        protected bool GetBool(IReadOnlyList<string> record, int column)
        {
            var str = GetString(record, column);
            if (bool.TryParse(str, out bool result))
                return result;

            Debug.LogErrorFormat("Could not parse \"{0}\" to bool.", str);
            return default(bool);
        }

        protected T GetEnum<T>(IReadOnlyList<string> record, int column, T @default = default(T)) where T : struct
        {
            var str = GetString(record, column);
            if (Enum.TryParse(str, out T result))
                return result;

            Debug.LogErrorFormat("Could not parse {0} to {1}.", str, typeof(T).Name);
            return @default;
        }
    }
}