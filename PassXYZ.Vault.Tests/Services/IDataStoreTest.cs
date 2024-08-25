using KPCLib;
using PassXYZ.Vault.Services;
using PassXYZLib;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PassXYZ.Vault.Tests.Services
{
    [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
    public class NonParallelCollectionDefinitionClass
    {
    }

    [Collection("Non-Parallel Collection")]
    public class IDataStoreTest
    {
        readonly IDataStore<Item> dataStore;
        User _user;
        public IDataStoreTest() 
        {
            dataStore = new DataStore();
            _user = new User
            {
                Username = "test1",
                Password = "12345"
            };
        }

        [Fact]
        public async Task GetItemsAsyncTest() 
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            var items = await dataStore.GetItemsAsync(true);
            Assert.NotEmpty(items);
        }

        [Fact]
        public async Task AddGroupAsyncTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            Item newItem = new PxGroup()
            {
                Name = "New Group 1",
                Notes = "This is a new group."
            };
            result = await dataStore.AddItemAsync(newItem);
            Assert.True(result);
        }

        [Fact]
        public async Task AddEntryAsyncTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            Item newItem = new PxEntry()
            {
                Name = "New Entry 1",
                Notes = "This is a new entry."
            };
            result = await dataStore.AddItemAsync(newItem);
            Assert.True(result);
        }

        [Fact]
        public async Task AddItemAsyncFailureTest() 
        {
            bool result = false;
#pragma warning disable CS8625 // Possible null reference argument.
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => result = await dataStore.AddItemAsync(null));
#pragma warning restore CS8625 // Possible null reference argument.
            Assert.Equal("Value cannot be null. (Parameter 'item')", ex.Message);
        }

        [Fact]
        public async Task UpdateItemAsyncTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            Item newItem = new PxEntry()
            {
                Name = "New item 1",
                Notes = "This is a new item."
            };
            result = await dataStore.AddItemAsync(newItem);
            Assert.True(result);
            newItem.Name = "Updated item 1";
            result = await dataStore.UpdateItemAsync(newItem);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateNullItemAsyncTest() 
        {
            bool result = false;
#pragma warning disable CS8625 // Possible null reference argument.
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => result = await dataStore.UpdateItemAsync(null));
#pragma warning restore CS8625 // Possible null reference argument.
            Assert.Equal("Value cannot be null. (Parameter 'item')", ex.Message);
        }

        [Fact]
        public async Task UpdateNoExistItemAsyncTest() 
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            Item newItem = new NewItem()
            {
                Name = "No item 1",
                Notes = "You cannot find this item."
            };
            result = await dataStore.UpdateItemAsync(newItem);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteItemAsyncTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            Item newItem = new PxEntry()
            {
                Name = "New item 1",
                Notes = "Please delete it."
            };
            result = await dataStore.AddItemAsync(newItem);
            Assert.True(result);
            result = await dataStore.DeleteItemAsync(newItem.Id);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteNullItemAsyncTest()
        {
            bool result = false;
#pragma warning disable CS8625 // Possible null reference argument.
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => result = await dataStore.DeleteItemAsync(null));
#pragma warning restore CS8625 // Possible null reference argument.
            Assert.Equal("Value cannot be null. (Parameter 'id')", ex.Message);
        }

        [Fact]
        public async Task DeleteNoExistItemAsyncTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            Item newItem = new PxEntry()
            {
                Name = "No item 1",
                Notes = "You cannot find this item."
            };
            result = await dataStore.DeleteItemAsync(newItem.Id);
            Assert.False(result);
        }

        [Fact]
        public async Task GetItemAsyncTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            Item newItem = new PxEntry()
            {
                Name = "New item 1",
                Notes = "This is a new item."
            };
            result = await dataStore.AddItemAsync(newItem);
            Assert.True(result);
            var item = dataStore.GetItem(newItem.Id);
            Assert.NotNull(item);
            Assert.Equal(newItem.Name, item.Name);
        }

        [Fact]
        public async Task GetNullItemAsyncTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

#pragma warning disable CS8625 // Possible null reference argument.
            var ex = Assert.Throws<ArgumentNullException>(() => dataStore.GetItem(null));
#pragma warning restore CS8625 // Possible null reference argument.
            Assert.Equal("Value cannot be null. (Parameter 'id')", ex.Message);
        }

        [Fact]
        public async Task GetNoExistItemAsyncTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            dataStore.SetCurrentGroup();

            Item newItem = new NewItem()
            {
                Name = "No item 1",
                Notes = "You cannot find this item."
            };
            var item = dataStore.GetItem(newItem.Id);
            Assert.Null(item);
        }

        /// <summary>
        /// Search entries without a keyword
        /// </summary>
        [Fact]
        public async Task SearchEntriesWithoutKeywordTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            var items = await dataStore.SearchEntriesAsync();
            Assert.True((items.Count() > 10));
            Debug.WriteLine($"Found {items.Count()} items");
        }

        /// <summary>
        /// Search entries with a keyword
        /// </summary>
        [Theory]
        [InlineData("Group")]
        [InlineData("Test")]
        public async Task SearchEntriesFoundKeywordTest(string keyword)
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            var items = await dataStore.SearchEntriesAsync(keyword);
            Assert.True((items.Count() > 0));
            Debug.WriteLine($"Found {items.Count()} items");
        }

        /// <summary>
        /// Search entries with a keyword
        /// </summary>
        [Theory]
        [InlineData("ststem")]
        [InlineData("c0nnect")]
        public async Task SearchEntriesNoKeywordFoundTest(string keyword)
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            var items = await dataStore.SearchEntriesAsync(keyword);
            Assert.True((items.Count() == 0));
            Debug.WriteLine($"Found {items.Count()} items");
        }

        [Fact]
        public async Task GetIconsTest() 
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            var icons = dataStore.GetIcons();
            Debug.WriteLine($"{icons.Count}");
        }
    }
}
