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

        // ---------- AddIngredientUseCount ----------

        [Test]
        public void AddIngredientUseCount_Throws_WhenIngredientIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddIngredientUseCount(Guid.Empty, Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient ID must be a valid GUID."));
        }

        [Test]
        public void AddIngredientUseCount_CreatesEntity_AndCallsRepository()
        {
            var ingredientId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            IngredientUseCount? captured = null;

            _repoMock
                .Setup(r => r.AddIngredientUseCount(It.IsAny<IngredientUseCount>()))
                .Callback<IngredientUseCount>(iuc => captured = iuc);

            _logic.AddIngredientUseCount(ingredientId, eventId);

            _repoMock.Verify(
                r => r.AddIngredientUseCount(It.IsAny<IngredientUseCount>()),
                Times.Once);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.IngredientId, Is.EqualTo(ingredientId));
                Assert.That(captured.EventId, Is.EqualTo(eventId));
                Assert.That(captured.UseCountId, Is.Not.EqualTo(Guid.Empty));
                Assert.That(captured.TimeStamp, Is.Not.EqualTo(default(DateTime)));
            });
        }

        // ---------- GetAllIngredientsUseCountForEvent ----------

        [Test]
        public void GetAllIngredientsUseCountForEvent_Throws_WhenEventIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetAllIngredientsUseCountForEvent(Guid.Empty));

            Assert.That(ex!.Message, Is.EqualTo("Event ID must be a valid GUID."));
        }

        [Test]
        public void GetAllIngredientsUseCountForEvent_ReturnsGroupedAndOrderedCounts()
        {
            var eventId = Guid.NewGuid();
            var ingA = Guid.NewGuid();
            var ingB = Guid.NewGuid();

            var ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientId = ingA, Name = "Vodka" },
                new Ingredient { IngredientId = ingB, Name = "Lime" }
            };

            var uses = new List<IngredientUseCount>
            {
                new IngredientUseCount { IngredientId = ingB },
                new IngredientUseCount { IngredientId = ingA },
                new IngredientUseCount { IngredientId = ingA }
            };

            _repoMock
                .Setup(r => r.GetIngredientUseCountForEvent(eventId))
                .Returns((ingredients, uses));

            var result = _logic
                .GetAllIngredientsUseCountForEvent(eventId)
                .ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].IngredientName, Is.EqualTo("Vodka"));
                Assert.That(result[0].TotalUseCount, Is.EqualTo(2));
                Assert.That(result[1].IngredientName, Is.EqualTo("Lime"));
                Assert.That(result[1].TotalUseCount, Is.EqualTo(1));
            });
        }

        // ---------- GetIngredientUseCountByTimeFrame ----------

        [Test]
        public void GetIngredientUseCountByTimeFrame_Throws_WhenEventIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetIngredientUseCountByTimeFrame(
                    Guid.Empty, DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("Event ID must be a valid GUID."));
        }

        [Test]
        public void GetIngredientUseCountByTimeFrame_Throws_WhenStartIsAfterOrEqualEnd()
        {
            var eventId = Guid.NewGuid();
            var t = DateTime.Now;

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetIngredientUseCountByTimeFrame(eventId, t, t));

            Assert.That(ex!.Message, Is.EqualTo("Start time must be earlier than end time."));
        }

        [Test]
        public void GetIngredientUseCountByTimeFrame_Throws_WhenDatesAreDefault()
        {
            var eventId = Guid.NewGuid();

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetIngredientUseCountByTimeFrame(
                    eventId, default, DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("Start time and end time must be valid dates."));
        }

        [Test]
        public void GetIngredientUseCountByTimeFrame_ReturnsEmpty_WhenRepositoryReturnsNull()
        {
            var eventId = Guid.NewGuid();

            _repoMock
                .Setup(r => r.GetIngredientUseCountForEvent(eventId))
                .Returns((null, null));

            var result = _logic.GetIngredientUseCountByTimeFrame(
                eventId, DateTime.Now.AddHours(-1), DateTime.Now);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetIngredientUseCountByTimeFrame_ReturnsFilteredGroupedCounts()
        {
            var eventId = Guid.NewGuid();
            var ingA = Guid.NewGuid();
            var ingB = Guid.NewGuid();
            var now = DateTime.Now;

            var ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientId = ingA, Name = "Vodka" },
                new Ingredient { IngredientId = ingB, Name = "Lime" }
            };

            var uses = new List<IngredientUseCount>
            {
                new IngredientUseCount { IngredientId = ingA, TimeStamp = now.AddMinutes(-20) },
                new IngredientUseCount { IngredientId = ingA, TimeStamp = now.AddMinutes(-30) },
                new IngredientUseCount { IngredientId = ingB, TimeStamp = now.AddHours(-3) }
            };

            _repoMock
                .Setup(r => r.GetIngredientUseCountForEvent(eventId))
                .Returns((ingredients, uses));

            var result = _logic
                .GetIngredientUseCountByTimeFrame(eventId, now.AddHours(-1), now)
                .ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].IngredientName, Is.EqualTo("Vodka"));
                Assert.That(result[0].TotalUseCount, Is.EqualTo(2));
            });
        }
    }
}
