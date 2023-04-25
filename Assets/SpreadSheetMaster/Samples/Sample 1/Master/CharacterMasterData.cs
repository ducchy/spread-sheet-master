using SpreadSheetMaster;
using System.Collections.Generic;

namespace SpreadSheetMaster.Samples
{
	public partial class CharacterMasterData : ImportableSpreadSheetMasterDataBase
	{
		private const int ColumnId = 1;
		private const int ColumnName = 2;
		private const int ColumnAge = 3;
		private const int ColumnHeight = 4;
		private const int ColumnMax = 5;

		public int Id { get; private set; }
		public string Name { get; private set; }
		public float Age { get; private set; }
		public int Height { get; private set; }

		public override int GetKey()
		{
			return Id;
		}

		protected override void SetDataInternal(IReadOnlyList<string> record)
		{
			Id = GetInt(record, ColumnId);
			Name = GetString(record, ColumnName);
			Age = GetFloat(record, ColumnAge);
			Height = GetInt(record, ColumnHeight);
		}
		public override string ToString()
		{
			return "CharacterMasterData [" +
				"Id=" + Id + ", " +
				"Name=" + Name + ", " +
				"Age=" + Age + ", " +
				"Height=" + Height +
				"]";
		}
	}
}
