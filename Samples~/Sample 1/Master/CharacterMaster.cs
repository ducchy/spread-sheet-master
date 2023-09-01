using SpreadSheetMaster;

namespace SpreadSheetMaster.Samples
{
	public partial class CharacterMaster : ImportableSpreadSheetMasterBase<CharacterMasterData>
	{
		public override string sheetName => "キャラクター";
	}
}
