using System;
using System.Collections.Generic;

namespace Prefab
{
	public interface Layer
	{

		string Name{ get; }
		void Init(Dictionary<string, object> parameters);
		void Close();
		void Interpret(InterpretArgs args);
		void AfterInterpret(Tree tree);
		void ProcessAnnotations(AnnotationArgs args);
		IEnumerable<string> AnnotationLibraries();
	}
}

