using SpreadSheetMaster;

namespace SpreadSheetMaster.Samples
{
	public partial class CharacterDetailMaster : ImportableSpreadSheetMasterBase<CharacterDetailMasterData>
	{
		public override string sheetName => "キャラクター詳細";
	}
}
