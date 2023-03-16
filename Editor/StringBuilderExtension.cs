using System.Text;

namespace SpreadSheetMaster.Editor
{
    public static class StringBuilderExtension
    {
        public static StringBuilder AppendTab(this StringBuilder @this, int tabCount)
        {
            return @this.Append('\t', tabCount);
        }
    }
}