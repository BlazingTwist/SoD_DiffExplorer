using SoD_DiffExplorer.csutils;

namespace SoD_DiffExplorer.commonconfig
{
	class ResultFilter : YamlObject
	{
		public string path = null;
		public bool isAllowed = false;
		public string outputName = null;

		public void ToggleIsAllowed() {
			isAllowed = !isAllowed;
		}

		public string[] GetFieldNames() {
			return new string[]{"path", "isAllowed", "outputName"};
		}

		public string[] GetFieldValues() {
			return new string[]{path, isAllowed.ToString(), outputName};
		}
	}
}
