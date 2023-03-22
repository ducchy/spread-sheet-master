using SpreadSheetMaster;

namespace SpreadSheetMaster.Samples
{
	public partial class CharacterDetailMaster : ImportableSpreadSheetMasterBase<CharacterDetailMasterData>
	{
		protected override string defaultSpreadSheetId => "1qUJ3j7djNezcfvet3p7WcrJ2NS0cQlthybEIjGjK6As";
		public override string sheetId => "";
		public override string sheetName => "キャラクター詳細";
	}
}
