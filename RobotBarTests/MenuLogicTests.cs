using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.DAL.Repositories.Interfaces;
using RobotBarApp.Services.Application.Interfaces;

namespace UnitTests
{
    [TestFixture]
    public class MenuLogicTests
    {
        private Mock<IMenuRepository> _menuRepoMock;
        private Mock<IDrinkRepository> _drinkRepoMock;
        private Mock<IEventRepository> _eventRepoMock;
        private Mock<IEventSessionService> _eventSessionMock;

        private MenuLogic _logic;

        [SetUp]
        public void Setup()
        {
            _menuRepoMock = new Mock<IMenuRepository>();
            _drinkRepoMock = new Mock<IDrinkRepository>();
            _eventRepoMock = new Mock<IEventRepository>();
            _eventSessionMock = new Mock<IEventSessionService>();

            _logic = new MenuLogic(
                _menuRepoMock.Object,
                _drinkRepoMock.Object,
                _eventRepoMock.Object,
                _eventSessionMock.Object);
        }

        // ---------- AddDrinksToMenu ----------

        [Test]
        public void AddDrinksToMenu_Throws_WhenDrinkIdsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.AddDrinksToMenu(null!, Guid.NewGuid()));

            Assert.Throws<ArgumentException>(() =>
                _logic.AddDrinksToMenu(new List<Guid>(), Guid.NewGuid()));
        }

        [Test]
        public void AddDrinksToMenu_Throws_WhenMenuNotFound()
        {
            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns((Menu?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _logic.AddDrinksToMenu(new List<Guid> { Guid.NewGuid() }, Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Menu not found for the event."));
        }

        [Test]
        public void AddDrinksToMenu_CallsRepository_WhenValid()
        {
            var eventId = Guid.NewGuid();
            var drinkIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var menu = new Menu
            {
                MenuId = Guid.NewGuid(),
                MenuContents = new List<MenuContent>()
            };

            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(eventId))
                .Returns(menu);

            _logic.AddDrinksToMenu(drinkIds, eventId);

            _menuRepoMock.Verify(
                r => r.AddDrinksToMenu(menu.MenuId, drinkIds),
                Times.Once);
        }

        // ---------- GetDrinksForMenu ----------

        [Test]
        public void GetDrinksForMenu_Throws_WhenMenuNotFound()
        {
            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns((Menu?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _logic.GetDrinksForMenu(Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Menu not found for this event."));
        }

        [Test]
        public void GetDrinksForMenu_ReturnsEmpty_WhenMenuHasNoDrinks()
        {
            var menu = new Menu
            {
                MenuContents = new List<MenuContent>()
            };

            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns(menu);

            var result = _logic.GetDrinksForMenu(Guid.NewGuid());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetDrinksForMenu_ReturnsDrinks_WhenPresent()
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

            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns(menu);

            _drinkRepoMock
                .Setup(r => r.GetDrinksByIds(It.IsAny<IEnumerable<Guid>>()))
                .Returns(new List<Drink> { drink });

            var result = _logic.GetDrinksForMenu(Guid.NewGuid()).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].DrinkId, Is.EqualTo(drinkId));
        }

        // ---------- RemoveDrinkFromMenu ----------

        [Test]
        public void RemoveDrinkFromMenu_Throws_WhenMenuNotFound()
        {
            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns((Menu?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _logic.RemoveDrinkFromMenu(Guid.NewGuid(), Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Menu not found for this event."));
        }

        [Test]
        public void RemoveDrinkFromMenu_Throws_WhenDrinkNotOnMenu()
        {
            var menu = new Menu
            {
                MenuContents = new List<MenuContent>()
            };

            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns(menu);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _logic.RemoveDrinkFromMenu(Guid.NewGuid(), Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Drink is not on the menu."));
        }

        [Test]
        public void RemoveDrinkFromMenu_RemovesDrink_AndUpdatesMenu()
        {
            var drinkId = Guid.NewGuid();

            var entry = new MenuContent { DrinkId = drinkId };
            var menu = new Menu
            {
                MenuContents = new List<MenuContent> { entry }
            };

            _menuRepoMock
                .Setup(r => r.GetMenuWithContentByEventId(It.IsAny<Guid>()))
                .Returns(menu);

            _logic.RemoveDrinkFromMenu(Guid.NewGuid(), drinkId);

            Assert.That(menu.MenuContents, Is.Empty);
            _menuRepoMock.Verify(r => r.UpdateMenu(menu), Times.Once);
        }

        // ---------- GetMenuWithDrinksAndIngredients ----------

        [Test]
        public void GetMenuWithDrinksAndIngredients_Throws_WhenNoActiveEvent()
        {
            _eventSessionMock.Setup(e => e.HasActiveEvent).Returns(false);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _logic.GetMenuWithDrinksAndIngredients());

            Assert.That(ex!.Message, Is.EqualTo("No active event"));
        }

        [Test]
        public void GetMenuWithDrinksAndIngredients_Throws_WhenMenuNotFound()
        {
            var eventId = Guid.NewGuid();

            _eventSessionMock.Setup(e => e.HasActiveEvent).Returns(true);
            _eventSessionMock.Setup(e => e.CurrentEventId).Returns(eventId);

            _menuRepoMock
                .Setup(r => r.GetMenuWithDrinksAndIngredientsByEventId(eventId))
                .Returns((Menu?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _logic.GetMenuWithDrinksAndIngredients());

            Assert.That(ex!.Message, Is.EqualTo("Menu not found for active event"));
        }

        [Test]
        public void GetMenuWithDrinksAndIngredients_ReturnsDrinks()
        {
            var eventId = Guid.NewGuid();
            var drink = new Drink { DrinkId = Guid.NewGuid() };

            var menu = new Menu
            {
                MenuContents = new List<MenuContent>
                {
                    new MenuContent { Drink = drink }
                }
            };

            _eventSessionMock.Setup(e => e.HasActiveEvent).Returns(true);
            _eventSessionMock.Setup(e => e.CurrentEventId).Returns(eventId);

            _menuRepoMock
                .Setup(r => r.GetMenuWithDrinksAndIngredientsByEventId(eventId))
                .Returns(menu);

            var result = _logic.GetMenuWithDrinksAndIngredients().ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].DrinkId, Is.EqualTo(drink.DrinkId));
        }
    }
}
