using System.Collections.Generic;

namespace SoD_DiffExplorer.csutils
{
	interface YamlObject
	{
		public bool Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth);
	}
}
