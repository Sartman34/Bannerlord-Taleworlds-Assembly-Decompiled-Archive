using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Xsl;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace TaleWorlds.ObjectSystem;

public sealed class MBObjectManager
{
	internal interface IObjectTypeRecord : IEnumerable
	{
		bool AutoCreate { get; }

		string ElementName { get; }

		string ElementListName { get; }

		Type ObjectClass { get; }

		uint TypeNo { get; }

		bool IsTemporary { get; }

		void ReInitialize();

		MBObjectBase CreatePresumedMBObject(string objectName);

		void RegisterMBObject(MBObjectBase obj, bool presumed, out MBObjectBase registeredObject);

		void RegisterMBObjectWithoutInitialization(MBObjectBase obj);

		void UnregisterMBObject(MBObjectBase obj);

		MBObjectBase GetFirstMBObject();

		MBObjectBase GetMBObject(string objId);

		MBObjectBase GetMBObject(MBGUID objId);

		bool ContainsObject(string objId);

		string DebugDump();

		string DebugBasicDump();

		IEnumerable GetList();

		void PreAfterLoad();

		void AfterLoad();
	}

	internal class ObjectTypeRecord<T> : IObjectTypeRecord, IEnumerable, IEnumerable<T> where T : MBObjectBase
	{
		private readonly bool _autoCreate;

		private readonly string _elementName;

		private readonly string _elementListName;

		private uint _objCount;

		private readonly uint _typeNo;

		private readonly bool _isTemporary;

		private readonly Dictionary<string, T> _registeredObjects;

		private readonly Dictionary<MBGUID, T> _registeredObjectsWithGuid;

		bool IObjectTypeRecord.AutoCreate => _autoCreate;

		string IObjectTypeRecord.ElementName => _elementName;

		string IObjectTypeRecord.ElementListName => _elementListName;

		Type IObjectTypeRecord.ObjectClass => typeof(T);

		uint IObjectTypeRecord.TypeNo => _typeNo;

		bool IObjectTypeRecord.IsTemporary => _isTemporary;

		internal MBList<T> RegisteredObjectsList { get; }

		internal ObjectTypeRecord(uint newTypeNo, string classPrefix, string classListPrefix, bool autoCreate, bool isTemporary)
		{
			_typeNo = newTypeNo;
			_elementName = classPrefix;
			_elementListName = classListPrefix;
			_autoCreate = autoCreate;
			_isTemporary = isTemporary;
			_registeredObjects = new Dictionary<string, T>();
			_registeredObjectsWithGuid = new Dictionary<MBGUID, T>();
			RegisteredObjectsList = new MBList<T>();
			_objCount = 0u;
		}

		void IObjectTypeRecord.ReInitialize()
		{
			uint num = 0u;
			foreach (T registeredObjects in RegisteredObjectsList)
			{
				uint subId = registeredObjects.Id.SubId;
				if (subId > num)
				{
					num = subId;
				}
			}
			_objCount = num + 1;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return EnumerateElements();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return EnumerateElements();
		}

		internal MBGUID GetNewId()
		{
			return new MBGUID(_typeNo, ++_objCount);
		}

		MBObjectBase IObjectTypeRecord.CreatePresumedMBObject(string objectName)
		{
			return CreatePresumedObject(objectName);
		}

		private T CreatePresumedObject(string objectName)
		{
			return new T
			{
				StringId = objectName,
				IsReady = false,
				IsInitialized = false
			};
		}

		MBObjectBase IObjectTypeRecord.GetMBObject(string objId)
		{
			return GetObject(objId);
		}

		MBObjectBase IObjectTypeRecord.GetFirstMBObject()
		{
			return GetFirstObject();
		}

		internal T GetFirstObject()
		{
			if (RegisteredObjectsList.Count <= 0)
			{
				return null;
			}
			return RegisteredObjectsList[0];
		}

		internal T GetObject(string objId)
		{
			_registeredObjects.TryGetValue(objId, out var value);
			return value;
		}

		bool IObjectTypeRecord.ContainsObject(string objId)
		{
			return _registeredObjects.ContainsKey(objId);
		}

		public MBObjectBase GetMBObject(MBGUID objId)
		{
			T value = null;
			_registeredObjectsWithGuid.TryGetValue(objId, out value);
			return value;
		}

		void IObjectTypeRecord.RegisterMBObjectWithoutInitialization(MBObjectBase mbObject)
		{
			T val = (T)mbObject;
			if (!string.IsNullOrEmpty(val.StringId) && val.Id.InternalValue != 0 && !_registeredObjects.ContainsKey(val.StringId))
			{
				_registeredObjects.Add(val.StringId, val);
				_registeredObjectsWithGuid.Add(val.Id, val);
				RegisteredObjectsList.Add(val);
			}
		}

		void IObjectTypeRecord.RegisterMBObject(MBObjectBase obj, bool presumed, out MBObjectBase registeredObject)
		{
			if (obj is T)
			{
				RegisterObject(obj as T, presumed, out registeredObject);
			}
			else
			{
				registeredObject = null;
			}
		}

		internal void RegisterObject(T obj, bool presumed, out MBObjectBase registeredObject)
		{
			if (_registeredObjects.TryGetValue(obj.StringId, out var value))
			{
				if (value == obj || presumed)
				{
					registeredObject = value;
					return;
				}
				string text;
				long num;
				(text, num) = GetIdParts(obj.StringId);
				if (_registeredObjects.ContainsKey(obj.StringId))
				{
					num = _objCount;
					obj.StringId = text + num;
					while (_registeredObjects.ContainsKey(obj.StringId))
					{
						num++;
						obj.StringId = text + num;
					}
				}
			}
			_registeredObjects.Add(obj.StringId, obj);
			obj.Id = GetNewId();
			_registeredObjectsWithGuid.Add(obj.Id, obj);
			RegisteredObjectsList.Add(obj);
			obj.IsReady = !presumed;
			obj.OnRegistered();
			registeredObject = obj;
		}

		private (string str, long number) GetIdParts(string stringId)
		{
			int num = stringId.Length - 1;
			while (num > 0 && char.IsDigit(stringId[num]))
			{
				num--;
			}
			string item = stringId.Substring(0, num + 1);
			long result = 0L;
			if (num < stringId.Length - 1)
			{
				long.TryParse(stringId.Substring(num + 1, stringId.Length - num - 1), out result);
			}
			return (str: item, number: result);
		}

		void IObjectTypeRecord.UnregisterMBObject(MBObjectBase obj)
		{
			if (obj is T)
			{
				UnregisterObject((T)obj);
				return;
			}
			throw new MBIllegalRegisterException();
		}

		private void UnregisterObject(T obj)
		{
			obj.OnUnregistered();
			if (_registeredObjects.ContainsKey(obj.StringId) && _registeredObjects[obj.StringId] == obj)
			{
				_registeredObjects.Remove(obj.StringId);
			}
			if (_registeredObjectsWithGuid.ContainsKey(obj.Id) && _registeredObjectsWithGuid[obj.Id] == obj)
			{
				_registeredObjectsWithGuid.Remove(obj.Id);
			}
			RegisteredObjectsList.Remove(obj);
		}

		internal MBReadOnlyList<T> GetObjectsList()
		{
			return RegisteredObjectsList;
		}

		IEnumerable IObjectTypeRecord.GetList()
		{
			return RegisteredObjectsList;
		}

		string IObjectTypeRecord.DebugDump()
		{
			string text = "";
			text += "**************************************\r\n";
			text = text + _elementName + " " + _objCount + "\r\n";
			text += "**************************************\r\n";
			text += "\r\n";
			foreach (KeyValuePair<MBGUID, T> item in _registeredObjectsWithGuid)
			{
				text = text + item.Key.ToString() + " " + item.Value.ToString() + "\r\n";
			}
			return text;
		}

		string IObjectTypeRecord.DebugBasicDump()
		{
			return _elementName + " " + _objCount;
		}

		private IEnumerator<T> EnumerateElements()
		{
			for (int i = 0; i < RegisteredObjectsList.Count; i++)
			{
				yield return RegisteredObjectsList[i];
			}
		}

		void IObjectTypeRecord.PreAfterLoad()
		{
			for (int num = RegisteredObjectsList.Count - 1; num >= 0; num--)
			{
				RegisteredObjectsList[num].PreAfterLoadInternal();
			}
		}

		void IObjectTypeRecord.AfterLoad()
		{
			for (int num = RegisteredObjectsList.Count - 1; num >= 0; num--)
			{
				RegisteredObjectsList[num].AfterLoadInternal();
			}
		}
	}

	internal List<IObjectTypeRecord> ObjectTypeRecords = new List<IObjectTypeRecord>();

	private List<IObjectManagerHandler> _handlers;

	public static MBObjectManager Instance { get; private set; }

	public int NumRegisteredTypes
	{
		get
		{
			if (ObjectTypeRecords == null)
			{
				return 0;
			}
			return ObjectTypeRecords.Count;
		}
	}

	public int MaxRegisteredTypes => 256;

	private MBObjectManager()
	{
	}

	public static MBObjectManager Init()
	{
		_ = Instance;
		Instance = new MBObjectManager();
		return Instance;
	}

	public void Destroy()
	{
		ClearAllObjects();
		Instance = null;
	}

	public void RegisterType<T>(string classPrefix, string classListPrefix, uint typeId, bool autoCreateInstance = true, bool isTemporary = false) where T : MBObjectBase
	{
		if (NumRegisteredTypes > MaxRegisteredTypes)
		{
			Debug.FailedAssert(new MBTooManyRegisteredTypesException().ToString(), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "RegisterType", 66);
		}
		ObjectTypeRecords.Add(new ObjectTypeRecord<T>(typeId, classPrefix, classListPrefix, autoCreateInstance, isTemporary));
	}

	public bool HasType(Type type)
	{
		if (type.IsSealed)
		{
			foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
			{
				if (objectTypeRecord.ObjectClass == type)
				{
					return true;
				}
			}
		}
		else
		{
			foreach (IObjectTypeRecord objectTypeRecord2 in ObjectTypeRecords)
			{
				if (type.IsAssignableFrom(objectTypeRecord2.ObjectClass))
				{
					return true;
				}
			}
		}
		return false;
	}

	public string FindRegisteredClassPrefix(Type type)
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.ObjectClass == type)
			{
				return objectTypeRecord.ElementName;
			}
		}
		Debug.FailedAssert(type.Name + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "FindRegisteredClassPrefix", 108);
		return null;
	}

	public Type FindRegisteredType(string classPrefix)
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.ElementName == classPrefix)
			{
				return objectTypeRecord.ObjectClass;
			}
		}
		Debug.FailedAssert(classPrefix + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "FindRegisteredType", 122);
		return null;
	}

	public T RegisterObject<T>(T obj) where T : MBObjectBase
	{
		RegisterObjectInternalWithoutTypeId(obj, presumed: false, out var registeredObject);
		return registeredObject as T;
	}

	public T RegisterPresumedObject<T>(T obj) where T : MBObjectBase
	{
		RegisterObjectInternalWithoutTypeId(obj, presumed: true, out var registeredObject);
		return registeredObject as T;
	}

	internal void TryRegisterObjectWithoutInitialization(MBObjectBase obj)
	{
		Type type = obj.GetType();
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.ObjectClass == type)
			{
				objectTypeRecord.RegisterMBObjectWithoutInitialization(obj);
				return;
			}
		}
		Debug.FailedAssert(obj.GetType().Name + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "TryRegisterObjectWithoutInitialization", 153);
	}

	private void RegisterObjectInternalWithoutTypeId<T>(T obj, bool presumed, out MBObjectBase registeredObject) where T : MBObjectBase
	{
		Type type = obj.GetType();
		type = typeof(T);
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.ObjectClass == type)
			{
				objectTypeRecord.RegisterMBObject(obj, presumed, out registeredObject);
				return;
			}
		}
		registeredObject = null;
		Debug.FailedAssert(typeof(T).Name + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "RegisterObjectInternalWithoutTypeId", 170);
	}

	public void UnregisterObject(MBObjectBase obj)
	{
		if (obj == null)
		{
			return;
		}
		Type type = obj.GetType();
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (type == objectTypeRecord.ObjectClass)
			{
				objectTypeRecord.UnregisterMBObject(obj);
				AfterUnregisterObject(obj);
				return;
			}
		}
		Debug.FailedAssert("UnregisterObject call for an unregistered object! Type: " + obj.GetType(), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "UnregisterObject", 192);
	}

	private void AfterUnregisterObject(MBObjectBase obj)
	{
		if (_handlers == null)
		{
			return;
		}
		foreach (IObjectManagerHandler handler in _handlers)
		{
			handler.AfterUnregisterObject(obj);
		}
	}

	public T GetObject<T>(Func<T, bool> predicate) where T : MBObjectBase
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsSealed)
		{
			foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
			{
				if (objectTypeRecord.ObjectClass == typeFromHandle)
				{
					return ((ObjectTypeRecord<T>)objectTypeRecord).FirstOrDefault(predicate);
				}
			}
		}
		else
		{
			foreach (IObjectTypeRecord objectTypeRecord2 in ObjectTypeRecords)
			{
				if (typeFromHandle.IsAssignableFrom(objectTypeRecord2.ObjectClass))
				{
					return objectTypeRecord2.OfType<T>().FirstOrDefault(predicate);
				}
			}
		}
		Debug.FailedAssert(typeof(T).Name + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "GetObject", 232);
		return null;
	}

	public T GetObject<T>(string objectName) where T : MBObjectBase
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsSealed)
		{
			foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
			{
				if (objectTypeRecord.ObjectClass == typeFromHandle)
				{
					return ((ObjectTypeRecord<T>)objectTypeRecord).GetObject(objectName);
				}
			}
		}
		else
		{
			foreach (IObjectTypeRecord objectTypeRecord2 in ObjectTypeRecords)
			{
				if (typeFromHandle.IsAssignableFrom(objectTypeRecord2.ObjectClass) && objectTypeRecord2.GetMBObject(objectName) is T result)
				{
					return result;
				}
			}
		}
		return null;
	}

	public T GetFirstObject<T>() where T : MBObjectBase
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsSealed)
		{
			foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
			{
				if (objectTypeRecord.ObjectClass == typeFromHandle)
				{
					return ((ObjectTypeRecord<T>)objectTypeRecord).GetFirstObject();
				}
			}
		}
		else
		{
			foreach (IObjectTypeRecord objectTypeRecord2 in ObjectTypeRecords)
			{
				if (typeFromHandle.IsAssignableFrom(objectTypeRecord2.ObjectClass) && objectTypeRecord2.GetFirstMBObject() is T result)
				{
					return result;
				}
			}
		}
		return null;
	}

	public bool ContainsObject<T>(string objectName) where T : MBObjectBase
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsSealed)
		{
			foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
			{
				if (objectTypeRecord.ObjectClass == typeFromHandle)
				{
					return objectTypeRecord.ContainsObject(objectName);
				}
			}
		}
		else
		{
			foreach (IObjectTypeRecord objectTypeRecord2 in ObjectTypeRecords)
			{
				if (typeFromHandle.IsAssignableFrom(objectTypeRecord2.ObjectClass))
				{
					bool flag = objectTypeRecord2.ContainsObject(objectName);
					if (flag)
					{
						return flag;
					}
				}
			}
		}
		return false;
	}

	public void RemoveTemporaryTypes()
	{
		for (int num = ObjectTypeRecords.Count - 1; num >= 0; num--)
		{
			IObjectTypeRecord objectTypeRecord = ObjectTypeRecords[num];
			if (objectTypeRecord.IsTemporary)
			{
				foreach (MBObjectBase item in objectTypeRecord)
				{
					UnregisterObject(item);
				}
				ObjectTypeRecords.Remove(objectTypeRecord);
			}
		}
	}

	public void PreAfterLoad()
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			objectTypeRecord.PreAfterLoad();
		}
	}

	public void AfterLoad()
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			objectTypeRecord.AfterLoad();
		}
	}

	public MBObjectBase GetObject(MBGUID objectId)
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.TypeNo == objectId.GetTypeIndex())
			{
				return objectTypeRecord.GetMBObject(objectId);
			}
		}
		Debug.FailedAssert(objectId.GetTypeIndex() + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "GetObject", 391);
		return null;
	}

	public MBObjectBase GetObject(string typeName, string objectName)
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.ElementName == typeName)
			{
				return objectTypeRecord.GetMBObject(objectName);
			}
		}
		Debug.FailedAssert(typeName + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "GetObject", 406);
		return null;
	}

	private MBObjectBase GetPresumedObject(string typeName, string objectName, bool isInitialize = false)
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.ElementName == typeName)
			{
				MBObjectBase mBObject = objectTypeRecord.GetMBObject(objectName);
				if (mBObject != null)
				{
					return mBObject;
				}
				if (objectTypeRecord.AutoCreate)
				{
					mBObject = objectTypeRecord.CreatePresumedMBObject(objectName);
					objectTypeRecord.RegisterMBObject(mBObject, presumed: true, out var registeredObject);
					return registeredObject;
				}
				throw new MBCanNotCreatePresumedObjectException();
			}
		}
		Debug.FailedAssert(typeName + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "GetPresumedObject", 434);
		return null;
	}

	public MBReadOnlyList<T> GetObjectTypeList<T>() where T : MBObjectBase
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsSealed)
		{
			foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
			{
				if (objectTypeRecord.ObjectClass == typeFromHandle)
				{
					return ((ObjectTypeRecord<T>)objectTypeRecord).GetObjectsList();
				}
			}
			Debug.FailedAssert(typeof(T).Name + " could not be found in MBObjectManager objectTypeRecords!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.ObjectSystem\\MBObjectManager.cs", "GetObjectTypeList", 471);
			return null;
		}
		MBList<T> mBList = new MBList<T>();
		foreach (IObjectTypeRecord objectTypeRecord2 in ObjectTypeRecords)
		{
			if (!typeFromHandle.IsAssignableFrom(objectTypeRecord2.ObjectClass))
			{
				continue;
			}
			foreach (object item in objectTypeRecord2.GetList())
			{
				mBList.Add((T)item);
			}
		}
		return mBList;
	}

	public IList<MBObjectBase> CreateObjectTypeList(Type objectClassType)
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (!(objectTypeRecord.ObjectClass == objectClassType))
			{
				continue;
			}
			List<MBObjectBase> list = new List<MBObjectBase>();
			foreach (object item2 in objectTypeRecord)
			{
				MBObjectBase item = item2 as MBObjectBase;
				list.Add(item);
			}
			return list;
		}
		return null;
	}

	public void LoadXML(string id, bool isDevelopment, string gameType, bool skipXmlFilterForEditor = false)
	{
		bool ignoreGameTypeInclusionCheck = skipXmlFilterForEditor || isDevelopment;
		XmlDocument mergedXmlForManaged = GetMergedXmlForManaged(id, skipValidation: false, ignoreGameTypeInclusionCheck, gameType);
		try
		{
			LoadXml(mergedXmlForManaged, isDevelopment);
		}
		catch (Exception)
		{
		}
	}

	public static XmlDocument GetMergedXmlForManaged(string id, bool skipValidation, bool ignoreGameTypeInclusionCheck = true, string gameType = "")
	{
		List<Tuple<string, string>> list = new List<Tuple<string, string>>();
		List<string> xsltList = new List<string>();
		foreach (MbObjectXmlInformation xmlInformation in XmlResource.XmlInformationList)
		{
			if (!(xmlInformation.Id == id) || (!ignoreGameTypeInclusionCheck && xmlInformation.GameTypesIncluded.Count != 0 && !xmlInformation.GameTypesIncluded.Contains(gameType)))
			{
				continue;
			}
			string xsdPath = ModuleHelper.GetXsdPath(xmlInformation.Id);
			string xmlPath = ModuleHelper.GetXmlPath(xmlInformation.ModuleName, xmlInformation.Name);
			if (File.Exists(xmlPath))
			{
				list.Add(Tuple.Create(ModuleHelper.GetXmlPath(xmlInformation.ModuleName, xmlInformation.Name), xsdPath));
				HandleXsltList(ModuleHelper.GetXsltPath(xmlInformation.ModuleName, xmlInformation.Name), ref xsltList);
				continue;
			}
			string text = xmlPath.Replace(".xml", "");
			if (Directory.Exists(text))
			{
				FileInfo[] files = new DirectoryInfo(text).GetFiles("*.xml");
				foreach (FileInfo fileInfo in files)
				{
					xmlPath = text + "/" + fileInfo.Name;
					list.Add(Tuple.Create(xmlPath, xsdPath));
					HandleXsltList(xmlPath.Replace(".xml", ".xsl"), ref xsltList);
				}
			}
			else
			{
				list.Add(Tuple.Create("", ""));
				HandleXsltList(ModuleHelper.GetXsltPath(xmlInformation.ModuleName, xmlInformation.Name), ref xsltList);
			}
		}
		return CreateMergedXmlFile(list, xsltList, skipValidation);
	}

	public static XmlDocument GetMergedXmlForNative(string id, out List<string> usedPaths)
	{
		usedPaths = new List<string>();
		List<Tuple<string, string>> list = new List<Tuple<string, string>>();
		List<string> xsltList = new List<string>();
		foreach (MbObjectXmlInformation mbprojXml in XmlResource.MbprojXmls)
		{
			if (mbprojXml.Id == id)
			{
				if (File.Exists(ModuleHelper.GetXmlPathForNative(mbprojXml.ModuleName, mbprojXml.Name)))
				{
					usedPaths.Add(ModuleHelper.GetXmlPathForNativeWBase(mbprojXml.ModuleName, mbprojXml.Name));
					list.Add(Tuple.Create(ModuleHelper.GetXmlPathForNative(mbprojXml.ModuleName, mbprojXml.Name), string.Empty));
				}
				else
				{
					list.Add(Tuple.Create("", ""));
				}
				HandleXsltList(ModuleHelper.GetXsltPathForNative(mbprojXml.ModuleName, mbprojXml.Name), ref xsltList);
			}
		}
		return CreateMergedXmlFile(list, xsltList, skipValidation: true);
	}

	private static bool HandleXsltList(string xslPath, ref List<string> xsltList)
	{
		string text = xslPath + "t";
		if (File.Exists(xslPath))
		{
			xsltList.Add(xslPath);
			return true;
		}
		if (File.Exists(text))
		{
			xsltList.Add(text);
			return true;
		}
		xsltList.Add("");
		return false;
	}

	public static XmlDocument CreateMergedXmlFile(List<Tuple<string, string>> toBeMerged, List<string> xsltList, bool skipValidation)
	{
		XmlDocument xmlDocument = CreateDocumentFromXmlFile(toBeMerged[0].Item1, toBeMerged[0].Item2, skipValidation);
		for (int i = 1; i < toBeMerged.Count; i++)
		{
			if (xsltList[i] != "")
			{
				xmlDocument = ApplyXslt(xsltList[i], xmlDocument);
			}
			if (toBeMerged[i].Item1 != "")
			{
				XmlDocument xmlDocument2 = CreateDocumentFromXmlFile(toBeMerged[i].Item1, toBeMerged[i].Item2, skipValidation);
				xmlDocument = MergeTwoXmls(xmlDocument, xmlDocument2);
			}
		}
		return xmlDocument;
	}

	public static XmlDocument ApplyXslt(string xsltPath, XmlDocument baseDocument)
	{
		XmlReader input = new XmlNodeReader(baseDocument);
		XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
		xslCompiledTransform.Load(xsltPath);
		XmlDocument xmlDocument = new XmlDocument(baseDocument.CreateNavigator().NameTable);
		using XmlWriter xmlWriter = xmlDocument.CreateNavigator().AppendChild();
		xslCompiledTransform.Transform(input, xmlWriter);
		xmlWriter.Close();
		return xmlDocument;
	}

	public static XmlDocument MergeTwoXmls(XmlDocument xmlDocument1, XmlDocument xmlDocument2)
	{
		XDocument xDocument = ToXDocument(xmlDocument1);
		XDocument xDocument2 = ToXDocument(xmlDocument2);
		xDocument.Root.Add(xDocument2.Root.Elements());
		return ToXmlDocument(xDocument);
	}

	public static XDocument ToXDocument(XmlDocument xmlDocument)
	{
		using XmlNodeReader xmlNodeReader = new XmlNodeReader(xmlDocument);
		try
		{
			xmlNodeReader.MoveToContent();
			return XDocument.Load(xmlNodeReader);
		}
		catch (Exception ex)
		{
			Debug.Print(ex.Message);
			throw;
		}
	}

	public static XmlDocument ToXmlDocument(XDocument xDocument)
	{
		XmlDocument xmlDocument = new XmlDocument();
		using (xDocument.CreateReader())
		{
			xmlDocument.Load(xDocument.CreateReader());
			return xmlDocument;
		}
	}

	public void LoadOneXmlFromFile(string xmlPath, string xsdPath, bool skipValidation = false)
	{
		try
		{
			XmlDocument doc = CreateDocumentFromXmlFile(xmlPath, xsdPath, skipValidation);
			LoadXml(doc);
		}
		catch (Exception)
		{
		}
	}

	public XmlDocument LoadXMLFromFileSkipValidation(string xmlPath, string xsdPath)
	{
		try
		{
			return CreateDocumentFromXmlFile(xmlPath, xsdPath, forceSkipValidation: true);
		}
		catch
		{
			return null;
		}
	}

	private static void LoadXmlWithValidation(string xmlPath, string xsdPath, XmlDocument xmlDocument)
	{
		Debug.Print("opening " + xsdPath);
		XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
		XmlTextReader xmlTextReader = null;
		try
		{
			xmlTextReader = new XmlTextReader(xsdPath);
			xmlSchemaSet.Add(null, xmlTextReader);
		}
		catch (FileNotFoundException)
		{
			Debug.Print("xsd file of " + xmlPath + " could not be found!", 0, Debug.DebugColor.Red);
		}
		catch (XmlSchemaException ex2)
		{
			Debug.Print("XmlSchemaException, line number: " + ex2.LineNumber + ", line position: " + ex2.LinePosition + ", SourceSchemaObject: " + ex2.SourceSchemaObject);
			Debug.Print("xsd file of " + xmlPath + " could not be read! " + ex2.Message, 0, Debug.DebugColor.Red);
		}
		catch (ArgumentNullException ex3)
		{
			Debug.Print("ArgumentNullException, ParamName: " + ex3.ParamName);
			Debug.Print("xsd file of " + xmlPath + " could not be read! " + ex3.Message, 0, Debug.DebugColor.Red);
		}
		catch (Exception ex4)
		{
			Debug.Print("xsd file of " + xmlPath + " could not be read! " + ex4.Message, 0, Debug.DebugColor.Red);
		}
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.ValidationType = ValidationType.None;
		xmlReaderSettings.Schemas.Add(xmlSchemaSet);
		xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
		xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
		xmlReaderSettings.CloseInput = true;
		try
		{
			XmlReader xmlReader = XmlReader.Create(xmlPath, xmlReaderSettings);
			xmlDocument.Load(xmlReader);
			xmlReader.Close();
			XmlReaderSettings xmlReaderSettings2 = new XmlReaderSettings();
			xmlReaderSettings2.ValidationType = ValidationType.Schema;
			xmlReaderSettings2.Schemas.Add(xmlSchemaSet);
			xmlReaderSettings2.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
			xmlReaderSettings2.ValidationEventHandler += ValidationEventHandler;
			xmlReaderSettings2.CloseInput = true;
			xmlReader = XmlReader.Create(xmlPath, xmlReaderSettings2);
			xmlDocument.Load(xmlReader);
			xmlReader.Close();
		}
		catch (Exception)
		{
			_ = new Uri(xmlDocument.BaseURI).LocalPath;
		}
		xmlTextReader?.Close();
	}

	private static void ValidationEventHandler(object sender, ValidationEventArgs e)
	{
		XmlReader xmlReader = (XmlReader)sender;
		string text = string.Empty;
		switch (e.Severity)
		{
		case XmlSeverityType.Error:
			text = text + "Error: " + e.Message;
			break;
		case XmlSeverityType.Warning:
			text = text + "Warning: " + e.Message;
			break;
		}
		text = text + "\nNode: " + xmlReader.Name + "  Value: " + xmlReader.Value;
		text = text + "\nLine: " + e.Exception.LineNumber;
		text = text + "\nXML Path: " + xmlReader.BaseURI;
		Debug.Print(text, 0, Debug.DebugColor.Red);
	}

	private static XmlDocument CreateDocumentFromXmlFile(string xmlPath, string xsdPath, bool forceSkipValidation = false)
	{
		Debug.Print("opening " + xmlPath);
		XmlDocument xmlDocument = new XmlDocument();
		StreamReader streamReader = new StreamReader(xmlPath);
		string xml = streamReader.ReadToEnd();
		if (!forceSkipValidation)
		{
			LoadXmlWithValidation(xmlPath, xsdPath, xmlDocument);
		}
		else
		{
			xmlDocument.LoadXml(xml);
		}
		streamReader.Close();
		return xmlDocument;
	}

	public void LoadXml(XmlDocument doc, bool isDevelopment = false)
	{
		int i = 0;
		bool flag = false;
		string typeName = null;
		for (; i < doc.ChildNodes.Count; i++)
		{
			int i2 = i;
			foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
			{
				if (objectTypeRecord.ElementListName == doc.ChildNodes[i2].Name)
				{
					typeName = objectTypeRecord.ElementName;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		for (XmlNode xmlNode = doc.ChildNodes[i].ChildNodes[0]; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.NodeType != XmlNodeType.Comment)
			{
				string value = xmlNode.Attributes["id"].Value;
				MBObjectBase presumedObject = GetPresumedObject(typeName, value, isInitialize: true);
				presumedObject.Deserialize(this, xmlNode);
				presumedObject.AfterInitialized();
			}
		}
	}

	public MBObjectBase CreateObjectFromXmlNode(XmlNode node)
	{
		string name = node.Name;
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.ElementName == name)
			{
				string value = node.Attributes["id"].Value;
				MBObjectBase presumedObject = GetPresumedObject(objectTypeRecord.ElementName, value);
				presumedObject.Deserialize(this, node);
				presumedObject.AfterInitialized();
				return presumedObject;
			}
		}
		return null;
	}

	public MBObjectBase CreateObjectWithoutDeserialize(XmlNode node)
	{
		string name = node.Name;
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			if (objectTypeRecord.ElementName == name)
			{
				string value = node.Attributes["id"].Value;
				MBObjectBase presumedObject = GetPresumedObject(objectTypeRecord.ElementName, value);
				presumedObject.Initialize();
				presumedObject.AfterInitialized();
				return presumedObject;
			}
		}
		return null;
	}

	public void UnregisterNonReadyObjects()
	{
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			List<MBObjectBase> list = new List<MBObjectBase>();
			foreach (MBObjectBase item in objectTypeRecord)
			{
				if (!item.IsReady)
				{
					list.Add(item);
				}
			}
			if (!list.Any())
			{
				continue;
			}
			foreach (MBObjectBase item2 in list)
			{
				Debug.Print("Null object reference found with ID: " + item2.StringId);
				UnregisterObject(item2);
			}
		}
	}

	public void ClearAllObjects()
	{
		for (int num = ObjectTypeRecords.Count - 1; num >= 0; num--)
		{
			List<MBObjectBase> list = new List<MBObjectBase>();
			foreach (MBObjectBase item in ObjectTypeRecords[num])
			{
				list.Add(item);
			}
			foreach (MBObjectBase item2 in list)
			{
				ObjectTypeRecords[num].UnregisterMBObject(item2);
				AfterUnregisterObject(item2);
			}
		}
	}

	public void ClearAllObjectsWithType(Type type)
	{
		for (int num = ObjectTypeRecords.Count - 1; num >= 0; num--)
		{
			if (ObjectTypeRecords[num].ObjectClass == type)
			{
				List<MBObjectBase> list = new List<MBObjectBase>();
				foreach (MBObjectBase item in ObjectTypeRecords[num])
				{
					list.Add(item);
				}
				foreach (MBObjectBase item2 in list)
				{
					UnregisterObject(item2);
				}
			}
		}
	}

	public T ReadObjectReferenceFromXml<T>(string attributeName, XmlNode node) where T : MBObjectBase
	{
		if (node.Attributes[attributeName] == null)
		{
			return null;
		}
		string value = node.Attributes[attributeName].Value;
		string text = value.Split(".".ToCharArray())[0];
		if (text == value)
		{
			throw new MBInvalidReferenceException(value);
		}
		string text2 = value.Split(".".ToCharArray())[1];
		if (text == string.Empty || text2 == string.Empty)
		{
			throw new MBInvalidReferenceException(value);
		}
		return GetPresumedObject(text, text2) as T;
	}

	public MBObjectBase ReadObjectReferenceFromXml(string attributeName, Type objectType, XmlNode node)
	{
		if (node.Attributes[attributeName] == null)
		{
			return null;
		}
		string value = node.Attributes[attributeName].Value;
		string text = value.Split(".".ToCharArray())[0];
		if (text == value)
		{
			throw new MBInvalidReferenceException(value);
		}
		string text2 = value.Split(".".ToCharArray())[1];
		if (text == string.Empty || text2 == string.Empty)
		{
			throw new MBInvalidReferenceException(value);
		}
		return GetPresumedObject(text, text2);
	}

	public T CreateObject<T>(string stringId) where T : MBObjectBase, new()
	{
		T val = new T
		{
			StringId = stringId
		};
		RegisterObject(val);
		if (_handlers != null)
		{
			foreach (IObjectManagerHandler handler in _handlers)
			{
				handler.AfterCreateObject(val);
			}
		}
		return val;
	}

	public T CreateObject<T>() where T : MBObjectBase, new()
	{
		return CreateObject<T>(typeof(T).Name.ToString() + "_1");
	}

	public void DebugPrint(PrintOutputDelegate printOutput)
	{
		printOutput("-Printing MBObjectManager Debug-");
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			printOutput(objectTypeRecord.DebugBasicDump());
		}
	}

	public void AddHandler(IObjectManagerHandler handler)
	{
		if (_handlers == null)
		{
			_handlers = new List<IObjectManagerHandler>();
		}
		_handlers.Add(handler);
	}

	public void RemoveHandler(IObjectManagerHandler handler)
	{
		_handlers.Remove(handler);
	}

	public string DebugDump()
	{
		string text = "";
		text += "--------------------------------------\r\n";
		text += "----Printing MBObjectManager Debug----\r\n";
		text += "--------------------------------------\r\n";
		text += "\r\n";
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			text += objectTypeRecord.DebugDump();
		}
		File.WriteAllText("mbobjectmanagerdump.txt", text);
		return text;
	}

	public void ReInitialize()
	{
		foreach (IObjectTypeRecord item in ObjectTypeRecords.ToList())
		{
			item.ReInitialize();
		}
	}

	public string GetObjectTypeIds()
	{
		string text = "";
		foreach (IObjectTypeRecord objectTypeRecord in ObjectTypeRecords)
		{
			text = text + objectTypeRecord.TypeNo + " - " + objectTypeRecord.GetType().FullName + "\n";
		}
		return text;
	}
}
