using SpreadSheetMaster;
using System.Collections.Generic;

namespace SpreadSheetMaster.Samples
{
	public partial class CharacterMasterData : ImportableSpreadSheetMasterDataBase
	{
		private const int COLUMN_ID = 1;
		private const int COLUMN_NAME = 2;
		private const int COLUMN_AGE = 3;
		private const int COLUMN_GENDER = 4;

		public int id { get; private set; }
		public string name { get; private set; }
		public int age { get; private set; }
		public int gender { get; private set; }

		public override int GetId()
		{
			return id;
		}
		public override void SetData(IReadOnlyList<string> record)
		{
			id = GetInt(record, COLUMN_ID);
			name = GetString(record, COLUMN_NAME);
			age = GetInt(record, COLUMN_AGE);
			gender = GetInt(record, COLUMN_GENDER);
		}
		public override string ToString()
		{
			return "CharacterMasterData [" +
				"id=" + id + ", " +
				"name=" + name + ", " +
				"age=" + age + ", " +
				"gender=" + gender +
				"]";
		}
	}
}
