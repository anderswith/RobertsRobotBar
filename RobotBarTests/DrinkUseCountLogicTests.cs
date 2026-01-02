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
    public class DrinkUseCountLogicTests
    {
        private Mock<IDrinkUseCountRepository> _repoMock;
        private DrinkUseCountLogic _logic;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IDrinkUseCountRepository>();
            _logic = new DrinkUseCountLogic(_repoMock.Object);
        }

        // ---------- AddDrinkUseCount ----------

        [Test]
        public void AddDrinkUseCount_CreatesCorrectEntity_AndCallsRepository()
        {
            var drinkId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            DrinkUseCount? captured = null;

            _repoMock
                .Setup(r => r.AddDrinkUseCount(It.IsAny<DrinkUseCount>()))
                .Callback<DrinkUseCount>(duc => captured = duc);

            _logic.AddDrinkUseCount(drinkId, eventId);

            _repoMock.Verify(r => r.AddDrinkUseCount(It.IsAny<DrinkUseCount>()), Times.Once);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.DrinkId, Is.EqualTo(drinkId));
                Assert.That(captured.EventId, Is.EqualTo(eventId));
                Assert.That(captured.UseCountId, Is.Not.EqualTo(Guid.Empty));
                Assert.That(captured.TimeStamp, Is.Not.EqualTo(default(DateTime)));
            });
        }

        // ---------- GetAllDrinksUseCountForEvent ----------

        [Test]
        public void GetAllDrinksUseCountForEvent_Throws_WhenEventIdIsEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.GetAllDrinksUseCountForEvent(Guid.Empty));
        }

        [Test]
        public void GetAllDrinksUseCountForEvent_ReturnsEmpty_WhenNoDrinks()
        {
            var eventId = Guid.NewGuid();

            _repoMock
                .Setup(r => r.GetAllDrinksUseCountForEvent(eventId))
                .Returns((new List<Drink>(), new List<DrinkUseCount>()));

            var result = _logic.GetAllDrinksUseCountForEvent(eventId);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAllDrinksUseCountForEvent_ReturnsGroupedAndOrderedCounts()
        {
            var eventId = Guid.NewGuid();
            var drinkA = Guid.NewGuid();
            var drinkB = Guid.NewGuid();

            var drinks = new List<Drink>
            {
                new Drink { DrinkId = drinkA, Name = "Mojito" },
                new Drink { DrinkId = drinkB, Name = "Cola" }
            };

            var uses = new List<DrinkUseCount>
            {
                new DrinkUseCount { DrinkId = drinkA },
                new DrinkUseCount { DrinkId = drinkA },
                new DrinkUseCount { DrinkId = drinkB }
            };

            _repoMock
                .Setup(r => r.GetAllDrinksUseCountForEvent(eventId))
                .Returns((drinks, uses));

            var result = _logic.GetAllDrinksUseCountForEvent(eventId).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].DrinkName, Is.EqualTo("Mojito"));
                Assert.That(result[0].TotalUseCount, Is.EqualTo(2));
                Assert.That(result[1].DrinkName, Is.EqualTo("Cola"));
                Assert.That(result[1].TotalUseCount, Is.EqualTo(1));
            });
        }

        // ---------- GetAllDrinkUseCountByTimeFrame ----------

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_Throws_WhenEventIdIsEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.GetAllDrinkUseCountByTimeFrame(
                    Guid.Empty, DateTime.Now.AddHours(-1), DateTime.Now));
        }

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_Throws_WhenStartIsAfterOrEqualToEnd()
        {
            var eventId = Guid.NewGuid();
            var time = DateTime.Now;

            Assert.Throws<ArgumentException>(() =>
                _logic.GetAllDrinkUseCountByTimeFrame(eventId, time, time));
        }

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_Throws_WhenDatesAreDefault()
        {
            var eventId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() =>
                _logic.GetAllDrinkUseCountByTimeFrame(
                    eventId, default, DateTime.Now));
        }

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_ReturnsEmpty_WhenNoResultsInTimeFrame()
        {
            var eventId = Guid.NewGuid();
            var now = DateTime.Now;

            var drinks = new List<Drink>
            {
                new Drink { DrinkId = Guid.NewGuid(), Name = "Mojito" }
            };

            var uses = new List<DrinkUseCount>
            {
                new DrinkUseCount
                {
                    DrinkId = drinks[0].DrinkId,
                    TimeStamp = now.AddHours(-5)
                }
            };

            _repoMock
                .Setup(r => r.GetAllDrinksUseCountForEvent(eventId))
                .Returns((drinks, uses));

            var result = _logic.GetAllDrinkUseCountByTimeFrame(
                eventId, now.AddHours(-1), now);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_ReturnsFilteredGroupedCounts()
        {
            var eventId = Guid.NewGuid();
            var drinkA = Guid.NewGuid();
            var drinkB = Guid.NewGuid();
            var now = DateTime.Now;

            var drinks = new List<Drink>
            {
                new Drink { DrinkId = drinkA, Name = "Mojito" },
                new Drink { DrinkId = drinkB, Name = "Cola" }
            };

            var uses = new List<DrinkUseCount>
            {
                new DrinkUseCount { DrinkId = drinkA, TimeStamp = now.AddMinutes(-30) },
                new DrinkUseCount { DrinkId = drinkA, TimeStamp = now.AddMinutes(-20) },
                new DrinkUseCount { DrinkId = drinkB, TimeStamp = now.AddHours(-3) }
            };

            _repoMock
                .Setup(r => r.GetAllDrinksUseCountForEvent(eventId))
                .Returns((drinks, uses));

            var result = _logic
                .GetAllDrinkUseCountByTimeFrame(eventId, now.AddHours(-1), now)
                .ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].DrinkName, Is.EqualTo("Mojito"));
                Assert.That(result[0].TotalUseCount, Is.EqualTo(2));
            });
        }
    }
}
