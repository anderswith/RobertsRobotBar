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
    public class IngredientUseCountLogicTests
    {
        private Mock<IIngredientUseCountRepository> _repoMock;
        private IngredientUseCountLogic _logic;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IIngredientUseCountRepository>();
            _logic = new IngredientUseCountLogic(_repoMock.Object);
        }
        
        // AddIngredientUseCount

        [Test]
        public void AddIngredientUseCount_CreatesCorrectEntity_AndCallsRepository()
        {
            Guid ingredientId = Guid.NewGuid();

            IngredientUseCount? capturedEntity = null;

            _repoMock
                .Setup(r => r.AddIngredientUseCount(It.IsAny<IngredientUseCount>()))
                .Callback<IngredientUseCount>(iuc => capturedEntity = iuc);
            
            _logic.AddIngredientUseCount(ingredientId);
            
            _repoMock.Verify(r => r.AddIngredientUseCount(It.IsAny<IngredientUseCount>()), Times.Once);

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity!.IngredientId, Is.EqualTo(ingredientId));
            Assert.That(capturedEntity.UseCountId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(capturedEntity.TimeStamp, Is.Not.EqualTo(default(DateTime)));
        }
        
        // GetAllIngredientUseCounts

        [Test]
        public void GetAllIngredientUseCounts_ReturnsRepositoryValue()
        {
            var mockList = new List<IngredientUseCount>
            {
                new IngredientUseCount { IngredientId = Guid.NewGuid() }
            };

            _repoMock.Setup(r => r.GetAllIngredientUseCounts()).Returns(mockList);

            var result = _logic.GetAllIngredientUseCounts();

            Assert.That(result, Is.EqualTo(mockList));
        }

        // GetAllIngredientUseCountByTimeFrame - Validation

        [Test]
        public void GetAllIngredientUseCountByTimeFrame_Throws_WhenStartIsAfterOrEqualEnd()
        {
            DateTime start = DateTime.Now;
            DateTime end = start; // equal

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetAllIngredientUseCountByTimeFrame(start, end));

            Assert.That(ex!.Message, Is.EqualTo("Start time must be earlier than end time."));
        }

        [Test]
        public void GetAllIngredientUseCountByTimeFrame_Throws_WhenDatesAreDefault()
        {
            DateTime start = default;
            DateTime end = DateTime.Now;

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetAllIngredientUseCountByTimeFrame(start, end));

            Assert.That(ex!.Message, Is.EqualTo("Start time and end time must be valid dates."));
        }

        [Test]
        public void GetAllIngredientUseCountByTimeFrame_Throws_WhenRepositoryReturnsNull()
        {
            _repoMock
                .Setup(r => r.GetAllIngredientUseCountByTimeFrame(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((IEnumerable<IngredientUseCount>)null!);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _logic.GetAllIngredientUseCountByTimeFrame(DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("No ingredient use counts found in the specified time frame."));
        }

        [Test]
        public void GetAllIngredientUseCountByTimeFrame_Throws_WhenRepositoryReturnsEmptyList()
        {
            _repoMock
                .Setup(r => r.GetAllIngredientUseCountByTimeFrame(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<IngredientUseCount>());

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _logic.GetAllIngredientUseCountByTimeFrame(DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("No ingredient use counts found in the specified time frame."));
        }
        
        // GetAllIngredientUseCountByTimeFrame - Valid grouping

        [Test]
        public void GetAllIngredientUseCountByTimeFrame_GroupsCorrectly_AndReturnsOrderedResults()
        {
            Guid ing1 = Guid.NewGuid();
            Guid ing2 = Guid.NewGuid();

            var input = new List<IngredientUseCount>
            {
                new IngredientUseCount
                {
                    IngredientId = ing1,
                    Ingredient = new Ingredient { Name = "Vodka" }
                },
                new IngredientUseCount
                {
                    IngredientId = ing1,
                    Ingredient = new Ingredient { Name = "Vodka" }
                },
                new IngredientUseCount
                {
                    IngredientId = ing2,
                    Ingredient = new Ingredient { Name = "Cola" }
                }
            };

            _repoMock
                .Setup(r => r.GetAllIngredientUseCountByTimeFrame(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(input);

            var result = _logic
                .GetAllIngredientUseCountByTimeFrame(DateTime.Now.AddHours(-1), DateTime.Now)
                .ToList();

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].IngredientName, Is.EqualTo("Vodka"));
            Assert.That(result[0].TotalUseCount, Is.EqualTo(2));

            Assert.That(result[1].IngredientName, Is.EqualTo("Cola"));
            Assert.That(result[1].TotalUseCount, Is.EqualTo(1));
        }
    }
}
