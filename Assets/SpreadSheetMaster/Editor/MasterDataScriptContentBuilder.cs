using System.Collections.Generic;
using System.Text;

namespace SpreadSheetMaster.Editor
{
    public class MasterDataScriptContentBuilder
    {
        private readonly StringBuilder _sb = new();
        private int _tabCount;
        private MasterConfigData _configData;

        public string Build(MasterConfigData configData)
        {
            _configData = configData;
            
            _sb.Clear();
            _tabCount = 0;

            AppendUsing();
            AppendBeginNamespaceIfNeeded();
            AppendClass();
            AppendEndNamespaceIfNeeded();

            return _sb.ToString();
        }

        private void AppendUsing()
        {
            var additionalNamespaceNames = new List<string>();
            foreach (var column in _configData.columns)
            {
                var namespaceName = column.enumType?.Namespace;
                if (string.IsNullOrEmpty(namespaceName) || 
                    _configData.exportNamespaceName == namespaceName ||
                    additionalNamespaceNames.Contains(namespaceName))
                    continue;

                additionalNamespaceNames.Add(namespaceName);
            }
            
            _sb.AppendTab(_tabCount).Append("using SpreadSheetMaster;").AppendLine();
            _sb.AppendTab(_tabCount).Append("using System.Collections.Generic;").AppendLine();
            foreach (var namespaceName in additionalNamespaceNames)
                _sb.AppendTab(_tabCount).AppendFormat("using {0};", namespaceName).AppendLine();
            _sb.AppendLine();
        }

        private void AppendBeginNamespaceIfNeeded()
        {
            var namespaceName = _configData.exportNamespaceName;
            
            if (string.IsNullOrEmpty(namespaceName))
                return;

            _sb.AppendTab(_tabCount).AppendFormat("namespace {0}", namespaceName).AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();
        }

        private void AppendClass()
        {
            _sb.AppendTab(_tabCount).AppendFormat("public partial class {0} : ImportableSpreadSheetMasterDataBase",
                _configData.masterDataName).AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();

            var columnIndexConfigList = CreateColumnIndexConfigList();
            AppendClassConstants(columnIndexConfigList);
            AppendClassProperties(columnIndexConfigList);
            AppendClassMethodGetId();
            AppendClassMethodSetData(columnIndexConfigList);
            AppendClassMethodToString(columnIndexConfigList);

            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }

        private List<System.Tuple<int, MasterColumnConfigData>> CreateColumnIndexConfigList()
        {
            var ret = new List<System.Tuple<int, MasterColumnConfigData>>();
            for (var i = 0; i < _configData.columns.Length; i++)
            {
                var column = _configData.columns[i];
                if (column.exportFlag && column.validFlag)
                    ret.Add(System.Tuple.Create(i, column));
            }

            return ret;
        }

        private void AppendClassConstants(List<System.Tuple<int, MasterColumnConfigData>> columnIndexConfigList)
        {
            var maxColumnConfigData = _configData.maxMasterColumnConfigData;
            
            foreach (var columnIndexConfig in columnIndexConfigList)
                _sb.AppendTab(_tabCount)
                    .AppendFormat("private const int {0} = {1};", columnIndexConfig.Item2.constantName,
                        columnIndexConfig.Item1).AppendLine();

            var maxCount = columnIndexConfigList.Count == 0 ? 0 : columnIndexConfigList[^1].Item1 + 1;
            _sb.AppendTab(_tabCount)
                .AppendFormat("private const int {0} = {1};", maxColumnConfigData.constantName, maxCount).AppendLine();

            _sb.AppendLine();
        }

        private void AppendClassProperties(List<System.Tuple<int, MasterColumnConfigData>> columnIndexConfigList)
        {
            foreach (var columnIndexConfig in columnIndexConfigList)
                _sb.AppendTab(_tabCount).AppendFormat("public {0} {1} ", columnIndexConfig.Item2.typeName,
                        columnIndexConfig.Item2.propertyName)
                    .Append("{ get; private set; }").AppendLine();

            _sb.AppendLine();
        }

        private void AppendClassMethodGetId()
        {
            var idMasterColumnConfigData = _configData.idMasterColumnConfigData;
            _sb.AppendTab(_tabCount).Append("public override int GetKey()").AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();
            _sb.AppendTab(_tabCount).AppendFormat("return {0};", idMasterColumnConfigData.propertyName).AppendLine();
            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
            _sb.AppendLine();
        }

        private void AppendClassMethodSetData(List<System.Tuple<int, MasterColumnConfigData>> columnIndexConfigList)
        {
            _sb.AppendTab(_tabCount).Append("protected override void SetDataInternal(IReadOnlyList<string> record)")
                .AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();

            foreach (var columnIndexConfig in columnIndexConfigList)
                _sb.AppendTab(_tabCount).AppendFormat("{0} = Get{1}{2}(record, {3});",
                    columnIndexConfig.Item2.propertyName,
                    columnIndexConfig.Item2.type.ToString(), 
                    (columnIndexConfig.Item2.enumType != null ? $"<{columnIndexConfig.Item2.enumTypeName}>" : string.Empty),
                    columnIndexConfig.Item2.constantName).AppendLine();

            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }

        private void AppendClassMethodToString(List<System.Tuple<int, MasterColumnConfigData>> columnIndexConfigList)
        {
            _sb.AppendTab(_tabCount).Append("public override string ToString()").AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();

            _sb.AppendTab(_tabCount++).AppendFormat("return \"{0} [\" +", _configData.masterDataName).AppendLine();
            for (var i = 0; i < columnIndexConfigList.Count; i++)
            {
                var columnIndexConfig = columnIndexConfigList[i];
                _sb.AppendTab(_tabCount).AppendFormat("\"{0}=\" + {0} +{1}",
                        columnIndexConfig.Item2.propertyName,
                        (i >= columnIndexConfigList.Count - 1 ? string.Empty : (" \", \" +")))
                    .AppendLine();
            }

            _sb.AppendTab(_tabCount--).Append("\"]\";").AppendLine();
            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }

        private void AppendEndNamespaceIfNeeded()
        {
            var namespaceName = _configData.exportNamespaceName;
            
            if (string.IsNullOrEmpty(namespaceName))
                return;

            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }
    }
}