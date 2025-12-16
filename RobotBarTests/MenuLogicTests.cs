using Moq;
using NUnit.Framework;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.Tests.BLL
{
    [TestFixture]
    public class MenuLogicTests
    {
        private Mock<IMenuRepository> _menuRepoMock;
        private Mock<IDrinkRepository> _drinkRepoMock;
        private Mock<IEventRepository> _eventRepoMock;

        private MenuLogic _logic;

        [SetUp]
        public void Setup()
        {
            _menuRepoMock = new Mock<IMenuRepository>();
            _drinkRepoMock = new Mock<IDrinkRepository>();
            _eventRepoMock = new Mock<IEventRepository>();

            _logic = new MenuLogic(_menuRepoMock.Object, _drinkRepoMock.Object, _eventRepoMock.Object);
        }


        // AddDrinksToMenu
        [Test]
        public void AddDrinksToMenu_Throws_WhenDrinkIdsNull()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.AddDrinksToMenu(null, Guid.NewGuid()));
        }

        [Test]
        public void AddDrinksToMenu_Throws_WhenDrinkIdsEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.AddDrinksToMenu(new List<Guid>(), Guid.NewGuid()));
        }

        [Test]
        public void AddDrinksToMenu_Throws_WhenMenuNotFound()
        {
            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns((Menu)null);

            Assert.Throws<NullReferenceException>(() =>
                _logic.AddDrinksToMenu(new List<Guid> { Guid.NewGuid() }, Guid.NewGuid()));
        }

        [Test]
        public void AddDrinksToMenu_CallsRepository_OnSuccess()
        {
            var eventId = Guid.NewGuid();
            var drinkIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var menu = new Menu { MenuId = Guid.NewGuid(), MenuContents = new List<MenuContent>() };

            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(eventId))
                .Returns(menu);

            _logic.AddDrinksToMenu(drinkIds, eventId);

            _menuRepoMock.Verify(r => r.AddDrinksToMenu(menu.MenuId, drinkIds), Times.Once);
        }
        
        // GetAllMenus
        [Test]
        public void GetAllMenus_ReturnsRepositoryValue()
        {
            var menus = new List<Menu> { new Menu(), new Menu() };
            _menuRepoMock.Setup(r => r.GetAllMenus()).Returns(menus);

            var result = _logic.GetAllMenus();

            Assert.That(result, Is.EqualTo(menus));
        }

        // GetMenuById
        [Test]
        public void GetMenuById_ReturnsMenu()
        {
            var menuId = Guid.NewGuid();
            var menu = new Menu { MenuId = menuId };

            _menuRepoMock.Setup(r => r.GetMenuById(menuId)).Returns(menu);

            var result = _logic.GetMenuById(menuId);

            Assert.That(result, Is.EqualTo(menu));
        }


        // DeleteMenu

        [Test]
        public void DeleteMenu_Throws_WhenMenuNotFound()
        {
            _menuRepoMock.Setup(r => r.GetMenuById(It.IsAny<Guid>())).Returns((Menu)null);

            Assert.Throws<KeyNotFoundException>(() =>
                _logic.DeleteMenu(Guid.NewGuid()));
        }

        [Test]
        public void DeleteMenu_CallsRepository_WhenValid()
        {
            var menu = new Menu { MenuId = Guid.NewGuid() };

            _menuRepoMock.Setup(r => r.GetMenuById(menu.MenuId)).Returns(menu);

            _logic.DeleteMenu(menu.MenuId);

            _menuRepoMock.Verify(r => r.DeleteMenu(menu), Times.Once);
        }

        // UpdateMenu
        [Test]
        public void UpdateMenu_Throws_WhenNameIsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.UpdateMenu(Guid.NewGuid(), "", new List<Guid> { Guid.NewGuid() }));
        }

        [Test]
        public void UpdateMenu_Throws_WhenDrinkIdsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.UpdateMenu(Guid.NewGuid(), "MenuName", null));

            Assert.Throws<ArgumentException>(() =>
                _logic.UpdateMenu(Guid.NewGuid(), "MenuName", new List<Guid>()));
        }

        [Test]
        public void UpdateMenu_Throws_WhenMenuNotFound()
        {
            _menuRepoMock.Setup(r => r.GetMenuById(It.IsAny<Guid>()))
                .Returns((Menu)null);

            Assert.Throws<KeyNotFoundException>(() =>
                _logic.UpdateMenu(Guid.NewGuid(), "Test", new List<Guid> { Guid.NewGuid() }));
        }

        [Test]
        public void UpdateMenu_Throws_WhenDrinksNotFound()
        {
            var menu = new Menu { MenuId = Guid.NewGuid(), MenuContents = new List<MenuContent>() };
            _menuRepoMock.Setup(r => r.GetMenuById(menu.MenuId)).Returns(menu);

            _drinkRepoMock.Setup(r => r.GetDrinksByIds(It.IsAny<IEnumerable<Guid>>()))
                .Returns(new List<Drink>()); // empty drinks -> invalid

            Assert.Throws<InvalidOperationException>(() =>
                _logic.UpdateMenu(menu.MenuId, "Menu", new List<Guid> { Guid.NewGuid() }));
        }

        [Test]
        public void UpdateMenu_UpdatesMenuCorrectly()
        {
            var menuId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();

            var menu = new Menu
            {
                MenuId = menuId,
                Name = "Old Name",
                MenuContents = new List<MenuContent>()
            };

            var drink = new Drink { DrinkId = drinkId };

            _menuRepoMock.Setup(r => r.GetMenuById(menuId)).Returns(menu);
            _drinkRepoMock.Setup(r => r.GetDrinksByIds(It.IsAny<IEnumerable<Guid>>()))
                .Returns(new List<Drink> { drink });

            _logic.UpdateMenu(menuId, "New Name", new List<Guid> { drinkId });

            Assert.That(menu.Name, Is.EqualTo("New Name"));
            Assert.That(menu.MenuContents.Count, Is.EqualTo(1));
            Assert.That(menu.MenuContents.First().DrinkId, Is.EqualTo(drinkId));

            _menuRepoMock.Verify(r => r.UpdateMenu(menu), Times.Once);
        }
        
        // GetDrinksForMenu
        [Test]
        public void GetDrinksForMenu_Throws_WhenMenuNotFound()
        {
            _menuRepoMock.Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns((Menu)null);

            Assert.Throws<KeyNotFoundException>(() =>
                _logic.GetDrinksForMenu(Guid.NewGuid()));
        }

        [Test]
        public void GetDrinksForMenu_ReturnsEmpty_WhenNoDrinkIds()
        {
            var emptyMenu = new Menu
            {
                MenuContents = new List<MenuContent>()
            };

            _menuRepoMock.Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns(emptyMenu);

            var result = _logic.GetDrinksForMenu(Guid.NewGuid());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetDrinksForMenu_ReturnsDrinks()
        {
            var drinkId = Guid.NewGuid();

            var menu = new Menu
            {
                MenuContents = new List<MenuContent>
                {
                    new MenuContent { DrinkId = drinkId }
                }
            };

            var drink = new Drink { DrinkId = drinkId };

            _menuRepoMock.Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns(menu);

            _drinkRepoMock.Setup(r => r.GetDrinksByIds(It.IsAny<IEnumerable<Guid>>()))
                .Returns(new List<Drink> { drink });

            var result = _logic.GetDrinksForMenu(Guid.NewGuid());

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().DrinkId, Is.EqualTo(drinkId));
        }
        
        // RemoveDrinkFromMenu
        [Test]
        public void RemoveDrinkFromMenu_Throws_WhenMenuNotFound()
        {
            _menuRepoMock.Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns((Menu)null);

            Assert.Throws<KeyNotFoundException>(() =>
                _logic.RemoveDrinkFromMenu(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Test]
        public void RemoveDrinkFromMenu_Throws_WhenDrinkNotOnMenu()
        {
            var menu = new Menu
            {
                MenuContents = new List<MenuContent>()
            };

            _menuRepoMock.Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns(menu);

            Assert.Throws<KeyNotFoundException>(() =>
                _logic.RemoveDrinkFromMenu(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Test]
        public void RemoveDrinkFromMenu_RemovesEntry_AndUpdatesMenu()
        {
            var drinkId = Guid.NewGuid();

            var entry = new MenuContent { DrinkId = drinkId };
            var menu = new Menu
            {
                MenuContents = new List<MenuContent> { entry }
            };

            _menuRepoMock.Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns(menu);

            _logic.RemoveDrinkFromMenu(Guid.NewGuid(), drinkId);

            Assert.That(menu.MenuContents.Count, Is.EqualTo(0));
            _menuRepoMock.Verify(r => r.UpdateMenu(menu), Times.Once);
        }
    }
}
