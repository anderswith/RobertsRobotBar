using Moq;
using NUnit.Framework;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Application.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

namespace RobotBarApp.Tests.BLL
{
    [TestFixture]
    public class RobotLogicTests
    {
        private Mock<IRobotScriptRunner> _scriptRunnerMock;
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
            _ingredientLogicMock = new Mock<IIngredientLogic>();
            _drinkLogicMock = new Mock<IDrinkLogic>();
            _ingredientUseMock = new Mock<IIngredientUseCountLogic>();
            _drinkUseMock = new Mock<IDrinkUseCountLogic>();
            _sessionMock = new Mock<IEventSessionService>();

            _sessionMock.Setup(s => s.CurrentEventId).Returns(Guid.NewGuid());
            _sessionMock.Setup(s => s.HasActiveEvent).Returns(true);

            _logic = new RobotLogic(
                _scriptRunnerMock.Object,
                _ingredientLogicMock.Object,
                _drinkLogicMock.Object,
                _ingredientUseMock.Object,
                _drinkUseMock.Object,
                _sessionMock.Object);
        }

        // ------------------------------------------------------------
        // RunRobotScripts
        // ------------------------------------------------------------

        [Test]
        public void RunRobotScripts_QueuesScripts()
        {
            var scripts = new List<string> { "move", "place" };

            _logic.RunRobotScripts(scripts);

            _scriptRunnerMock.Verify(r => r.QueueScripts(
                It.Is<IEnumerable<string>>(l => l.SequenceEqual(scripts))
            ), Times.Once);
        }

        // ------------------------------------------------------------
        // RunIngredientScript
        // ------------------------------------------------------------

        [Test]
        public void RunIngredientScript_Throws_WhenIngredientIdsNull()
        {
            Assert.Throws<ArgumentException>(() => _logic.RunIngredientScript(null));
        }

        [Test]
        public void RunIngredientScript_Throws_WhenIngredientIdsEmpty()
        {
            Assert.Throws<ArgumentException>(() => _logic.RunIngredientScript(new List<Guid>()));
        }

        [Test]
        public void RunIngredientScript_Throws_WhenNoIngredientScriptsFound()
        {
            _ingredientLogicMock
                .Setup(l => l.GetIngredientsWithScripts(It.IsAny<List<Guid>>()))
                .Returns(new List<Ingredient>());

            Assert.Throws<ArgumentException>(() =>
                _logic.RunIngredientScript(new List<Guid> { Guid.NewGuid() }));
        }

        [Test]
        public void RunIngredientScript_AddsUseCounts_AndQueuesScripts()
        {
            var eventId = Guid.NewGuid();
            _sessionMock.Setup(s => s.CurrentEventId).Returns(eventId);

            var ing1 = Guid.NewGuid();
            var ing2 = Guid.NewGuid();

            var ingredients = new List<Ingredient>
            {
                new Ingredient
                {
                    IngredientId = ing1,
                    IngredientScripts = new List<IngredientScript>
                    {
                        new IngredientScript { Number = 1, UrScript = "A1" },
                        new IngredientScript { Number = 2, UrScript = "A2" }
                    }
                },
                new Ingredient
                {
                    IngredientId = ing2,
                    IngredientScripts = new List<IngredientScript>
                    {
                        new IngredientScript { Number = 1, UrScript = "B1" }
                    }
                }
            };

            _ingredientLogicMock
                .Setup(l => l.GetIngredientsWithScripts(It.IsAny<List<Guid>>()))
                .Returns(ingredients);

            _logic.RunIngredientScript(new List<Guid> { ing1, ing2 });

            // Verify usecounts
            _ingredientUseMock.Verify(u => u.AddIngredientUseCount(ing1, eventId), Times.Once);
            _ingredientUseMock.Verify(u => u.AddIngredientUseCount(ing2, eventId), Times.Once);

            // Expected ordered scripts
            var expectedScripts = new[] { "A1", "A2", "B1" };

            _scriptRunnerMock.Verify(r => r.QueueScripts(
                It.Is<IEnumerable<string>>(l => l.SequenceEqual(expectedScripts))
            ), Times.Once);
        }

        // ------------------------------------------------------------
        // RunDrinkScripts
        // ------------------------------------------------------------

        [Test]
        public void RunDrinkScripts_Throws_WhenDrinkIdEmpty()
        {
            Assert.Throws<ArgumentException>(() => _logic.RunDrinkScripts(Guid.Empty));
        }

        [Test]
        public void RunDrinkScripts_Throws_WhenDrinkNotFound()
        {
            _drinkLogicMock
                .Setup(l => l.GetDrinksWithScripts(It.IsAny<Guid>()))
                .Returns((Drink)null);

            Assert.Throws<ArgumentException>(() => _logic.RunDrinkScripts(Guid.NewGuid()));
        }

        [Test]
        public void RunDrinkScripts_AddsUseCounts_AndQueuesScripts()
        {
            var eventId = Guid.NewGuid();
            _sessionMock.Setup(s => s.CurrentEventId).Returns(eventId);

            var drinkId = Guid.NewGuid();
            var ing1 = Guid.NewGuid();
            var ing2 = Guid.NewGuid();

            var drink = new Drink
            {
                DrinkId = drinkId,
                DrinkContents = new List<DrinkContent>
                {
                    new DrinkContent { IngredientId = ing1 },
                    new DrinkContent { IngredientId = ing2 }
                },
                DrinkScripts = new List<DrinkScript>
                {
                    new DrinkScript { Number = 1, UrScript = "S1" },
                    new DrinkScript { Number = 2, UrScript = "S2" }
                }
            };

            _drinkLogicMock
                .Setup(l => l.GetDrinksWithScripts(drinkId))
                .Returns(drink);

            _logic.RunDrinkScripts(drinkId);

            // Verify drink usecount
            _drinkUseMock.Verify(u => u.AddDrinkUseCount(drinkId, eventId), Times.Once);

            // Verify ingredient usecounts
            _ingredientUseMock.Verify(u => u.AddIngredientUseCount(ing1, eventId), Times.Once);
            _ingredientUseMock.Verify(u => u.AddIngredientUseCount(ing2, eventId), Times.Once);

            // Expected ordered scripts
            var expectedScripts = new[] { "S1", "S2" };

            _scriptRunnerMock.Verify(r => r.QueueScripts(
                It.Is<IEnumerable<string>>(l => l.SequenceEqual(expectedScripts))
            ), Times.Once);
        }
    }
}
