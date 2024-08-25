using KeePassLib;
using KPCLib;
using PassXYZLib;
using System.Collections.ObjectModel;

namespace PassXYZ.Vault.Services
{
    public interface IDataStore<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(string id);
        T? GetItem(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        string SetCurrentGroup(T? group = default);
        Task<bool> ConnectAsync(User user);
        Task SignUpAsync(User user);
        void Close();
        T? CreateNewItem(ItemSubType type);
        Task<IEnumerable<Item>> SearchEntriesAsync(string? strSearch = null, Item? itemGroup = null);
        ObservableCollection<PxIcon> GetIcons(string? searchText = null);
        Task<bool> DeleteCustomIconAsync(PwUuid uuidIcon);
        ImageSource GetBuiltInImage(PwUuid uuid);
        Task<bool> ChangeMasterPassword(string newPassword);
    }
}
