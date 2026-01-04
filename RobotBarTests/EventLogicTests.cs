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
    public class EventLogicTests
    {
        private Mock<IEventRepository> _eventRepositoryMock;
        private EventLogic _eventLogic;

        [SetUp]
        public void Setup()
        {
            _eventRepositoryMock = new Mock<IEventRepository>();
            _eventLogic = new EventLogic(_eventRepositoryMock.Object);
        }

        //AddEvent

        [TestCase(null)]
        [TestCase("")]
        public void AddEvent_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.AddEvent(invalidName!, "image.png"));

            Assert.That(ex!.Message, Is.EqualTo("Event name cannot be null or empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddEvent_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.AddEvent("Event Name", invalidImage!));

            Assert.That(ex!.Message, Is.EqualTo("Event image URL cannot be null or empty"));
        }

        [Test]
        public void AddEvent_ShouldCreateEventWithMenu_AndCallRepository()
        {
            Event? captured = null;

            _eventRepositoryMock
                .Setup(r => r.AddEvent(It.IsAny<Event>()))
                .Callback<Event>(e => captured = e);

            var resultId = _eventLogic.AddEvent("New Event", "image.png");

            _eventRepositoryMock.Verify(r =>
                r.AddEvent(It.IsAny<Event>()), Times.Once);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(resultId, Is.EqualTo(captured!.EventId));
                Assert.That(captured.Name, Is.EqualTo("New Event"));
                Assert.That(captured.Image, Is.EqualTo("image.png"));
                Assert.That(captured.Menu, Is.Not.Null);
                Assert.That(captured.Menu!.MenuId, Is.Not.EqualTo(Guid.Empty));
                Assert.That(captured.Menu.Name, Is.EqualTo("New Event Menu"));
            });
        }

        //GetAllEvents

        [Test]
        public void GetAllEvents_ShouldReturnRepositoryResult()
        {
            var events = new List<Event>
            {
                new Event { Name = "Event 1" },
                new Event { Name = "Event 2" }
            };

            _eventRepositoryMock
                .Setup(r => r.GetAllEvents())
                .Returns(events);

            var result = _eventLogic.GetAllEvents();

            Assert.That(result, Is.EqualTo(events));
        }

        //GetEventById 

        [Test]
        public void GetEventById_ShouldThrow_WhenIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.GetEventById(Guid.Empty));

            Assert.That(ex!.Message, Is.EqualTo("Event ID cannot be empty"));
        }

        [Test]
        public void GetEventById_ShouldReturnEvent_WhenItExists()
        {
            var eventId = Guid.NewGuid();
            var evt = new Event { EventId = eventId };

            _eventRepositoryMock
                .Setup(r => r.GetEventById(eventId))
                .Returns(evt);

            var result = _eventLogic.GetEventById(eventId);

            Assert.That(result, Is.EqualTo(evt));
        }

        // DeleteEvent

        [Test]
        public void DeleteEvent_ShouldThrow_WhenIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.DeleteEvent(Guid.Empty));

            Assert.That(ex!.Message, Is.EqualTo("Event ID cannot be empty"));
        }

        [Test]
        public void DeleteEvent_ShouldThrow_WhenEventNotFound()
        {
            var eventId = Guid.NewGuid();

            _eventRepositoryMock
                .Setup(r => r.GetEventById(eventId))
                .Returns((Event?)null);

            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.DeleteEvent(eventId));

            Assert.That(ex!.Message, Is.EqualTo("Event not found"));
        }

        [Test]
        public void DeleteEvent_ShouldCallRepository_WhenEventExists()
        {
            var evt = new Event { EventId = Guid.NewGuid() };

            _eventRepositoryMock
                .Setup(r => r.GetEventById(evt.EventId))
                .Returns(evt);

            _eventLogic.DeleteEvent(evt.EventId);

            _eventRepositoryMock.Verify(
                r => r.DeleteEvent(evt),
                Times.Once);
        }

        //UpdateEvent 

        [Test]
        public void UpdateEvent_ShouldThrow_WhenIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.UpdateEvent(Guid.Empty, "Name", "image.png", Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Event ID cannot be empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateEvent_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.UpdateEvent(Guid.NewGuid(), invalidName!, "image.png", Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Event name cannot be null or empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateEvent_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.UpdateEvent(Guid.NewGuid(), "Event Name", invalidImage!, Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Event image URL cannot be null or empty"));
        }

        [Test]
        public void UpdateEvent_ShouldThrow_WhenMenuIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.UpdateEvent(Guid.NewGuid(), "Event Name", "image.png", Guid.Empty));

            Assert.That(ex!.Message, Is.EqualTo("Menu ID cannot be empty"));
        }

        [Test]
        public void UpdateEvent_ShouldThrow_WhenEventNotFound()
        {
            var eventId = Guid.NewGuid();

            _eventRepositoryMock
                .Setup(r => r.GetEventById(eventId))
                .Returns((Event?)null);

            var ex = Assert.Throws<ArgumentException>(() =>
                _eventLogic.UpdateEvent(eventId, "Name", "image.png", Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Event not found"));
        }

        [Test]
        public void UpdateEvent_ShouldUpdateFields_AndCallRepository()
        {
            var eventId = Guid.NewGuid();
            var evt = new Event
            {
                EventId = eventId,
                Name = "Old Name",
                Image = "old.png",
                MenuId = Guid.NewGuid()
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventById(eventId))
                .Returns(evt);

            var newMenuId = Guid.NewGuid();

            _eventLogic.UpdateEvent(eventId, "New Name", "new.png", newMenuId);

            _eventRepositoryMock.Verify(r =>
                r.UpdateEvent(It.Is<Event>(e =>
                    e.EventId == eventId &&
                    e.Name == "New Name" &&
                    e.Image == "new.png" &&
                    e.MenuId == newMenuId
                )),
                Times.Once);
        }

        //GetEventIdForDrink

        [Test]
        public void GetEventIdForDrink_DelegatesToRepository()
        {
            var drinkId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            _eventRepositoryMock
                .Setup(r => r.GetEventIdByDrinkId(drinkId))
                .Returns(eventId);

            var result = _eventLogic.GetEventIdForDrink(drinkId);

            Assert.That(result, Is.EqualTo(eventId));
            _eventRepositoryMock.Verify(
                r => r.GetEventIdByDrinkId(drinkId),
                Times.Once);
        }
    }
}
