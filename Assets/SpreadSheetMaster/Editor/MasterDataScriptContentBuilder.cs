using System.Collections.Generic;
using System.Text;

namespace SpreadSheetMaster.Editor
{
    public class MasterDataScriptContentBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _tabCount;

        public MasterDataScriptContentBuilder()
        {
        }

        public string Build(MasterConfigData configData, string namespaceName)
        {
            _sb.Clear();
            _tabCount = 0;

            AppendUsing();
            AppendBeginNamespaceIfNeeded(namespaceName);
            AppendClass(configData);
            AppendEndNamespaceIfNeeded(namespaceName);

            return _sb.ToString();
        }

        private void AppendUsing()
        {
            _sb.AppendTab(_tabCount).Append("using SpreadSheetMaster;").AppendLine();
            _sb.AppendTab(_tabCount).Append("using System.Collections.Generic;").AppendLine();
            _sb.AppendLine();
        }

        private void AppendBeginNamespaceIfNeeded(string namespaceName)
        {
            ;
            if (!string.IsNullOrEmpty(namespaceName))
                return;

            _sb.AppendTab(_tabCount).AppendFormat("namespace {0}", namespaceName).AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();
        }

        private void AppendClass(MasterConfigData configData)
        {
            _sb.AppendTab(_tabCount).AppendFormat("public partial class {0} : ImportableSpreadSheetMasterDataBase",
                configData.masterDataName).AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();

            var columnIndexConfigList = CreateColumnIndexConfigList(configData);
            AppendClassConstants(columnIndexConfigList);
            AppendClassProperties(columnIndexConfigList);
            AppendClassMethodGetId(configData._idMasterColumnConfigData);
            AppendClassMethodSetData(columnIndexConfigList);
            AppendClassMethodToString(configData, columnIndexConfigList);

            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }

        private List<System.Tuple<int, MasterColumnConfigData>> CreateColumnIndexConfigList(
            MasterConfigData configData)
        {
            var ret = new List<System.Tuple<int, MasterColumnConfigData>>();
            for (var i = 0; i < configData.columns.Length; i++)
            {
                var column = configData.columns[i];
                if (column.exportFlag && column.validFlag)
                    ret.Add(System.Tuple.Create(i, column));
            }

            return ret;
        }

        private void AppendClassConstants(List<System.Tuple<int, MasterColumnConfigData>> columnIndexConfigList)
        {
            foreach (var columnIndexConfig in columnIndexConfigList)
                _sb.AppendTab(_tabCount)
                    .AppendFormat("private const int {0} = {1};", columnIndexConfig.Item2.constantName,
                        columnIndexConfig.Item1).AppendLine();

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

        private void AppendClassMethodGetId(MasterColumnConfigData idMasterColumnConfigData)
        {
            _sb.AppendTab(_tabCount).Append("public override int GetId()").AppendLine();
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
                _sb.AppendTab(_tabCount).AppendFormat("{0} = Get{1}(record, {2});",
                    columnIndexConfig.Item2.propertyName,
                    columnIndexConfig.Item2.type.ToString(), columnIndexConfig.Item2.constantName).AppendLine();

            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }

        private void AppendClassMethodToString(MasterConfigData configData,
            List<System.Tuple<int, MasterColumnConfigData>> columnIndexConfigList)
        {
            _sb.AppendTab(_tabCount).Append("public override string ToString()").AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();

            _sb.AppendTab(_tabCount++).AppendFormat("return \"{0} [\" +", configData.masterDataName).AppendLine();
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

        private void AppendEndNamespaceIfNeeded(string namespaceName)
        {
            ;
            if (!string.IsNullOrEmpty(namespaceName))
                return;

            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }
    }
}