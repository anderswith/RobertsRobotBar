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
        public void AddIngredientUseCount_Throws_WhenIngredientIdIsEmpty()
        {
            var ingredientId = Guid.Empty;
            var eventSessionId = Guid.NewGuid();
            Assert.Throws<ArgumentException>(() =>
                _logic.AddIngredientUseCount(ingredientId, eventSessionId));
        }

        [Test]
        public void AddIngredientUseCount_CreatesCorrectEntity_AndCallsRepository()
        {
            // Arrange
            Guid ingredientId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();

            IngredientUseCount? captured = null;

            _repoMock
                .Setup(r => r.AddIngredientUseCount(It.IsAny<IngredientUseCount>()))
                .Callback<IngredientUseCount>(iuc => captured = iuc);

            var logic = new IngredientUseCountLogic(_repoMock.Object);

            // Act
            logic.AddIngredientUseCount(ingredientId, eventId);

            // Assert
            _repoMock.Verify(r => r.AddIngredientUseCount(It.IsAny<IngredientUseCount>()), Times.Once);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.IngredientId, Is.EqualTo(ingredientId));
            Assert.That(captured.EventId, Is.EqualTo(eventId));          // NEW ASSERT
            Assert.That(captured.UseCountId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(captured.TimeStamp, Is.Not.EqualTo(default(DateTime)));
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

        // GetAllIngredientsUseCountForEvent
        [Test]
        public void GetAllIngredientsUseCountForEvent_Throws_WhenEventIdIsEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.GetAllIngredientsUseCountForEvent(Guid.Empty));
        }

        [Test]
        public void GetAllIngredientsUseCountForEvent_ReturnsGroupedCounts()
        {
            Guid eventId = Guid.NewGuid();
            Guid ingA = Guid.NewGuid();
            Guid ingB = Guid.NewGuid();

            var ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientId = ingA, Name = "Vodka" },
                new Ingredient { IngredientId = ingB, Name = "Lime" }
            };

            var uses = new List<IngredientUseCount>
            {
                new IngredientUseCount { IngredientId = ingA },
                new IngredientUseCount { IngredientId = ingA },
                new IngredientUseCount { IngredientId = ingB }
            };

            _repoMock.Setup(r => r.GetIngredientUseCountForEvent(eventId))
                .Returns((ingredients, uses));

            var result = _logic.GetAllIngredientsUseCountForEvent(eventId).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].IngredientName, Is.EqualTo("Vodka"));
            Assert.That(result[0].TotalUseCount, Is.EqualTo(2));
            Assert.That(result[1].IngredientName, Is.EqualTo("Lime"));
            Assert.That(result[1].TotalUseCount, Is.EqualTo(1));
        }

        // GetIngredientUseCountByTimeFrame
        [Test]
        public void GetIngredientUseCountByTimeFrame_Throws_WhenEventIdIsEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.GetIngredientUseCountByTimeFrame(Guid.Empty, DateTime.Now, DateTime.Now.AddHours(1)));
        }

        [Test]
        public void GetIngredientUseCountByTimeFrame_Throws_WhenStartIsAfterOrEqualEnd()
        {
            Guid eventId = Guid.NewGuid();
            var t = DateTime.Now;

            Assert.Throws<ArgumentException>(() =>
                _logic.GetIngredientUseCountByTimeFrame(eventId, t, t));
        }

        [Test]
        public void GetIngredientUseCountByTimeFrame_Throws_WhenDatesAreDefault()
        {
            Guid eventId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() =>
                _logic.GetIngredientUseCountByTimeFrame(eventId, default, DateTime.Now));
        }

        [Test]
        public void GetIngredientUseCountByTimeFrame_ReturnsFilteredGroupedCounts()
        {
            Guid eventId = Guid.NewGuid();
            Guid ingA = Guid.NewGuid();
            Guid ingB = Guid.NewGuid();

            var ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientId = ingA, Name = "Vodka" },
                new Ingredient { IngredientId = ingB, Name = "Lime" }
            };

            var now = DateTime.Now;

            var uses = new List<IngredientUseCount>
            {
                new IngredientUseCount { IngredientId = ingA, TimeStamp = now.AddMinutes(-20) },
                new IngredientUseCount { IngredientId = ingA, TimeStamp = now.AddMinutes(-30) },
                new IngredientUseCount { IngredientId = ingB, TimeStamp = now.AddHours(-3) } // outside range
            };

            _repoMock.Setup(r => r.GetIngredientUseCountForEvent(eventId))
                .Returns((ingredients, uses));

            var result = _logic
                .GetIngredientUseCountByTimeFrame(eventId, now.AddHours(-1), now)
                .ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].IngredientName, Is.EqualTo("Vodka"));
            Assert.That(result[0].TotalUseCount, Is.EqualTo(2));
        }
    }
}
