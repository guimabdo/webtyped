using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebTyped {
	public interface ITsFile {
		INamedTypeSymbol TypeSymbol { get; }
		string Module { get; }
		string FullName { get; }
		string OutputFilePath { get; }
		string ClassName { get; }
		string FilenameWithoutExtenstion { get; }
	}
}
