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
        private Mock<ILogRepository> _logRepositoryMock;
        private LogLogic _logLogic;

        [SetUp]
        public void Setup()
        {
            _logRepositoryMock = new Mock<ILogRepository>();
            _logLogic = new LogLogic(_logRepositoryMock.Object);
        }

        // ---------- AddLog ----------

        [TestCase(null)]
        [TestCase("")]
        public void AddLog_ShouldThrow_WhenMessageIsNullOrEmpty(string? invalidMsg)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logLogic.AddLog(invalidMsg, "Error"));

            Assert.That(ex.Message, Is.EqualTo("Log message cannot be null or empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddLog_ShouldThrow_WhenTypeIsNullOrEmpty(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logLogic.AddLog("System started", invalidType));

            Assert.That(ex.Message, Is.EqualTo("Log type cannot be null or empty"));
        }

        [Test]
        public void AddLog_ShouldCallRepository_WhenValidData()
        {
            var message = "System started";
            var type = "Info";

            _logLogic.AddLog(message, type);

            _logRepositoryMock.Verify(r => r.AddLog(It.Is<Log>(l =>
                l.LogMsg == message &&
                l.Type == type &&
                l.TimeStamp <= DateTime.Now &&
                l.LogId != Guid.Empty
            )), Times.Once);
        }

        // ---------- GetAllLogs----------

        [Test]
        public void GetAllLogs_ShouldReturnAllLogsFromRepository()
        {
            var logs = new List<Log>
            {
                new Log { LogId = Guid.NewGuid(), Type = "Info", LogMsg = "Startup" },
                new Log { LogId = Guid.NewGuid(), Type = "Error", LogMsg = "Crash" }
            };

            _logRepositoryMock.Setup(r => r.GetAllLogs()).Returns(logs);

            var result = _logLogic.GetAllLogs().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result.Count, Is.EqualTo(2));
                Assert.That(result, Is.EqualTo(logs));
            });
        }

        // ---------- GetLogsByType----------

        [TestCase(null)]
        [TestCase("")]
        public void GetLogsByType_ShouldThrow_WhenTypeIsNullOrEmpty(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logLogic.GetLogsByType(invalidType));

            Assert.That(ex.Message, Is.EqualTo("Log type cannot be null or empty"));
        }

        [Test]
        public void GetLogsByType_ShouldCallRepository_WhenValidType()
        {
            var logs = new List<Log> { new Log { Type = "Info", LogMsg = "Valid" } };
            _logRepositoryMock.Setup(r => r.GetLogsByType("Info")).Returns(logs);

            var result = _logLogic.GetLogsByType("Info");

            Assert.That(result, Is.EqualTo(logs));
            _logRepositoryMock.Verify(r => r.GetLogsByType("Info"), Times.Once);
        }

        // ---------- GetLogsByTimeframe----------

        [Test]
        public void GetLogsInTimeFrame_ShouldThrow_WhenStartAfterEnd()
        {
            var start = DateTime.Now;
            var end = start.AddHours(-1);

            var ex = Assert.Throws<ArgumentException>(() =>
                _logLogic.GetLogsInTimeFrame(start, end));

            Assert.That(ex.Message, Is.EqualTo("Start time must be earlier than end time"));
        }

        [Test]
        public void GetLogsInTimeFrame_ShouldCallRepository_WhenValidDates()
        {
            var start = DateTime.Now.AddHours(-2);
            var end = DateTime.Now;
            var logs = new List<Log> { new Log { LogMsg = "In range" } };

            _logRepositoryMock.Setup(r => r.GetLogsInTimeFrame(start, end)).Returns(logs);

            var result = _logLogic.GetLogsInTimeFrame(start, end);

            Assert.That(result, Is.EqualTo(logs));
            _logRepositoryMock.Verify(r => r.GetLogsInTimeFrame(start, end), Times.Once);
        }

        // (Optional — for DateTime? nullable scenario)
        [Test]
        public void GetLogsInTimeFrame_ShouldThrow_WhenStartOrEndIsDefault()
        {
            // Simulate “null” check logic by passing default(DateTime)
            var ex = Assert.Throws<ArgumentException>(() =>
                _logLogic.GetLogsInTimeFrame(default(DateTime), default(DateTime)));

            Assert.That(ex.Message, Is.EqualTo("Start and end times must be specified"));
        }

        // ----------------------- GetLogByTypeInTimeframe-----------------------

        [TestCase(null)]
        [TestCase("")]
        public void GetLogsByTypeInTimeFrame_ShouldThrow_WhenTypeIsNullOrEmpty(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logLogic.GetLogsByTypeInTimeFrame(invalidType, DateTime.Now.AddHours(-1), DateTime.Now));

            Assert.That(ex.Message, Is.EqualTo("Log type cannot be null or empty"));
        }

        [Test]
        public void GetLogsByTypeInTimeFrame_ShouldThrow_WhenStartAfterEnd()
        {
            var start = DateTime.Now;
            var end = start.AddHours(-1);

            var ex = Assert.Throws<ArgumentException>(() =>
                _logLogic.GetLogsByTypeInTimeFrame("Error", start, end));

            Assert.That(ex.Message, Is.EqualTo("Start time must be earlier than end time"));
        }

        [Test]
        public void GetLogsByTypeInTimeFrame_ShouldThrow_WhenStartOrEndIsDefault()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logLogic.GetLogsByTypeInTimeFrame("Info", default(DateTime), default(DateTime)));

            Assert.That(ex.Message, Is.EqualTo("Start and end times must be specified"));
        }

        [Test]
        public void GetLogsByTypeInTimeFrame_ShouldCallRepository_WhenValidData()
        {
            var start = DateTime.Now.AddHours(-2);
            var end = DateTime.Now;
            var logs = new List<Log> { new Log { Type = "Info", LogMsg = "Valid" } };

            _logRepositoryMock
                .Setup(r => r.GetLogsByTypeInTimeFrame("Info", start, end))
                .Returns(logs);

            var result = _logLogic.GetLogsByTypeInTimeFrame("Info", start, end);

            Assert.That(result, Is.EqualTo(logs));
            _logRepositoryMock.Verify(r => r.GetLogsByTypeInTimeFrame("Info", start, end), Times.Once);
        }
    }
}
