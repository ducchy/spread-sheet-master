using SpreadSheetMaster;

namespace SpreadSheetMaster.Samples
{
	public partial class CharacterDetailMaster : ImportableSpreadSheetMasterBase<CharacterDetailMasterData>
	{
		protected override string defaultSpreadSheetId => "1qUJ3j7djNezcfvet3p7WcrJ2NS0cQlthybEIjGjK6As";
		public override string sheetId => "0";
		public override string sheetName => "キャラクター詳細";
	}
}
