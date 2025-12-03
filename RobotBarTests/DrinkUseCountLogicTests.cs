using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.BLL.Interfaces;
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
        
        // AddDrinkUseCount

        [Test]
        public void AddDrinkUseCount_CreatesCorrectEntity_AndCallsRepository()
        {
            Guid drinkId = Guid.NewGuid();

            DrinkUseCount? captured = null;

            _repoMock
                .Setup(r => r.AddDrinkUseCount(It.IsAny<DrinkUseCount>()))
                .Callback<DrinkUseCount>(duc => captured = duc);

            _logic.AddDrinkUseCount(drinkId);

            _repoMock.Verify(r => r.AddDrinkUseCount(It.IsAny<DrinkUseCount>()), Times.Once);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.DrinkId, Is.EqualTo(drinkId));
            Assert.That(captured.UseCountId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(captured.TimeStamp, Is.Not.EqualTo(default(DateTime)));
        }

        // GetAllDrinkUseCounts
        [Test]
        public void GetAllDrinkUseCounts_ReturnsRepositoryValue()
        {
            var fakeList = new List<DrinkUseCount>
            {
                new DrinkUseCount { DrinkId = Guid.NewGuid() }
            };

            _repoMock.Setup(r => r.GetAllDrinkUseCounts()).Returns(fakeList);

            var result = _logic.GetAllDrinkUseCounts();

            Assert.That(result, Is.EqualTo(fakeList));
        }


        // Validation tests for GetAllDrinkUseCountByTimeFrame

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_Throws_WhenStartIsAfterOrEqualToEnd()
        {
            DateTime start = DateTime.Now;
            DateTime end = start; // same time

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetAllDrinkUseCountByTimeFrame(start, end));

            Assert.That(ex!.Message, Is.EqualTo("Start time must be earlier than end time."));
        }

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_Throws_WhenDatesAreDefault()
        {
            DateTime start = default;
            DateTime end = DateTime.Now;

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetAllDrinkUseCountByTimeFrame(start, end));

            Assert.That(ex!.Message, Is.EqualTo("Start time and end time must be valid dates."));
        }

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_Throws_WhenRepositoryReturnsNull()
        {
            _repoMock
                .Setup(r => r.GetAllDrinkUseCountByTimeFrame(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((IEnumerable<DrinkUseCount>)null!);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _logic.GetAllDrinkUseCountByTimeFrame(DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("No drink use counts found in the specified time frame."));
        }

        [Test]
        public void GetAllDrinkUseCountByTimeFrame_Throws_WhenRepositoryReturnsEmpty()
        {
            _repoMock
                .Setup(r => r.GetAllDrinkUseCountByTimeFrame(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<DrinkUseCount>());

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _logic.GetAllDrinkUseCountByTimeFrame(DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("No drink use counts found in the specified time frame."));
        }
        
        // Grouping tests for GetAllDrinkUseCountByTimeFrame
        [Test]
        public void GetAllDrinkUseCountByTimeFrame_GroupsCorrectly_AndReturnsOrderedResults()
        {
            Guid drink1 = Guid.NewGuid();
            Guid drink2 = Guid.NewGuid();

            var input = new List<DrinkUseCount>
            {
                new DrinkUseCount
                {
                    DrinkId = drink1,
                    drink = new Drink { Name = "Mojito" }
                },
                new DrinkUseCount
                {
                    DrinkId = drink1,
                    drink = new Drink { Name = "Mojito" }
                },
                new DrinkUseCount
                {
                    DrinkId = drink2,
                    drink = new Drink { Name = "Cola" }
                }
            };

            _repoMock
                .Setup(r => r.GetAllDrinkUseCountByTimeFrame(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(input);

            var result = _logic
                .GetAllDrinkUseCountByTimeFrame(DateTime.Now.AddHours(-1), DateTime.Now)
                .ToList();

            Assert.That(result.Count, Is.EqualTo(2));

            // Highest count first
            Assert.That(result[0].DrinkName, Is.EqualTo("Mojito"));
            Assert.That(result[0].TotalUseCount, Is.EqualTo(2));

            Assert.That(result[1].DrinkName, Is.EqualTo("Cola"));
            Assert.That(result[1].TotalUseCount, Is.EqualTo(1));
        }
    }
}
