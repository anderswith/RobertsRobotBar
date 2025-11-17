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
    public class SopLogicTests
    {
        private Mock<ISopRepository> _sopRepositoryMock;
        private SopLogic _sopLogic;

        [SetUp]
        public void Setup()
        {
            _sopRepositoryMock = new Mock<ISopRepository>();
            _sopLogic = new SopLogic(_sopRepositoryMock.Object);
        }

        // ---------- AddSop----------

        [TestCase(null)]
        [TestCase("")]
        public void AddSop_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var steps = new List<SopStep> { new SopStep() };

            var ex = Assert.Throws<ArgumentException>(() =>
                _sopLogic.AddSop(invalidName!, "img.png", steps));

            Assert.That(ex.Message, Is.EqualTo("Name cannot be null or empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddSop_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var steps = new List<SopStep> { new SopStep() };

            var ex = Assert.Throws<ArgumentException>(() =>
                _sopLogic.AddSop("Sop Name", invalidImage!, steps));

            Assert.That(ex.Message, Is.EqualTo("Image cannot be null or empty"));
        }

        [Test]
        public void AddSop_ShouldThrow_WhenSopStepsIsNull()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _sopLogic.AddSop("Sop Name", "img.png", null!));

            Assert.That(ex.Message, Is.EqualTo("SopSteps cannot be null or empty"));
        }

        [Test]
        public void AddSop_ShouldThrow_WhenSopStepsIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _sopLogic.AddSop("Sop Name", "img.png", new List<SopStep>()));

            Assert.That(ex.Message, Is.EqualTo("SopSteps cannot be null or empty"));
        }

        [Test]
        public void AddSop_ShouldCallRepository_WhenDataIsValid()
        {
            var steps = new List<SopStep> { new SopStep { Description = "Step 1" } };

            _sopLogic.AddSop("Test SOP", "img.png", steps);

            _sopRepositoryMock.Verify(r => r.AddSop(It.Is<Sop>(s =>
                s.Name == "Test SOP" &&
                s.Image == "img.png" &&
                s.SopSteps == steps
            )), Times.Once);
        }

        // ---------- DeleteSop----------

        [Test]
        public void DeleteSop_ShouldThrow_WhenSopNotFound()
        {
            _sopRepositoryMock.Setup(r => r.GetSopById(It.IsAny<Guid>()))
                .Returns((Sop?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _sopLogic.DeleteSop(Guid.NewGuid()));

            Assert.That(ex.Message, Does.Contain("not found"));
        }

        [Test]
        public void DeleteSop_ShouldCallRepository_WhenSopExists()
        {
            var sop = new Sop { SopId = Guid.NewGuid() };
            _sopRepositoryMock.Setup(r => r.GetSopById(sop.SopId)).Returns(sop);

            _sopLogic.DeleteSop(sop.SopId);

            _sopRepositoryMock.Verify(r => r.DeleteSop(sop), Times.Once);
        }

        // ---------- UpdateSop----------

        [TestCase(null)]
        [TestCase("")]
        public void UpdateSop_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var steps = new List<SopStep> { new SopStep() };

            var ex = Assert.Throws<ArgumentException>(() =>
                _sopLogic.UpdateSop(invalidName!, "img.png", steps, Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("Name cannot be null or empty"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateSop_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var steps = new List<SopStep> { new SopStep() };

            var ex = Assert.Throws<ArgumentException>(() =>
                _sopLogic.UpdateSop("Name", invalidImage!, steps, Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("Image cannot be null or empty"));
        }

        [Test]
        public void UpdateSop_ShouldThrow_WhenSopStepsIsNull()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _sopLogic.UpdateSop("Name", "img.png", null!, Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("SopSteps cannot be null or empty"));
        }

        [Test]
        public void UpdateSop_ShouldThrow_WhenSopStepsIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _sopLogic.UpdateSop("Name", "img.png", new List<SopStep>(), Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("SopSteps cannot be null or empty"));
        }

        [Test]
        public void UpdateSop_ShouldThrow_WhenSopNotFound()
        {
            _sopRepositoryMock.Setup(r => r.GetSopById(It.IsAny<Guid>()))
                .Returns((Sop?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _sopLogic.UpdateSop("Name", "img.png", new List<SopStep> { new SopStep() }, Guid.NewGuid()));

            Assert.That(ex.Message, Does.Contain("not found"));
        }

        [Test]
        public void UpdateSop_ShouldCallRepository_WhenSopExists()
        {
            var sopId = Guid.NewGuid();
            var existingSop = new Sop
            {
                SopId = sopId,
                Name = "Old Name",
                Image = "old.png",
                SopSteps = new List<SopStep> { new SopStep { Description = "Old Step" } }
            };

            _sopRepositoryMock.Setup(r => r.GetSopById(sopId)).Returns(existingSop);

            var newSteps = new List<SopStep> { new SopStep { Description = "New Step" } };

            _sopLogic.UpdateSop("New Name", "new.png", newSteps, sopId);

            _sopRepositoryMock.Verify(r => r.UpdateSop(It.Is<Sop>(s =>
                s.SopId == sopId &&
                s.Name == "New Name" &&
                s.Image == "new.png" &&
                s.SopSteps == newSteps
            )), Times.Once);
        }
        
        // ---------- GetAllSops----------

        [Test]
        public void GetAllSops_ShouldReturnDataFromRepository()
        {
            var sops = new List<Sop>
            {
                new Sop { Name = "SOP 1" },
                new Sop { Name = "SOP 2" }
            };

            _sopRepositoryMock.Setup(r => r.GetAllSops()).Returns(sops);

            var result = _sopLogic.GetAllSops();

            Assert.That(result, Is.EqualTo(sops));
        }

        [Test]
        public void GetSopById_ShouldReturnSingleSop()
        {
            var sop = new Sop { SopId = Guid.NewGuid(), Name = "Single SOP" };
            _sopRepositoryMock.Setup(r => r.GetSopById(sop.SopId)).Returns(sop);

            var result = _sopLogic.GetSopById(sop.SopId);

            Assert.That(result, Is.EqualTo(sop));
        }
    }
}
