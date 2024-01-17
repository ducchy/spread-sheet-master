namespace SpreadSheetMaster.Editor {
	public partial class SpreadSheetMasterGeneratorWindow {
		#region Variables

		private string _warningDownloadRootInfo;

		#endregion

		#region Methods

		private void DrawDownloadRootInfo() {
			DrawSectionHeader("ルート情報読み込み");
			DrawSectionContent(() => {
				// バリデーション
				if (!ValidationSectionDownloadRootInfo(out var message)) {
					DrawWarning(message);
				}
			});
		}

		/// <summary> バリデーション </summary>
		private bool ValidationSectionDownloadRootInfo(out string message) {
			if (_settings == null) {
				message = "設定データを選択してください";
				return false;
			}

			if (string.IsNullOrEmpty(_settings.RootInfoSpreadSheetId)) {
				message = "ルート情報のシートIDが未設定";
				return false;
			}

			message = string.Empty;
			return true;
		}

		#endregion
	}
}