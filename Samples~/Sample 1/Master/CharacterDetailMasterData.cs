using SpreadSheetMaster;
using System.Collections.Generic;

namespace SpreadSheetMaster.Samples
{
	public partial class CharacterDetailMasterData : ImportableSpreadSheetMasterDataBase
	{
		private const int ColumnId = 1;
		private const int ColumnName = 2;
		private const int ColumnAge = 3;
		private const int ColumnGender = 4;
		private const int ColumnMailAddress = 6;
		private const int ColumnMax = 7;

		public int Id { get; private set; }
		public string Name { get; private set; }
		public int Age { get; private set; }
		public int Gender { get; private set; }
		public string MailAddress { get; private set; }

		public override int GetKey()
		{
			return Id;
		}

		protected override void SetDataInternal(IReadOnlyList<string> record)
		{
			Id = GetInt(record, ColumnId);
			Name = GetString(record, ColumnName);
			Age = GetInt(record, ColumnAge);
			Gender = GetInt(record, ColumnGender);
			MailAddress = GetString(record, ColumnMailAddress);
		}
		public override string ToString()
		{
			return "CharacterDetailMasterData [" +
				"Id=" + Id + ", " +
				"Name=" + Name + ", " +
				"Age=" + Age + ", " +
				"Gender=" + Gender + ", " +
				"MailAddress=" + MailAddress +
				"]";
		}
	}
}
