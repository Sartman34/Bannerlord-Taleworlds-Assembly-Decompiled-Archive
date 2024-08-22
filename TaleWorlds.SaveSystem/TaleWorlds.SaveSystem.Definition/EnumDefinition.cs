using System;
using TaleWorlds.SaveSystem.Resolvers;

namespace TaleWorlds.SaveSystem.Definition;

internal class EnumDefinition : TypeDefinitionBase
{
	public readonly IEnumResolver Resolver;

	public EnumDefinition(Type type, SaveId saveId, IEnumResolver resolver)
		: base(type, saveId)
	{
		Resolver = resolver;
	}

	public EnumDefinition(Type type, int saveId, IEnumResolver resolver)
		: this(type, new TypeSaveId(saveId), resolver)
	{
	}
}
