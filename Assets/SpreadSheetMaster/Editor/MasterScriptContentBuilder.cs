using System.Text;

namespace SpreadSheetMaster.Editor
{
    public class MasterScriptContentBuilder
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
            _sb.AppendTab(_tabCount).Append("using SpreadSheetMaster;").AppendLine();
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
            _sb.AppendTab(_tabCount).AppendFormat("public partial class {0} : ImportableSpreadSheetMasterBase<{1}>",
                _configData.masterName, _configData.masterDataName).AppendLine();
            _sb.AppendTab(_tabCount++).Append("{").AppendLine();

            AppendClassProperties();
            
            _sb.AppendTab(--_tabCount).Append("}").AppendLine();
        }

        private void AppendClassProperties()
        {
            _sb.AppendTab(_tabCount).AppendFormat("public override string sheetName => \"{0}\";", _configData.sheetName)
                .AppendLine();
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