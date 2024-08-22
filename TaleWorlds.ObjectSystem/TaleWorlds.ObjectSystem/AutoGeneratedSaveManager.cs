using TaleWorlds.SaveSystem.Definition;

namespace TaleWorlds.ObjectSystem;

internal class AutoGeneratedSaveManager : IAutoGeneratedSaveManager
{
	public void Initialize(DefinitionContext definitionContext)
	{
		TypeDefinition obj = (TypeDefinition)definitionContext.TryGetTypeDefinition(new TypeSaveId(10034));
		CollectObjectsDelegate collectObjectsDelegate = MBObjectBase.AutoGeneratedStaticCollectObjectsMBObjectBase;
		obj.InitializeForAutoGeneration(collectObjectsDelegate);
		obj.GetPropertyDefinitionWithId(new MemberTypeId(2, 1)).InitializeForAutoGeneration(MBObjectBase.AutoGeneratedGetMemberValueStringId);
		obj.GetPropertyDefinitionWithId(new MemberTypeId(2, 2)).InitializeForAutoGeneration(MBObjectBase.AutoGeneratedGetMemberValueId);
		obj.GetPropertyDefinitionWithId(new MemberTypeId(2, 3)).InitializeForAutoGeneration(MBObjectBase.AutoGeneratedGetMemberValueIsRegistered);
	}
}
