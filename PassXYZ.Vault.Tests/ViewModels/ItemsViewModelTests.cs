﻿using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Core.UnitTests;
using NSubstitute;
using KPCLib;
using PassXYZ.Vault.Services;
using PassXYZ.Vault.ViewModels;
using PassXYZ.Vault.Views;
using PassXYZLib;
using Microsoft.Maui.Controls;

namespace PassXYZ.Vault.Tests.ViewModels
{
    public class ItemsViewModelTests : ShellTestBase
    {
        ILogger<ItemsViewModel> logger;
        readonly IDataStore<Item> dataStore;
        TestShell shell;

        public ItemsViewModelTests() 
        {
            shell = new TestShell();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ContentPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            var abougPage = new ShellItem { Route = "About" };
            var page = MakeSimpleShellSection("Readme", "content");
            abougPage.Items.Add(page);
            shell.Items.Add(abougPage);

            Application app = Substitute.For<Application>();
            //app.MainPage = shell;

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole()
                .SetMinimumLevel(LogLevel.Debug));
            logger = loggerFactory.CreateLogger<ItemsViewModel>();
            dataStore = new DataStore();
        }

        [Fact]
        public void NoItemSelectionChangedTest() 
        {
            ItemsViewModel vm = new(dataStore, logger);
            // Act
            vm.ItemSelectionChangedCommand.Execute(null);
            // Assert
            Assert.Null(vm.SelectedItem);
        }

        [Fact(Skip = "Need to fix later")]
        public async void SelectEntryTest()
        {
            // Arrange
            Item newItem = new PxEntry()
            {
                Name = "New item 1",
                Notes = "This is a new item.",
            };
            ItemsViewModel vm = new(dataStore, logger);
            await shell.GoToAsync("//About/Readme/");
            // Act
            vm.ItemSelectionChangedCommand.Execute(newItem);
            // Assert
            Assert.Null(vm.SelectedItem);
        }

        [Fact]
        public void LoadItemsTest() 
        {
            // Arrange
            ItemsViewModel vm = new(dataStore, logger);
            // Act
            vm.LoadItemsCommand.Execute(vm);
            // Assert
            Assert.Empty(vm.Items);
        }
    }
}
