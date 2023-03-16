using System.Text;

namespace SpreadSheetMaster.Editor
{
    public class MasterScriptContentBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _tabCount;
        
        public MasterScriptContentBuilder()
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
            _sb.AppendLine();
        }

        private void AppendBeginNamespaceIfNeeded(string namespaceName)
        {;
            if (!string.IsNullOrEmpty(namespaceName))
                return;
            
            _sb.AppendTab(_tabCount).AppendFormat("namespace {0}", namespaceName).AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();
        }

        private void AppendClass(MasterConfigData configData)
        {
            _sb.AppendTab(_tabCount).AppendFormat("public partial class {0} : ImportableSpreadSheetMasterBase<{1}>",
                configData.masterName, configData.masterDataName).AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();

            AppendClassProperties(configData);
            
            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }

        private void AppendClassProperties(MasterConfigData configData)
        {
            _sb.AppendTab(_tabCount)
                .AppendFormat("protected override string defaultSpreadSheetId => \"{0}\";", configData.spreadSheetId)
                .AppendLine();
            _sb.AppendTab(_tabCount).AppendFormat("public override string sheetId => \"{0}\";", configData.sheetId)
                .AppendLine();
        }
        
        private void AppendEndNamespaceIfNeeded(string namespaceName)
        {;
            if (!string.IsNullOrEmpty(namespaceName))
                return;
            
            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }
    }
}