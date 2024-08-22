using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TaleWorlds.Library;

namespace TaleWorlds.SaveSystem;

internal class ArchiveConcurrentSerializer : IArchiveContext
{
	private int _entryCount;

	private int _folderCount;

	private object _locker;

	private Dictionary<int, BinaryWriter> _writers;

	private ConcurrentBag<SaveEntryFolder> _folders;

	public ArchiveConcurrentSerializer()
	{
		_locker = new object();
		_writers = new Dictionary<int, BinaryWriter>();
		_folders = new ConcurrentBag<SaveEntryFolder>();
	}

	public void SerializeFolderConcurrent(SaveEntryFolder folder)
	{
		int managedThreadId = Thread.CurrentThread.ManagedThreadId;
		BinaryWriter value;
		lock (_locker)
		{
			if (!_writers.TryGetValue(managedThreadId, out value))
			{
				value = new BinaryWriter(262144);
				_writers.Add(managedThreadId, value);
			}
		}
		foreach (SaveEntry allEntry in folder.AllEntries)
		{
			SerializeEntryConcurrent(allEntry, value);
		}
	}

	public SaveEntryFolder CreateFolder(SaveEntryFolder parentFolder, FolderId folderId, int entryCount)
	{
		int globalId = Interlocked.Increment(ref _folderCount) - 1;
		SaveEntryFolder saveEntryFolder = new SaveEntryFolder(parentFolder, globalId, folderId, entryCount);
		parentFolder.AddChildFolderEntry(saveEntryFolder);
		_folders.Add(saveEntryFolder);
		return saveEntryFolder;
	}

	private void SerializeEntryConcurrent(SaveEntry entry, BinaryWriter writer)
	{
		BinaryWriter binaryWriter = BinaryWriterFactory.GetBinaryWriter();
		binaryWriter.Write3ByteInt(entry.FolderId);
		binaryWriter.Write3ByteInt(entry.Id.Id);
		binaryWriter.WriteByte((byte)entry.Id.Extension);
		binaryWriter.WriteShort((short)entry.Data.Length);
		binaryWriter.WriteBytes(entry.Data);
		byte[] data = binaryWriter.Data;
		BinaryWriterFactory.ReleaseBinaryWriter(binaryWriter);
		writer.WriteBytes(data);
		Interlocked.Increment(ref _entryCount);
	}

	public byte[] FinalizeAndGetBinaryDataConcurrent()
	{
		BinaryWriter binaryWriter = new BinaryWriter();
		binaryWriter.WriteInt(_folderCount);
		foreach (SaveEntryFolder folder in _folders)
		{
			int parentGlobalId = folder.ParentGlobalId;
			int globalId = folder.GlobalId;
			int localId = folder.FolderId.LocalId;
			SaveFolderExtension extension = folder.FolderId.Extension;
			binaryWriter.Write3ByteInt(parentGlobalId);
			binaryWriter.Write3ByteInt(globalId);
			binaryWriter.Write3ByteInt(localId);
			binaryWriter.WriteByte((byte)extension);
		}
		binaryWriter.WriteInt(_entryCount);
		foreach (BinaryWriter value in _writers.Values)
		{
			binaryWriter.AppendData(value);
		}
		return binaryWriter.Data;
	}
}
