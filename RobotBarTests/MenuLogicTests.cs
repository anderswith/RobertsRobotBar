using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace UnitTests
{
    [TestFixture]
    public class MenuLogicTests
    {
        private Mock<IMenuRepository> _menuRepoMock;
        private Mock<IDrinkRepository> _drinkRepoMock;
        private Mock<IEventRepository> _eventRepoMock;
        private MenuLogic _menuLogic;

        [SetUp]
        public void Setup()
        {
            _menuRepoMock = new Mock<IMenuRepository>();
            _drinkRepoMock = new Mock<IDrinkRepository>();
            _eventRepoMock = new Mock<IEventRepository>();
            _menuLogic = new MenuLogic(_menuRepoMock.Object, _drinkRepoMock.Object, _eventRepoMock.Object);
        }

        // ---------- AddMenuWithDrinks ----------

        [Test]
        public void AddMenuWithDrinks_ShouldThrow_WhenNameIsNullOrEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _menuLogic.AddMenuWithDrinks("", new List<Guid> { Guid.NewGuid() }));

            Assert.That(ex.Message, Is.EqualTo("Menu name cannot be null or empty."));
        }

        [Test]
        public void AddMenuWithDrinks_ShouldThrow_WhenDrinkIdsIsNullOrEmpty()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _menuLogic.AddMenuWithDrinks("Weekend Specials", new List<Guid>()));

            Assert.That(ex.Message, Is.EqualTo("No valid drinks found for the provided IDs."));
        }

        [Test]
        public void AddMenuWithDrinks_ShouldCallAddMenu_WhenValidInputs()
        {
            // Arrange
            var drinkIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var drinks = drinkIds.Select(id => new Drink { DrinkId = id, Name = "Test" }).ToList();

            _drinkRepoMock.Setup(r => r.GetDrinksByIds(drinkIds)).Returns(drinks);

            // Act
            _menuLogic.AddMenuWithDrinks("Cocktail Menu", drinkIds);

            // Assert
            _menuRepoMock.Verify(r => r.AddMenu(It.Is<Menu>(m =>
                m.Name == "Cocktail Menu" &&
                m.MenuContents.Count == drinks.Count
            )), Times.Once);
        }

        // ---------- GetAllMenus ----------

        [Test]
        public void GetAllMenus_ShouldCallRepositoryOnce()
        {
            _menuLogic.GetAllMenus();
            _menuRepoMock.Verify(r => r.GetAllMenus(), Times.Once);
        }

        // ---------- GetMenuById ----------

        [Test]
        public void GetMenuById_ShouldCallRepositoryOnce()
        {
            var id = Guid.NewGuid();
            _menuLogic.GetMenuById(id);
            _menuRepoMock.Verify(r => r.GetMenuById(id), Times.Once);
        }

        // ---------- DeleteMenu ----------

        [Test]
        public void DeleteMenu_ShouldThrow_WhenMenuNotFound()
        {
            _menuRepoMock.Setup(r => r.GetMenuById(It.IsAny<Guid>())).Returns((Menu)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _menuLogic.DeleteMenu(Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("Menu not found."));
        }

        [Test]
        public void DeleteMenu_ShouldCallRepository_WhenMenuExists()
        {
            var menu = new Menu { MenuId = Guid.NewGuid(), Name = "Brunch Menu" };
            _menuRepoMock.Setup(r => r.GetMenuById(menu.MenuId)).Returns(menu);

            _menuLogic.DeleteMenu(menu.MenuId);

            _menuRepoMock.Verify(r => r.DeleteMenu(menu), Times.Once);
        }

        // ---------- UpdateMenu ----------

        [Test]
        public void UpdateMenu_ShouldThrow_WhenNameIsNullOrEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _menuLogic.UpdateMenu(Guid.NewGuid(), "", new List<Guid> { Guid.NewGuid() }));

            Assert.That(ex.Message, Is.EqualTo("Menu name cannot be null or empty."));
        }

        [Test]
        public void UpdateMenu_ShouldThrow_WhenDrinkIdsIsNullOrEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _menuLogic.UpdateMenu(Guid.NewGuid(), "Evening Menu", new List<Guid>()));

            Assert.That(ex.Message, Is.EqualTo("Menu must contain at least one drink."));
        }

        [Test]
        public void UpdateMenu_ShouldThrow_WhenMenuNotFound()
        {
            _menuRepoMock.Setup(r => r.GetMenuById(It.IsAny<Guid>())).Returns((Menu)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _menuLogic.UpdateMenu(Guid.NewGuid(), "Lunch Menu", new List<Guid> { Guid.NewGuid() }));

            Assert.That(ex.Message, Is.EqualTo("Menu not found."), "KeyNotFoundException expected.");
        }

        [Test]
        public void UpdateMenu_ShouldThrow_WhenNoValidDrinksFound()
        {
            var menu = new Menu { MenuId = Guid.NewGuid(), Name = "Dinner Menu", MenuContents = new List<MenuContent>() };
            _menuRepoMock.Setup(r => r.GetMenuById(menu.MenuId)).Returns(menu);
            _drinkRepoMock.Setup(r => r.GetDrinksByIds(It.IsAny<List<Guid>>())).Returns(new List<Drink>());

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _menuLogic.UpdateMenu(menu.MenuId, "Updated Menu", new List<Guid> { Guid.NewGuid() }));

            Assert.That(ex.Message, Is.EqualTo("No valid drinks found for the provided IDs."));
        }

        [Test]
        public void UpdateMenu_ShouldCallRepository_WhenValidInputs()
        {
            var menu = new Menu
            {
                MenuId = Guid.NewGuid(),
                Name = "Old Menu",
                MenuContents = new List<MenuContent>()
            };

            var drinkIds = new List<Guid> { Guid.NewGuid() };
            var drinks = drinkIds.Select(id => new Drink { DrinkId = id, Name = "Whiskey Sour" }).ToList();

            _menuRepoMock.Setup(r => r.GetMenuById(menu.MenuId)).Returns(menu);
            _drinkRepoMock.Setup(r => r.GetDrinksByIds(drinkIds)).Returns(drinks);

            _menuLogic.UpdateMenu(menu.MenuId, "New Menu", drinkIds);

            _menuRepoMock.Verify(r => r.UpdateMenu(It.Is<Menu>(m =>
                m.Name == "New Menu" &&
                m.MenuContents.Count == 1 &&
                m.MenuContents.First().DrinkId == drinks.First().DrinkId
            )), Times.Once);
        }
        
         // ---------- GetMenuForEvent ----------

        [Test]
        public void GetMenuForEvent_ShouldThrow_WhenEventIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _menuLogic.GetMenuForEvent(Guid.Empty));

            Assert.That(ex.Message, Is.EqualTo("Event ID cannot be empty."));
        }



        [Test]
        public void GetMenuForEvent_ShouldThrow_WhenEventNotFound()
        {
            var eventId = Guid.NewGuid();

            _eventRepoMock
                .Setup(r => r.GetEventById(eventId))
                .Returns((Event?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _menuLogic.GetMenuForEvent(eventId));

            Assert.That(ex.Message, Is.EqualTo("Event not found."));
        }
        

        [Test]
        public void GetMenuForEvent_ShouldThrow_WhenMenuNotFound()
        {
            var eventId = Guid.NewGuid();
            var menuId = Guid.NewGuid();

            var ev = new Event
            {
                EventId = eventId,
                MenuId = menuId
            };

            _eventRepoMock
                .Setup(r => r.GetEventById(eventId))
                .Returns(ev);

            _menuRepoMock
                .Setup(r => r.GetMenuWithDrinksAndIngredients(menuId))
                .Returns((Menu?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _menuLogic.GetMenuForEvent(eventId));

            Assert.That(ex.Message, Is.EqualTo("Menu not found."));
        }
        

        [Test]
        public void GetMenuForEvent_ShouldReturnMenu_WhenEventAndMenuExist()
        {
            var eventId = Guid.NewGuid();
            var menuId = Guid.NewGuid();

            var ev = new Event
            {
                EventId = eventId,
                MenuId = menuId
            };

            var menu = new Menu
            {
                MenuId = menuId,
                Name = "Cocktail Menu"
            };

            _eventRepoMock
                .Setup(r => r.GetEventById(eventId))
                .Returns(ev);

            _menuRepoMock
                .Setup(r => r.GetMenuWithDrinksAndIngredients(menuId))
                .Returns(menu);

            var result = _menuLogic.GetMenuForEvent(eventId);

            Assert.That(result, Is.EqualTo(menu));

            _eventRepoMock.Verify(r => r.GetEventById(eventId), Times.Once);
            _menuRepoMock.Verify(r => r.GetMenuWithDrinksAndIngredients(menuId), Times.Once);
            
        }
        
        
    }
}
