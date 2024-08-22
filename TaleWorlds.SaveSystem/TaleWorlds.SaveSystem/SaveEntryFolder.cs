using System.Collections.Generic;

namespace TaleWorlds.SaveSystem;

public class SaveEntryFolder
{
	private Dictionary<FolderId, SaveEntryFolder> _saveEntryFolders;

	private Dictionary<EntryId, SaveEntry> _entries;

	public int GlobalId { get; private set; }

	public int ParentGlobalId { get; private set; }

	public FolderId FolderId { get; private set; }

	public IEnumerable<SaveEntry> AllEntries
	{
		get
		{
			foreach (SaveEntry value in _entries.Values)
			{
				yield return value;
			}
			foreach (SaveEntryFolder value2 in _saveEntryFolders.Values)
			{
				foreach (SaveEntry allEntry in value2.AllEntries)
				{
					yield return allEntry;
				}
			}
		}
	}

	public Dictionary<EntryId, SaveEntry>.ValueCollection ChildEntries => _entries.Values;

	public Dictionary<FolderId, SaveEntryFolder>.ValueCollection ChildFolders => _saveEntryFolders.Values;

	public static SaveEntryFolder CreateRootFolder()
	{
		return new SaveEntryFolder(-1, -1, new FolderId(-1, SaveFolderExtension.Root), 3);
	}

	public SaveEntryFolder(SaveEntryFolder parent, int globalId, FolderId folderId, int entryCount)
		: this(parent.GlobalId, globalId, folderId, entryCount)
	{
	}

	public SaveEntryFolder(int parentGlobalId, int globalId, FolderId folderId, int entryCount)
	{
		ParentGlobalId = parentGlobalId;
		GlobalId = globalId;
		FolderId = folderId;
		_entries = new Dictionary<EntryId, SaveEntry>(entryCount);
		_saveEntryFolders = new Dictionary<FolderId, SaveEntryFolder>(3);
	}

	public void AddEntry(SaveEntry saveEntry)
	{
		_entries.Add(saveEntry.Id, saveEntry);
	}

	public SaveEntry GetEntry(EntryId entryId)
	{
		return _entries[entryId];
	}

	public void AddChildFolderEntry(SaveEntryFolder saveEntryFolder)
	{
		_saveEntryFolders.Add(saveEntryFolder.FolderId, saveEntryFolder);
	}

	internal SaveEntryFolder GetChildFolder(FolderId folderId)
	{
		return _saveEntryFolders[folderId];
	}

	public SaveEntry CreateEntry(EntryId entryId)
	{
		SaveEntry saveEntry = SaveEntry.CreateNew(this, entryId);
		AddEntry(saveEntry);
		return saveEntry;
	}
}
