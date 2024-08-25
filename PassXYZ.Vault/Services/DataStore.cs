using System.Diagnostics;
using System.Collections.ObjectModel;

using SkiaSharp;

using KeePassLib;
using KPCLib;
using KeePassLib.Security;
using PassXYZLib;


namespace PassXYZ.Vault.Services;

public class EmbeddedDatabase
{
    public string Path;
    public string Key;
    public string ResourcePath;

    public EmbeddedDatabase(string path, string key, string rpath)
    {
        Path = path;
        Key = key;
        ResourcePath = rpath;
    }
}

public static class EmbeddedIcons
{
    public static EmbeddedDatabase iconZipFile = new EmbeddedDatabase(Path.Combine(PxDataFile.TmpFilePath, "icons.zip"),
        "icons", "PassXYZ.Vault.data.icons.zip");

    public static EmbeddedDatabase[] IconFiles = new EmbeddedDatabase[]
    {
            new EmbeddedDatabase(Path.Combine(PxDataFile.IconFilePath, "ic_passxyz_cloud.png"), "ic_passxyz_cloud", "PassXYZ.Vault.data.ic_passxyz_cloud.png"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.IconFilePath, "ic_passxyz_local.png"), "ic_passxyz_local", "PassXYZ.Vault.data.ic_passxyz_local.png"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.IconFilePath, "ic_passxyz_disabled.png"), "ic_passxyz_disabled", "PassXYZ.Vault.data.ic_passxyz_disabled.png"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.IconFilePath, "ic_passxyz_sync.png"), "ic_passxyz_sync", "PassXYZ.Vault.data.ic_passxyz_sync.png"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.IconFilePath, "ic_passxyz_synced.png"), "ic_passxyz_synced", "PassXYZ.Vault.data.ic_passxyz_synced.png")
    };
}

public static class TEST_DB
{
    public static EmbeddedDatabase[] DataFiles = new EmbeddedDatabase[]
    {
            new EmbeddedDatabase(Path.Combine(PxDataFile.DataFilePath, "pass_d_E8f4pEk.xyz"), "12345", "PassXYZ.Vault.data.pass_d_E8f4pEk.xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.DataFilePath, "pass_e_JyHzpRxcopt.xyz"), "123123", "PassXYZ.Vault.data.pass_e_JyHzpRxcopt.xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.KeyFilePath, "pass_k_JyHzpRxcopt.k4xyz"), "", "PassXYZ.Vault.data.pass_k_JyHzpRxcopt.k4xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.DataFilePath, "pass_e_2TjEf1Dy9V2jiEgbS.xyz"), "123123", "PassXYZ.Vault.data.pass_e_2TjEf1Dy9V2jiEgbS.xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.KeyFilePath, "pass_k_2TjEf1Dy9V2jiEgbS.k4xyz"), "", "PassXYZ.Vault.data.pass_k_2TjEf1Dy9V2jiEgbS.k4xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.DataFilePath, "pass_e_VpdPx4ZcUZs8Fpzpmuu.xyz"), "123123", "PassXYZ.Vault.data.pass_e_VpdPx4ZcUZs8Fpzpmuu.xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.KeyFilePath, "pass_k_VpdPx4ZcUZs8Fpzpmuu.k4xyz"), "", "PassXYZ.Vault.data.pass_k_VpdPx4ZcUZs8Fpzpmuu.k4xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.DataFilePath, "pass_e_WCXaKYYvXygN3nVYW3u.xyz"), "123123", "PassXYZ.Vault.data.pass_e_WCXaKYYvXygN3nVYW3u.xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.KeyFilePath, "pass_k_WCXaKYYvXygN3nVYW3u.k4xyz"), "", "PassXYZ.Vault.data.pass_k_WCXaKYYvXygN3nVYW3u.k4xyz"),
    };
}

public class DataStore : IDataStore<Item>
{
    private readonly PasswordDb _db = default!;
    private PwGroup? _currentGroup = null;
    private User? _user = default;

    public DataStore()
    {
        _db = PasswordDb.Instance;
    }

    public List<Item> Items => _currentGroup!.GetItems();

    /// <summary>
    /// Set the current group.
    /// If the group is null, set to root group.
    /// </summary>
    /// <param name="item">an instance of <c>PwGroup</c></param>
    /// <returns>Returns the current group name</returns>
    public string SetCurrentGroup(Item? item = default)
    {
        if(item == default) 
        { 
            _currentGroup = _db.RootGroup;
            return _db.RootGroup.Name;
        }

        if (item is PwGroup group)
        {
            _db.LastSelectedGroup = group.Uuid;
            if (_db.RootGroup.Uuid == _db.LastSelectedGroup || _db.LastSelectedGroup.Equals(PwUuid.Zero))
            {
                _db.LastSelectedGroup = _db.RootGroup.Uuid;
                _currentGroup = _db.RootGroup;
            }
            else
                _currentGroup = group;
            return _currentGroup.Name;
        }
        else 
        {
            throw new ArgumentException("Item must be a group", nameof(item));
        }
    }

    public async Task<bool> AddItemAsync(Item item)
    {
        if (item == null || _currentGroup == null) { throw new ArgumentNullException(nameof(item)); }

        if (item.IsGroup)
        {
            _currentGroup.AddGroup(item as PwGroup, true);
        }
        else
        {
            _currentGroup.AddEntry(item as PwEntry, true);
        }
        await _db.SaveAsync();
        return await Task.FromResult(true);
    }

    public async Task<bool> UpdateItemAsync(Item item)
    {
        if(item == null) { throw new ArgumentNullException(nameof(item)); }
        var oldItem = Items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
        if (oldItem != null) 
        {
            item.LastModificationTime = DateTime.UtcNow;
            await _db.SaveAsync();
            return await Task.FromResult(true);
        }
        else
        {
            return await Task.FromResult(false);
        }
    }

    public async Task<bool> DeleteItemAsync(string id)
    {
        if (id == null) { throw new ArgumentNullException(nameof(id)); }

        Item? oldItem = Items.Where((Item arg) => arg.Id == id).FirstOrDefault();

        if (oldItem != null && Items.Remove(oldItem))
        {
            if (oldItem.IsGroup)
            {
                _db.DeleteGroup(oldItem as PwGroup);
            }
            else
            {
                _db.DeleteEntry(oldItem as PwEntry);
            }
            await _db.SaveAsync();
            return await Task.FromResult(true);
        }
        else
        {
            return await Task.FromResult(false);
        }
    }

    public Item? GetItem(string id)
    {
        if (id == null) { throw new ArgumentNullException(nameof(id)); }

        var item = Items.FirstOrDefault(s => s.Id == id);
        if (item != null) { return item; }

        item = _db.FindEntryById(id);
        if (item != null) { return item; }
        else
        {
            return _db.RootGroup.FindGroup(id, true);
        }
    }

    public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
    {
        if (_currentGroup == null)
        {
            Debug.WriteLine($"MockDataStore: CurrentGroup is null!");
            return Enumerable.Empty<Item>();
        }
        return await Task.FromResult(Items);
    }

    public async Task<bool> ConnectAsync(User user)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password)) 
            {
                throw new ArgumentNullException(nameof(user), "Username or password cannot be null");
            }

            _db.Open(user);
            if (_db.IsOpen)
            {
                _db.CurrentGroup = _db.RootGroup;
                _user = user;
            }
            return _db.IsOpen;
        });
    }

    public async Task SignUpAsync(User user)
    {
        if (user == null) { Debug.Assert(false); throw new ArgumentNullException(nameof(user)); }

        var logger = new KPCLibLogger();
        await Task.Run(() => {
            _db.New(user);

            // Create a PassXYZ Usage note entry
            PwEntry pe = new(true, true);
            pe.Strings.Set(PxDefs.TitleField, new ProtectedString(false, Properties.Resources.entry_id_passxyz_usage));
            pe.Strings.Set(PxDefs.NotesField, new ProtectedString(false, Properties.Resources.about_passxyz_usage));
            pe.SetType(ItemSubType.Notes);
            _db.RootGroup.AddEntry(pe, true);

            try
            {
                logger.StartLogging("Saving database ...", true);
                _db.DescriptionChanged = DateTime.UtcNow;
                _db.Save(logger);
                logger.EndLogging();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to save database." + e.Message);
            }
        });

    }

    public void Close()
    {
        if (_db.IsOpen) 
        {
            Debug.WriteLine($"Closing database {_db.Name}.");
            _db.Close(); 
        }
    }

    /// <summary>
    /// This is a factory method to create a new item.
    /// </summary>
    /// <param name="type">type of item</param>
    /// <returns>an instance of PwEntry or PwGroup</returns>
    public Item? CreateNewItem(ItemSubType type)
    {
        Item? newItem = default;

        if (type == ItemSubType.Group)
        {
            newItem = new PwGroup(true, true);
        }
        else if (type != ItemSubType.None)
        {
            PwEntry entry = new PwEntry(true, true);
            entry.SetType(type);

            // Init standard field
            if (type == ItemSubType.Entry)
            {
                entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, ""));
                entry.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, ""));
                entry.Strings.Set(PwDefs.UrlField, new ProtectedString(false, ""));
            }
            else if (type == ItemSubType.PxEntry)
            {
                uint idx = 0;
                entry.Strings.Set(PxDefs.EncodeKey(Properties.Resources.field_id_username, idx++), new ProtectedString(false, ""));
                entry.Strings.Set(PxDefs.EncodeKey(Properties.Resources.field_id_password, idx++), new ProtectedString(true, ""));
                entry.Strings.Set(PxDefs.EncodeKey(Properties.Resources.field_id_url, idx++), new ProtectedString(false, ""));
                entry.Strings.Set(PxDefs.EncodeKey(Properties.Resources.field_id_email, idx++), new ProtectedString(false, ""));
                entry.Strings.Set(PxDefs.EncodeKey(Properties.Resources.field_id_mobile, idx++), new ProtectedString(false, ""));
            }

            newItem = entry;
        }
        return newItem;
    }

    /// <summary>
    /// Search entries with a keyword
    /// </summary>
    /// <param name="strSearch">keyword to be searched. If it is null, all items will be returned.</param>
    /// <param name="itemGroup">If it is not null, this group is searched</param>
    /// <returns>a list of entries</returns>
    public async Task<IEnumerable<Item>> SearchEntriesAsync(string? strSearch = null, Item? itemGroup = null)
    {
        return await Task.Run(() => { return _db.SearchEntries(strSearch, itemGroup); });
    }

    /// <summary>
    /// Delete a custom icon by uuid.
    /// </summary>
    /// <param name="uuid">uuid of the custom icon</param>
    /// <returns>success or failure</returns>
    public async Task<bool> DeleteCustomIconAsync(PwUuid uuidIcon)
    {
        List<PwUuid> vUuidsToDelete = new List<PwUuid>();

        if (uuidIcon == null) { return false; }
        vUuidsToDelete.Add(uuidIcon);
        bool result = _db.DeleteCustomIcons(vUuidsToDelete);
        if (result)
        {
            // Save the database to take effect
            await _db.SaveAsync();

        }
        return result;
    }

    /// <summary>
    /// Get the image source from the custom icon in the database
    /// </summary>
    /// <param name="uuid">UUID of custom icon</param>
    /// <returns>ImageSource or null</returns>
    public ImageSource? GetBuiltInImage(PwUuid uuid)
    {
        PwCustomIcon customIcon = _db.GetPwCustomIcon(uuid);
        if (customIcon != null)
        {
            byte[] pb = customIcon.ImageDataPng;
            SKBitmap bitmap = PxItem.LoadImage(pb);
            return PxItem.GetImageSource(bitmap);
        }
        return null;
    }

    private PxIcon GetPxIcon(PwCustomIcon pwci) 
    { 
        return new PxIcon
        {
            IconType = PxIconType.PxEmbeddedIcon,
            Uuid = pwci.Uuid,
            Name = pwci.Name,
            ImgSource = GetBuiltInImage(pwci.Uuid),
        };
    }

    public ObservableCollection<PxIcon> GetIcons(string? searchText = null)
    {
        ObservableCollection<PxIcon> icons = new ObservableCollection<PxIcon>();
        List<PwCustomIcon> customIconList = _db.CustomIcons;
        foreach (PwCustomIcon pwci in customIconList)
        {
            if (searchText == null) 
            {
                icons.Add(GetPxIcon(pwci));
            }
            else
            {
                if (pwci.Name.Contains(searchText)) 
                {
                    icons.Add(GetPxIcon(pwci));
                }
            }
        }
        return icons;
    }

    public async Task<bool> ChangeMasterPassword(string newPassword)
    {
        bool result = _db.ChangeMasterPassword(newPassword, _user);
        if (result)
        {
            _db.MasterKeyChanged = DateTime.UtcNow;
            // Save the database to take effect
            await _db.SaveAsync();
        }
        return result;
    }
}
