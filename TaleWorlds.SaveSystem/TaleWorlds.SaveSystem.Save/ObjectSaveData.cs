using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem.Definition;

namespace TaleWorlds.SaveSystem.Save;

internal class ObjectSaveData
{
	private Dictionary<PropertyInfo, PropertySaveData> _propertyValues;

	private Dictionary<FieldInfo, FieldSaveData> _fieldValues;

	private List<MemberSaveData> _stringMembers;

	private TypeDefinition _typeDefinition;

	private Dictionary<MemberDefinition, ObjectSaveData> _childStructs;

	public int ObjectId { get; private set; }

	public ISaveContext Context { get; private set; }

	public object Target { get; private set; }

	public Type Type { get; private set; }

	public bool IsClass { get; private set; }

	internal int PropertyCount => _propertyValues.Count;

	internal int FieldCount => _fieldValues.Count;

	public ObjectSaveData(ISaveContext context, int objectId, object target, bool isClass)
	{
		ObjectId = objectId;
		Context = context;
		Target = target;
		IsClass = isClass;
		_stringMembers = new List<MemberSaveData>();
		Type = target.GetType();
		if (IsClass)
		{
			_typeDefinition = context.DefinitionContext.GetClassDefinition(Type);
		}
		else
		{
			_typeDefinition = context.DefinitionContext.GetStructDefinition(Type);
		}
		_childStructs = new Dictionary<MemberDefinition, ObjectSaveData>(3);
		_propertyValues = new Dictionary<PropertyInfo, PropertySaveData>(_typeDefinition.PropertyDefinitions.Count);
		_fieldValues = new Dictionary<FieldInfo, FieldSaveData>(_typeDefinition.FieldDefinitions.Count);
		if (_typeDefinition == null)
		{
			throw new Exception("Could not find type definition of type: " + Type);
		}
	}

	public void CollectMembers()
	{
		for (int i = 0; i < _typeDefinition.MemberDefinitions.Count; i++)
		{
			MemberDefinition memberDefinition = _typeDefinition.MemberDefinitions[i];
			MemberSaveData memberSaveData = null;
			if (memberDefinition is PropertyDefinition)
			{
				PropertyDefinition propertyDefinition = (PropertyDefinition)memberDefinition;
				PropertyInfo propertyInfo = propertyDefinition.PropertyInfo;
				MemberTypeId id = propertyDefinition.Id;
				PropertySaveData propertySaveData = new PropertySaveData(this, propertyDefinition, id);
				_propertyValues.Add(propertyInfo, propertySaveData);
				memberSaveData = propertySaveData;
			}
			else if (memberDefinition is FieldDefinition)
			{
				FieldDefinition fieldDefinition = (FieldDefinition)memberDefinition;
				FieldInfo fieldInfo = fieldDefinition.FieldInfo;
				MemberTypeId id2 = fieldDefinition.Id;
				FieldSaveData fieldSaveData = new FieldSaveData(this, fieldDefinition, id2);
				_fieldValues.Add(fieldInfo, fieldSaveData);
				memberSaveData = fieldSaveData;
			}
			Type memberType = memberDefinition.GetMemberType();
			TypeDefinitionBase typeDefinition = Context.DefinitionContext.GetTypeDefinition(memberType);
			if (typeDefinition is TypeDefinition { IsClassDefinition: false })
			{
				ObjectSaveData objectSaveData = _childStructs[memberDefinition];
				memberSaveData.InitializeAsCustomStruct(objectSaveData.ObjectId);
			}
			else
			{
				memberSaveData.Initialize(typeDefinition);
			}
			if (memberSaveData.MemberType == SavedMemberType.String)
			{
				_stringMembers.Add(memberSaveData);
			}
		}
		foreach (ObjectSaveData value in _childStructs.Values)
		{
			value.CollectMembers();
		}
	}

	public void CollectStringsInto(List<string> collection)
	{
		for (int i = 0; i < _stringMembers.Count; i++)
		{
			string item = (string)_stringMembers[i].Value;
			collection.Add(item);
		}
		foreach (ObjectSaveData value in _childStructs.Values)
		{
			value.CollectStringsInto(collection);
		}
	}

	public void CollectStrings()
	{
		for (int i = 0; i < _stringMembers.Count; i++)
		{
			string text = (string)_stringMembers[i].Value;
			Context.AddOrGetStringId(text);
		}
		foreach (ObjectSaveData value in _childStructs.Values)
		{
			value.CollectStrings();
		}
	}

	public void CollectStructs()
	{
		int num = 0;
		for (int i = 0; i < _typeDefinition.MemberDefinitions.Count; i++)
		{
			MemberDefinition memberDefinition = _typeDefinition.MemberDefinitions[i];
			Type memberType = memberDefinition.GetMemberType();
			if (Context.DefinitionContext.GetStructDefinition(memberType) != null)
			{
				object value = memberDefinition.GetValue(Target);
				ObjectSaveData value2 = new ObjectSaveData(Context, num, value, isClass: false);
				_childStructs.Add(memberDefinition, value2);
				num++;
			}
		}
		foreach (ObjectSaveData value3 in _childStructs.Values)
		{
			value3.CollectStructs();
		}
	}

	public void SaveHeaderTo(SaveEntryFolder parentFolder, IArchiveContext archiveContext)
	{
		SaveFolderExtension extension = (IsClass ? SaveFolderExtension.Object : SaveFolderExtension.Struct);
		SaveEntryFolder saveEntryFolder = archiveContext.CreateFolder(parentFolder, new FolderId(ObjectId, extension), 1);
		BinaryWriter binaryWriter = BinaryWriterFactory.GetBinaryWriter();
		_typeDefinition.SaveId.WriteTo(binaryWriter);
		binaryWriter.WriteShort((short)_propertyValues.Count);
		binaryWriter.WriteShort((short)_childStructs.Count);
		saveEntryFolder.CreateEntry(new EntryId(-1, SaveEntryExtension.Basics)).FillFrom(binaryWriter);
		BinaryWriterFactory.ReleaseBinaryWriter(binaryWriter);
	}

	public void SaveTo(SaveEntryFolder parentFolder, IArchiveContext archiveContext)
	{
		SaveFolderExtension extension = (IsClass ? SaveFolderExtension.Object : SaveFolderExtension.Struct);
		int entryCount = 1 + _fieldValues.Values.Count + _propertyValues.Values.Count;
		SaveEntryFolder saveEntryFolder = archiveContext.CreateFolder(parentFolder, new FolderId(ObjectId, extension), entryCount);
		BinaryWriter binaryWriter = BinaryWriterFactory.GetBinaryWriter();
		_typeDefinition.SaveId.WriteTo(binaryWriter);
		binaryWriter.WriteShort((short)_propertyValues.Count);
		binaryWriter.WriteShort((short)_childStructs.Count);
		saveEntryFolder.CreateEntry(new EntryId(-1, SaveEntryExtension.Basics)).FillFrom(binaryWriter);
		BinaryWriterFactory.ReleaseBinaryWriter(binaryWriter);
		int num = 0;
		foreach (FieldSaveData value in _fieldValues.Values)
		{
			BinaryWriter binaryWriter2 = BinaryWriterFactory.GetBinaryWriter();
			value.SaveTo(binaryWriter2);
			saveEntryFolder.CreateEntry(new EntryId(num, SaveEntryExtension.Field)).FillFrom(binaryWriter2);
			BinaryWriterFactory.ReleaseBinaryWriter(binaryWriter2);
			num++;
		}
		int num2 = 0;
		foreach (PropertySaveData value2 in _propertyValues.Values)
		{
			BinaryWriter binaryWriter3 = BinaryWriterFactory.GetBinaryWriter();
			value2.SaveTo(binaryWriter3);
			saveEntryFolder.CreateEntry(new EntryId(num2, SaveEntryExtension.Property)).FillFrom(binaryWriter3);
			BinaryWriterFactory.ReleaseBinaryWriter(binaryWriter3);
			num2++;
		}
		foreach (ObjectSaveData value3 in _childStructs.Values)
		{
			value3.SaveTo(saveEntryFolder, archiveContext);
		}
	}

	internal static void GetChildObjectFrom(ISaveContext context, object target, MemberDefinition memberDefinition, List<object> collectedObjects)
	{
		Type memberType = memberDefinition.GetMemberType();
		if (memberType.IsClass || memberType.IsInterface)
		{
			if (memberType != typeof(string))
			{
				object value = memberDefinition.GetValue(target);
				if (value != null)
				{
					collectedObjects.Add(value);
				}
			}
			return;
		}
		TypeDefinition structDefinition = context.DefinitionContext.GetStructDefinition(memberType);
		if (structDefinition != null)
		{
			object value2 = memberDefinition.GetValue(target);
			for (int i = 0; i < structDefinition.MemberDefinitions.Count; i++)
			{
				MemberDefinition memberDefinition2 = structDefinition.MemberDefinitions[i];
				GetChildObjectFrom(context, value2, memberDefinition2, collectedObjects);
			}
		}
	}

	public IEnumerable<object> GetChildObjects()
	{
		List<object> list = new List<object>();
		GetChildObjects(Context, _typeDefinition, Target, list);
		return list;
	}

	public static void GetChildObjects(ISaveContext context, TypeDefinition typeDefinition, object target, List<object> collectedObjects)
	{
		if (typeDefinition.CollectObjectsMethod != null)
		{
			typeDefinition.CollectObjectsMethod(target, collectedObjects);
			return;
		}
		for (int i = 0; i < typeDefinition.MemberDefinitions.Count; i++)
		{
			MemberDefinition memberDefinition = typeDefinition.MemberDefinitions[i];
			GetChildObjectFrom(context, target, memberDefinition, collectedObjects);
		}
	}
}
