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
    public class LogLogicTests
    {
        private Mock<ILogRepository> _repoMock;
        private Mock<IEventSessionService> _eventSessionMock;
        private LogLogic _logic;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<ILogRepository>();
            _eventSessionMock = new Mock<IEventSessionService>();

            _logic = new LogLogic(_repoMock.Object, _eventSessionMock.Object);
        }

        //AddLog

        [TestCase(null)]
        [TestCase("")]
        public void AddLog_Throws_WhenMessageIsInvalid(string? invalidMsg)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddLog(invalidMsg!, "Info"));

            Assert.That(ex!.Message, Is.EqualTo("Log message cannot be null or empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddLog_Throws_WhenTypeIsInvalid(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddLog("Hello", invalidType!));

            Assert.That(ex!.Message, Is.EqualTo("Log type cannot be null or empty"));
        }

        [Test]
        public void AddLog_CreatesLogAndCallsRepository()
        {
            _logic.AddLog("Started", "Info");

            _repoMock.Verify(r =>
                r.AddLog(It.Is<Log>(l =>
                    l.LogMsg == "Started" &&
                    l.Type == "Info" &&
                    l.LogId != Guid.Empty &&
                    l.EventId == null
                )),
                Times.Once);
        }

        //AddEventLog

        [Test]
        public void AddEventLog_Throws_WhenNoActiveEvent()
        {
            _eventSessionMock.Setup(e => e.CurrentEventId).Returns(Guid.Empty);

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddEventLog("Event started", "Info"));

            Assert.That(ex!.Message, Is.EqualTo("Event ID must be specified"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddEventLog_Throws_WhenMessageInvalid(string? invalidMsg)
        {
            _eventSessionMock.Setup(e => e.CurrentEventId).Returns(Guid.NewGuid());

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddEventLog(invalidMsg!, "Info"));

            Assert.That(ex!.Message, Is.EqualTo("Log message cannot be null or empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddEventLog_Throws_WhenTypeInvalid(string? invalidType)
        {
            _eventSessionMock.Setup(e => e.CurrentEventId).Returns(Guid.NewGuid());

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddEventLog("Valid message", invalidType!));

            Assert.That(ex!.Message, Is.EqualTo("Log type cannot be null or empty"));
        }

        [Test]
        public void AddEventLog_CreatesLogWithEventId_AndCallsRepository()
        {
            var eventId = Guid.NewGuid();
            _eventSessionMock.Setup(e => e.CurrentEventId).Returns(eventId);

            Log? captured = null;
            _repoMock
                .Setup(r => r.AddLog(It.IsAny<Log>()))
                .Callback<Log>(l => captured = l);

            _logic.AddEventLog("Event started", "Info");

            _repoMock.Verify(r => r.AddLog(It.IsAny<Log>()), Times.Once);
            Assert.That(captured, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(captured!.EventId, Is.EqualTo(eventId));
                Assert.That(captured.LogMsg, Is.EqualTo("Event started"));
                Assert.That(captured.Type, Is.EqualTo("Info"));
                Assert.That(captured.LogId, Is.Not.EqualTo(Guid.Empty));
            });
        }

        //getAllCommunicationLogs

        [Test]
        public void GetAllCommunicationLogs_ReturnsOnlyLogsWithoutEventId()
        {
            var logs = new List<Log>
            {
                new Log { LogMsg = "Comm", EventId = null },
                new Log { LogMsg = "Event", EventId = Guid.NewGuid() }
            };

            _repoMock.Setup(r => r.GetAllLogs()).Returns(logs);

            var result = _logic.getAllCommunicationLogs().ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].EventId, Is.Null);
        }

        //GetCommunicationLogsInTimeFrame

        [Test]
        public void GetCommunicationLogsInTimeFrame_Throws_WhenDatesInvalid()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetCommunicationLogsInTimeFrame(default, DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("Start and end times must be specified"));
        }

        [Test]
        public void GetCommunicationLogsInTimeFrame_Throws_WhenStartAfterEnd()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetCommunicationLogsInTimeFrame(DateTime.Now, DateTime.Now.AddHours(-1)));

            Assert.That(ex!.Message, Is.EqualTo("Start time must be earlier than end time"));
        }

        [Test]
        public void GetCommunicationLogsInTimeFrame_CallsRepository()
        {
            var start = DateTime.Now.AddHours(-1);
            var end = DateTime.Now;
            var logs = new List<Log> { new Log { LogMsg = "Comm" } };

            _repoMock
                .Setup(r => r.GetCommunicationLogsInTimeFrame(start, end))
                .Returns(logs);

            var result = _logic.GetCommunicationLogsInTimeFrame(start, end);

            Assert.That(result, Is.EqualTo(logs));
            _repoMock.Verify(r => r.GetCommunicationLogsInTimeFrame(start, end), Times.Once);
        }

        //GetLogsInTimeFrame

        [Test]
        public void GetLogsInTimeFrame_Throws_WhenEventIdEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsInTimeFrame(Guid.Empty, DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("Event ID must be specified"));
        }

        [Test]
        public void GetLogsInTimeFrame_ReturnsRepositoryValue()
        {
            var eventId = Guid.NewGuid();
            var start = DateTime.Now.AddHours(-1);
            var end = DateTime.Now;
            var logs = new List<Log> { new Log { LogMsg = "EventLog" } };

            _repoMock
                .Setup(r => r.GetLogsInTimeFrame(eventId, start, end))
                .Returns(logs);

            var result = _logic.GetLogsInTimeFrame(eventId, start, end);

            Assert.That(result, Is.EqualTo(logs));
        }

        //GetLogsForEvent 

        [Test]
        public void GetLogsForEvent_Throws_WhenEventIdEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsForEvent(Guid.Empty));

            Assert.That(ex!.Message, Is.EqualTo("Event ID must be specified"));
        }

        [Test]
        public void GetLogsForEvent_ReturnsRepositoryValue()
        {
            var eventId = Guid.NewGuid();
            var logs = new List<Log> { new Log { LogMsg = "Hello" } };

            _repoMock.Setup(r => r.GetLogsForEvent(eventId)).Returns(logs);

            var result = _logic.GetLogsForEvent(eventId);

            Assert.That(result, Is.EqualTo(logs));
        }
    }
}
