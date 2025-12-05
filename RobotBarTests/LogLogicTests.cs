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
    public class LogLogicTests
    {
        private Mock<ILogRepository> _repoMock;
        private LogLogic _logic;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<ILogRepository>();
            _logic = new LogLogic(_repoMock.Object);
        }

        // AddLog
        [TestCase(null)]
        [TestCase("")]
        public void AddLog_Throws_WhenMessageIsInvalid(string? invalidMsg)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddLog(invalidMsg, "Info"));

            Assert.That(ex!.Message, Is.EqualTo("Log message cannot be null or empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddLog_Throws_WhenTypeIsInvalid(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddLog("Hello", invalidType));

            Assert.That(ex!.Message, Is.EqualTo("Log type cannot be null or empty"));
        }

        [Test]
        public void AddLog_CreatesLogAndCallsRepository()
        {
            string message = "Started";
            string type = "Info";

            _logic.AddLog(message, type);

            _repoMock.Verify(r => r.AddLog(It.Is<Log>(l =>
                l.LogMsg == message &&
                l.Type == type &&
                l.LogId != Guid.Empty &&
                l.TimeStamp <= DateTime.Now
            )), Times.Once);
        }

        // GetAllLogs
        [Test]
        public void GetAllLogs_ReturnsRepositoryValue()
        {
            var fakeLogs = new List<Log>
            {
                new Log { LogMsg = "A" },
                new Log { LogMsg = "B" }
            };

            _repoMock.Setup(r => r.GetAllLogs()).Returns(fakeLogs);

            var result = _logic.GetAllLogs();

            Assert.That(result, Is.EqualTo(fakeLogs));
        }

        // GetLogsByType
        [TestCase(null)]
        [TestCase("")]
        public void GetLogsByType_Throws_WhenTypeInvalid(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsByType(invalidType));

            Assert.That(ex!.Message, Is.EqualTo("Log type cannot be null or empty"));
        }

        [Test]
        public void GetLogsByType_ReturnsRepositoryValue()
        {
            var logs = new List<Log> { new Log { Type = "Info" } };
            _repoMock.Setup(r => r.GetLogsByType("Info")).Returns(logs);

            var result = _logic.GetLogsByType("Info");

            Assert.That(result, Is.EqualTo(logs));
        }

        // GetLogsInTimeFrame
        [Test]
        public void GetLogsInTimeFrame_Throws_WhenEventIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsInTimeFrame(Guid.Empty, DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("Event ID must be specified"));
        }

        [Test]
        public void GetLogsInTimeFrame_Throws_WhenStartOrEndDefault()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsInTimeFrame(Guid.NewGuid(), default, DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("Start and end times must be specified"));
        }

        [Test]
        public void GetLogsInTimeFrame_Throws_WhenStartAfterEnd()
        {
            DateTime start = DateTime.Now.AddHours(1);
            DateTime end = DateTime.Now;

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsInTimeFrame(Guid.NewGuid(), start, end));

            Assert.That(ex!.Message, Is.EqualTo("Start time must be earlier than end time"));
        }

        [Test]
        public void GetLogsInTimeFrame_ReturnsRepositoryValue()
        {
            Guid eventId = Guid.NewGuid();
            var start = DateTime.Now.AddHours(-2);
            var end = DateTime.Now;

            var fakeLogs = new List<Log> { new Log { LogMsg = "Test" } };

            _repoMock.Setup(r => r.GetLogsInTimeFrame(eventId, start, end))
                .Returns(fakeLogs);

            var result = _logic.GetLogsInTimeFrame(eventId, start, end);

            Assert.That(result, Is.EqualTo(fakeLogs));
        }

        // GetLogsByTypeInTimeFrame
        [TestCase(null)]
        [TestCase("")]
        public void GetLogsByTypeInTimeFrame_Throws_WhenTypeInvalid(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsByTypeInTimeFrame(Guid.NewGuid(), invalidType, DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("Log type cannot be null or empty"));
        }

        [Test]
        public void GetLogsByTypeInTimeFrame_Throws_WhenEventIdEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsByTypeInTimeFrame(Guid.Empty, "Error", DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex!.Message, Is.EqualTo("Event ID must be specified"));
        }

        [Test]
        public void GetLogsByTypeInTimeFrame_Throws_WhenStartOrEndDefault()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsByTypeInTimeFrame(Guid.NewGuid(), "Info", default, default));

            Assert.That(ex!.Message, Is.EqualTo("Start and end times must be specified"));
        }

        [Test]
        public void GetLogsByTypeInTimeFrame_Throws_WhenStartAfterEnd()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.GetLogsByTypeInTimeFrame(Guid.NewGuid(), "Error", DateTime.Now, DateTime.Now.AddHours(-1)));

            Assert.That(ex!.Message, Is.EqualTo("Start time must be earlier than end time"));
        }

        [Test]
        public void GetLogsByTypeInTimeFrame_ReturnsRepositoryValue()
        {
            Guid eventId = Guid.NewGuid();
            var start = DateTime.Now.AddHours(-5);
            var end = DateTime.Now;

            var fakeLogs = new List<Log> { new Log { Type = "Error" } };

            _repoMock.Setup(r => r.GetLogsByTypeInTimeFrame(eventId, "Error", start, end))
                .Returns(fakeLogs);

            var result = _logic.GetLogsByTypeInTimeFrame(eventId, "Error", start, end);

            Assert.That(result, Is.EqualTo(fakeLogs));
        }

        // GetLogsForEvent
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
            Guid eventId = Guid.NewGuid();
            var logs = new List<Log> { new Log { LogMsg = "Hello" } };

            _repoMock.Setup(r => r.GetLogsForEvent(eventId)).Returns(logs);

            var result = _logic.GetLogsForEvent(eventId);

            Assert.That(result, Is.EqualTo(logs));
        }
    }
}
