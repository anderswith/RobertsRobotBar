using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Application.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

namespace UnitTests
{
    [TestFixture]
    public class RobotLogicTests
    {
        private Mock<IRobotScriptRunner> _scriptRunnerMock;
        private Mock<IRobotDashboardStreamReader> _dashboardMock;
        private Mock<IIngredientLogic> _ingredientLogicMock;
        private Mock<IDrinkLogic> _drinkLogicMock;
        private Mock<IIngredientUseCountLogic> _ingredientUseMock;
        private Mock<IDrinkUseCountLogic> _drinkUseMock;
        private Mock<IEventSessionService> _sessionMock;

        private RobotLogic _logic;

        [SetUp]
        public void Setup()
        {
            _scriptRunnerMock = new Mock<IRobotScriptRunner>();
            _dashboardMock = new Mock<IRobotDashboardStreamReader>();
            _ingredientLogicMock = new Mock<IIngredientLogic>();
            _drinkLogicMock = new Mock<IDrinkLogic>();
            _ingredientUseMock = new Mock<IIngredientUseCountLogic>();
            _drinkUseMock = new Mock<IDrinkUseCountLogic>();
            _sessionMock = new Mock<IEventSessionService>();

            _sessionMock.Setup(s => s.HasActiveEvent).Returns(true);
            _sessionMock.Setup(s => s.CurrentEventId).Returns(Guid.NewGuid());

            _logic = new RobotLogic(
                _scriptRunnerMock.Object,
                _dashboardMock.Object,
                _ingredientLogicMock.Object,
                _drinkLogicMock.Object,
                _ingredientUseMock.Object,
                _drinkUseMock.Object,
                _sessionMock.Object);
        }

        //RunRobotScripts

        [Test]
        public void RunRobotScripts_QueuesScripts()
        {
            var scripts = new List<string> { "move", "place" };

            _logic.RunRobotScripts(scripts);

            _scriptRunnerMock.Verify(r =>
                r.QueueScripts(It.Is<IEnumerable<string>>(s => s.SequenceEqual(scripts))),
                Times.Once);
        }

        //RunMixSelvScripts

        [Test]
        public void RunMixSelvScripts_Throws_WhenOrderNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() => _logic.RunMixSelvScripts(null!));
            Assert.Throws<ArgumentException>(() => _logic.RunMixSelvScripts(new List<(Guid, int)>()));
        }

        [Test]
        public void RunMixSelvScripts_Throws_WhenIngredientNotFound()
        {
            var order = new List<(Guid, int)> { (Guid.NewGuid(), 2) };

            _ingredientLogicMock
                .Setup(l => l.GetIngredientsWithScripts(It.IsAny<List<Guid>>()))
                .Returns(new List<Ingredient>());

            Assert.Throws<InvalidOperationException>(() =>
                _logic.RunMixSelvScripts(order));
        }

        [Test]
        public void RunMixSelvScripts_Throws_WhenClUnsupported()
        {
            var ingId = Guid.NewGuid();

            _ingredientLogicMock
                .Setup(l => l.GetIngredientsWithScripts(It.IsAny<List<Guid>>()))
                .Returns(new List<Ingredient>
                {
                    new Ingredient
                    {
                        IngredientId = ingId,
                        Name = "Vodka"
                    }
                });

            var order = new List<(Guid, int)> { (ingId, 6) };

            Assert.Throws<InvalidOperationException>(() =>
                _logic.RunMixSelvScripts(order));
        }

        [Test]
        public void RunMixSelvScripts_QueuesCorrectScripts_AndAddsUseCounts()
        {
            var eventId = Guid.NewGuid();
            _sessionMock.Setup(s => s.CurrentEventId).Returns(eventId);

            var ingId = Guid.NewGuid();

            var ingredient = new Ingredient
            {
                IngredientId = ingId,
                SingleScripts = new List<SingleScript>
                {
                    new SingleScript { Number = 1, UrScript = "S1" },
                    new SingleScript { Number = 2, UrScript = "S2" }
                },
                DoubleScripts = new List<DoubleScript>
                {
                    new DoubleScript { Number = 1, UrScript = "D1" }
                }
            };

            _ingredientLogicMock
                .Setup(l => l.GetIngredientsWithScripts(It.IsAny<List<Guid>>()))
                .Returns(new List<Ingredient> { ingredient });

            var order = new List<(Guid, int)>
            {
                (ingId, 2),
                (ingId, 4)
            };

            _logic.RunMixSelvScripts(order);

            _ingredientUseMock.Verify(
                u => u.AddIngredientUseCount(ingId, eventId),
                Times.Exactly(2));

            _scriptRunnerMock.Verify(r =>
                r.QueueScripts(It.Is<IEnumerable<string>>(s =>
                    s.SequenceEqual(new[] { "S1", "S2", "D1" }))),
                Times.Once);
        }

        //RunDrinkScripts

        [Test]
        public void RunDrinkScripts_Throws_WhenDrinkIdEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _logic.RunDrinkScripts(Guid.Empty));
        }

        [Test]
        public void RunDrinkScripts_Throws_WhenDrinkNotFound()
        {
            _drinkLogicMock
                .Setup(l => l.GetDrinksWithScripts(It.IsAny<Guid>()))
                .Returns((Drink?)null);

            Assert.Throws<ArgumentException>(() =>
                _logic.RunDrinkScripts(Guid.NewGuid()));
        }

        [Test]
        public void RunDrinkScripts_AddsUseCounts_AndQueuesScripts()
        {
            var eventId = Guid.NewGuid();
            _sessionMock.Setup(s => s.CurrentEventId).Returns(eventId);

            var drinkId = Guid.NewGuid();
            var ingA = Guid.NewGuid();
            var ingB = Guid.NewGuid();

            var drink = new Drink
            {
                DrinkId = drinkId,
                DrinkContents = new List<DrinkContent>
                {
                    new DrinkContent { IngredientId = ingA, Dose = "single" },
                    new DrinkContent { IngredientId = ingB, Dose = "double" }
                },
                DrinkScripts = new List<DrinkScript>
                {
                    new DrinkScript { Number = 1, UrScript = "A" },
                    new DrinkScript { Number = 2, UrScript = "B" }
                }
            };

            _drinkLogicMock
                .Setup(l => l.GetDrinksWithScripts(drinkId))
                .Returns(drink);

            _logic.RunDrinkScripts(drinkId);

            _drinkUseMock.Verify(u =>
                u.AddDrinkUseCount(drinkId, eventId),
                Times.Once);

            _ingredientUseMock.Verify(u =>
                u.AddIngredientUseCount(ingA, eventId),
                Times.Once);

            _ingredientUseMock.Verify(u =>
                u.AddIngredientUseCount(ingB, eventId),
                Times.Exactly(2));

            _scriptRunnerMock.Verify(r =>
                r.QueueScripts(It.Is<IEnumerable<string>>(s =>
                    s.SequenceEqual(new[] { "A", "B" }))),
                Times.Once);
        }

        //ConnectionFailed

        [Test]
        public void ConnectionFailed_IsPropagated_AndFlagSet()
        {
            bool raised = false;
            _logic.ConnectionFailed += () => raised = true;

            _dashboardMock.Raise(d => d.ConnectionFailed += null);

            Assert.That(_logic.ConnectionFailedAlready, Is.True);
            Assert.That(raised, Is.True);
        }
    }
}
