using System;
using PrefabIdentificationLayers.Prototypes;
namespace PrefabIdentificationLayers.Models
{
	public interface PtypeBuilder
	{
		Ptype.Mutable BuildPrototype(IBuildPrototypeArgs args);
	}
}

