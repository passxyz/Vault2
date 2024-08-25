using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using KPCLib;
using PassXYZLib;
using PassXYZ.Vault.Services;
using PassXYZ.Vault.ViewModels;
using System.Windows.Input;

namespace PassXYZ.Vault.Tests.ViewModels
{
    public class IconSearchViewModelTests
    {
        ILogger<IconSearchViewModel> logger;
        readonly IDataStore<Item> dataStore;
        User _user;

        public IconSearchViewModelTests()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder.AddDebug()
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug));
            logger = loggerFactory.CreateLogger<IconSearchViewModel>();
            dataStore = new DataStore();
            _user = new User
            {
                Username = "test1",
                Password = "12345"
            };
        }

        [Fact]
        public async Task LoadIconsTest()
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            IconSearchViewModel vm = new(dataStore, logger);
            vm.Search("Test");
            System.Threading.Thread.Sleep(1000);
        }

    }
}
