using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace UnitTests
{
    [TestFixture]
    public class BarSetupLogicTests
    {
        private Mock<IBarSetupRepository> _barSetupRepositoryMock;
        private BarSetupLogic _barSetupLogic;

        [SetUp]
        public void Setup()
        {
            _barSetupRepositoryMock = new Mock<IBarSetupRepository>();
            _barSetupLogic = new BarSetupLogic(_barSetupRepositoryMock.Object);
        }

        // ---------- AddBarSetup ----------

        [TestCase(0)]
        [TestCase(-1)]
        public void AddBarSetup_ShouldThrow_WhenPositionIsZeroOrNegative(int invalidPos)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _barSetupLogic.AddBarSetup(invalidPos, Guid.NewGuid(), Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("Position number must be greater than zero."));
        }

        [Test]
        public void AddBarSetup_ShouldThrow_WhenIngredientIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _barSetupLogic.AddBarSetup(1, Guid.Empty, Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("Ingredient ID cannot be empty."));
        }

        [Test]
        public void AddBarSetup_ShouldThrow_WhenEventIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _barSetupLogic.AddBarSetup(1, Guid.NewGuid(), Guid.Empty));

            Assert.That(ex.Message, Is.EqualTo("Event ID cannot be empty."));
        }

        [Test]
        public void AddBarSetup_ShouldUpdate_WhenExistingSetupExists()
        {
            var eventId = Guid.NewGuid();
            var existing = new BarSetup
            {
                EventBarSetupId = Guid.NewGuid(),
                PositionNumber = 1,
                IngredientId = Guid.NewGuid(),
                EventId = eventId
            };

            _barSetupRepositoryMock
                .Setup(r => r.GetBarSetupEventAndPosition(eventId, 1))
                .Returns(existing);

            var newIngredientId = Guid.NewGuid();

            _barSetupLogic.AddBarSetup(1, newIngredientId, eventId);

            Assert.That(existing.IngredientId, Is.EqualTo(newIngredientId));

            _barSetupRepositoryMock.Verify(
                r => r.updateBarSetup(existing),
                Times.Once);

            _barSetupRepositoryMock.Verify(
                r => r.addBarSetup(It.IsAny<BarSetup>()),
                Times.Never);
        }

        [Test]
        public void AddBarSetup_ShouldAdd_WhenNoExistingSetup()
        {
            var eventId = Guid.NewGuid();
            var ingredientId = Guid.NewGuid();

            _barSetupRepositoryMock
                .Setup(r => r.GetBarSetupEventAndPosition(eventId, 1))
                .Returns((BarSetup?)null);

            _barSetupLogic.AddBarSetup(1, ingredientId, eventId);

            _barSetupRepositoryMock.Verify(r =>
                r.addBarSetup(It.Is<BarSetup>(b =>
                    b.PositionNumber == 1 &&
                    b.EventId == eventId &&
                    b.IngredientId == ingredientId
                )),
                Times.Once);

            _barSetupRepositoryMock.Verify(
                r => r.updateBarSetup(It.IsAny<BarSetup>()),
                Times.Never);
        }

        // ---------- DeleteBarSetup ----------

        [Test]
        public void DeleteBarSetup_ShouldThrow_WhenSetupNotFound()
        {
            _barSetupRepositoryMock
                .Setup(r => r.GetBarSetupEventAndPosition(It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns((BarSetup?)null);

            var ex = Assert.Throws<ArgumentException>(() =>
                _barSetupLogic.DeleteBarSetup(Guid.NewGuid(), 1));

            Assert.That(ex.Message, Is.EqualTo("No bar setup found for the given event and position."));
        }

        [Test]
        public void DeleteBarSetup_ShouldCallRepository_WhenSetupExists()
        {
            var setup = new BarSetup { EventId = Guid.NewGuid(), PositionNumber = 1 };

            _barSetupRepositoryMock
                .Setup(r => r.GetBarSetupEventAndPosition(setup.EventId, 1))
                .Returns(setup);

            _barSetupLogic.DeleteBarSetup(setup.EventId, 1);

            _barSetupRepositoryMock.Verify(r => r.deleteBarSetup(setup), Times.Once);
        }

        // ---------- GetBarSetups----------

        [Test]
        public void GetBarSetupsForEvent_ShouldThrow_WhenEventIdEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _barSetupLogic.GetBarSetupsForEvent(Guid.Empty));

            Assert.That(ex.Message, Is.EqualTo("Event ID cannot be empty."));
        }

        [Test]
        public void GetBarSetupsForEvent_ShouldCallRepository_WhenValid()
        {
            var eventId = Guid.NewGuid();
            var setups = new List<BarSetup>();

            _barSetupRepositoryMock
                .Setup(r => r.GetAllBarSetupsForEventById(eventId))
                .Returns(setups);

            var result = _barSetupLogic.GetBarSetupsForEvent(eventId);

            Assert.That(result, Is.EqualTo(setups));

            _barSetupRepositoryMock.Verify(
                r => r.GetAllBarSetupsForEventById(eventId),
                Times.Once);
        }
    }
}
