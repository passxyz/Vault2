using NSubstitute;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Extensions.Logging;

using KPCLib;
using PassXYZ.Vault.Services;
using PassXYZ.Vault.ViewModels;
using Microsoft.Maui.Controls;
using System;

namespace PassXYZ.Vault.Tests.ViewModels
{
    public class LoginViewModelTests : ShellTestBase
    {
        readonly ILogger<UserService> logger;
        readonly IDataStore<Item> dataStore;
        readonly UserService userService;

        public LoginViewModelTests()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder.AddDebug()
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug));
            logger = loggerFactory.CreateLogger<UserService>();
            dataStore = new DataStore();
            userService = new UserService(dataStore, logger);
        }

        [Fact]
        public async Task LoginCommandTest() 
        {
            // TODO: Need to fix this.
            var loginservice = new LoginService(userService);
            loginservice.Username = "test1";
            loginservice.Password = "12345";
            logger.LogInformation($"{loginservice.Username}");
            await loginservice.LoginAsync();
            System.Threading.Thread.Sleep(1000);
            var items = await dataStore.GetItemsAsync();
            System.Threading.Thread.Sleep(1000);
            logger.LogInformation($"Items={items.Count()}");
        }
    }
}
