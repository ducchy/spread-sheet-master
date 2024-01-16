using System.Collections.Generic;
using SpreadSheetMaster.Samples.MyEnums;

namespace SpreadSheetMaster.Samples {
	public class CharacterDetailMasterData : ImportableSpreadSheetMasterDataBase {
		#region Constants

		private const int ColumnId = 1;
		private const int ColumnName = 2;
		private const int ColumnAge = 3;
		private const int ColumnGender = 4;
		private const int ColumnMailAddress = 6;
		private const int ColumnMax = 7;

		#endregion

		#region Variables

		public int Id { get; private set; }
		public string Name { get; private set; }
		public string Age { get; private set; }
		public GenderType Gender { get; private set; }
		public string MailAddress { get; private set; }

		#endregion

		#region Methods

		public override int GetKey() {
			return Id;
		}

		public override string ToString() {
			return "CharacterDetailMasterData [" +
			       "Id=" + Id + ", " +
			       "Name=" + Name + ", " +
			       "Age=" + Age + ", " +
			       "Gender=" + Gender + ", " +
			       "MailAddress=" + MailAddress +
			       "]";
		}

		protected override void SetDataInternal(IReadOnlyList<string> record) {
			Id = GetInt(record, ColumnId);
			Name = GetString(record, ColumnName);
			Age = GetString(record, ColumnAge);
			Gender = GetEnum<GenderType>(record, ColumnGender);
			MailAddress = GetString(record, ColumnMailAddress);
		}

		#endregion
	}
}