using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem.Definition;

namespace TaleWorlds.SaveSystem.Load;

internal class FieldLoadData : MemberLoadData
{
	public FieldLoadData(ObjectLoadData objectLoadData, IReader reader)
		: base(objectLoadData, reader)
	{
	}

	public void FillObject()
	{
		FieldDefinition fieldDefinitionWithId;
		if (base.ObjectLoadData.TypeDefinition != null && (fieldDefinitionWithId = base.ObjectLoadData.TypeDefinition.GetFieldDefinitionWithId(base.MemberSaveId)) != null)
		{
			FieldInfo fieldInfo = fieldDefinitionWithId.FieldInfo;
			object target = base.ObjectLoadData.Target;
			object data = GetDataToUse();
			if (data == null || fieldInfo.FieldType.IsInstanceOfType(data) || LoadContext.TryConvertType(data.GetType(), fieldInfo.FieldType, ref data))
			{
				fieldInfo.SetValue(target, data);
			}
		}
	}
}
