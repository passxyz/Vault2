using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using KPCLib;
using PassXYZLib;
using PassXYZ.Vault.Services;
using PassXYZ.Vault.ViewModels;
using System.Windows.Input;

namespace PassXYZ.Vault.Tests.ViewModels
{
    public class OtpListViewModelTests
    {
        ILogger<OtpListViewModel> logger;
        readonly IDataStore<Item> dataStore;
        User _user;

        public OtpListViewModelTests()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder.AddDebug()
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug));
            logger = loggerFactory.CreateLogger<OtpListViewModel>();
            dataStore = new DataStore();
            _user = new User
            {
                Username = "test1",
                Password = "12345"
            };
        }

        [Fact]
        public async Task GetOtpListTest() 
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            OtpListViewModel vm = new(dataStore, logger);
            ICommand command = vm.GetOtpListCommand;
            command.Execute(null);
            System.Threading.Thread.Sleep(1000);
        }

        [Fact]
        public async Task UpdateTokensTest() 
        {
            bool result = await dataStore.ConnectAsync(_user);
            Assert.True(result);
            OtpListViewModel vm = new(dataStore, logger);
            ICommand getOtpList = vm.GetOtpListCommand;
            getOtpList.Execute(null);
            System.Threading.Thread.Sleep(1000);
            ICommand updateTokens = vm.UpdateTokensCommand;
            updateTokens.Execute(null);
        }
    }
}
