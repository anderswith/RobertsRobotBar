using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RobotBarApp.BLL;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.BE;
using RobotBarApp.Services.Robot.Interfaces;

namespace UnitTests
{
    [TestFixture]
    public class RobotLogicTests
    {
        private Mock<IRobotScriptRunner> _scriptRunnerMock;
        private Mock<IIngredientLogic> _ingredientLogicMock;
        private Mock<IDrinkLogic> _drinkLogicMock;
        private Mock<IDrinkUseCountLogic> _drinkUseCountLogicMock;
        private Mock<IIngredientUseCountLogic> _ingredientUseCountLogicMock;

        private RobotLogic _robotLogic;

        [SetUp]
        public void Setup()
        {
            _scriptRunnerMock = new Mock<IRobotScriptRunner>();
            _ingredientLogicMock = new Mock<IIngredientLogic>();
            _drinkLogicMock = new Mock<IDrinkLogic>();
            _drinkUseCountLogicMock = new Mock<IDrinkUseCountLogic>();
            _ingredientUseCountLogicMock = new Mock<IIngredientUseCountLogic>();

            _robotLogic = new RobotLogic(
                _scriptRunnerMock.Object,
                _ingredientLogicMock.Object,
                _drinkLogicMock.Object,
                _ingredientUseCountLogicMock.Object,
                _drinkUseCountLogicMock.Object
            );
        }
        
        // RunIngredientScript tests

        [Test]
        public void RunIngredientScript_Throws_WhenIdsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _robotLogic.RunIngredientScript(null));

            Assert.Throws<ArgumentException>(() =>
                _robotLogic.RunIngredientScript(new List<Guid>()));
        }

        [Test]
        public void RunIngredientScript_Throws_WhenIngredientsNotFound()
        {
            var ids = new List<Guid> { Guid.NewGuid() };

            _ingredientLogicMock
                .Setup(m => m.GetIngredientsWithScripts(ids))
                .Returns(new List<Ingredient>()); // empty list

            Assert.Throws<ArgumentException>(() =>
                _robotLogic.RunIngredientScript(ids));
        }

        [Test]
        public void RunIngredientScript_AddsUseCount_ForEachIngredient()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var ids = new List<Guid> { id1, id2 };

            var ingredient = new Ingredient
            {
                IngredientId = id1,
                IngredientScripts = new List<IngredientScript>()
            };

            _ingredientLogicMock
                .Setup(m => m.GetIngredientsWithScripts(ids))
                .Returns(new List<Ingredient>
                {
                    new Ingredient { IngredientId = id1, IngredientScripts = new List<IngredientScript>() },
                    new Ingredient { IngredientId = id2, IngredientScripts = new List<IngredientScript>() }
                });

            _robotLogic.RunIngredientScript(ids);

            _ingredientUseCountLogicMock.Verify(m => m.AddIngredientUseCount(id1), Times.Once);
            _ingredientUseCountLogicMock.Verify(m => m.AddIngredientUseCount(id2), Times.Once);
        }

        [Test]
        public void RunIngredientScript_QueuesScriptsInCorrectOrder()
        {
            var ingredientId = Guid.NewGuid();
            var ids = new List<Guid> { ingredientId };

            var ingredient = new Ingredient
            {
                IngredientId = ingredientId,
                IngredientScripts = new List<IngredientScript>
                {
                    new IngredientScript { Number = 3, UrScript = "third" },
                    new IngredientScript { Number = 1, UrScript = "first" },
                    new IngredientScript { Number = 2, UrScript = "second" }
                }
            };

            _ingredientLogicMock
                .Setup(m => m.GetIngredientsWithScripts(ids))
                .Returns(new List<Ingredient> { ingredient });

            _robotLogic.RunIngredientScript(ids);

            _scriptRunnerMock.Verify(s =>
                s.QueueScripts(It.Is<IEnumerable<string>>(list =>
                    list.SequenceEqual(new[] { "first", "second", "third" })
                )), Times.Once);
        }

        // RunDrinkScripts tests

        [Test]
        public void RunDrinkScripts_Throws_WhenDrinkIdEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                _robotLogic.RunDrinkScripts(Guid.Empty));
        }

        [Test]
        public void RunDrinkScripts_Throws_WhenDrinkNotFound()
        {
            var id = Guid.NewGuid();

            _drinkLogicMock
                .Setup(m => m.GetDrinksWithScripts(id))
                .Returns((Drink)null);

            Assert.Throws<ArgumentException>(() =>
                _robotLogic.RunDrinkScripts(id));
        }

        [Test]
        public void RunDrinkScripts_AddsDrinkUseCount()
        {
            var drinkId = Guid.NewGuid();

            var drink = new Drink
            {
                DrinkId = drinkId,
                DrinkContents = new List<DrinkContent>(),
                DrinkScripts = new List<DrinkScript>()
            };

            _drinkLogicMock
                .Setup(m => m.GetDrinksWithScripts(drinkId))
                .Returns(drink);

            _robotLogic.RunDrinkScripts(drinkId);

            _drinkUseCountLogicMock.Verify(m => m.AddDrinkUseCount(drinkId), Times.Once);
        }

        [Test]
        public void RunDrinkScripts_AddsIngredientUseCount_ForEachIngredient()
        {
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
                DrinkScripts = new List<DrinkScript>()
            };

            _drinkLogicMock
                .Setup(m => m.GetDrinksWithScripts(drinkId))
                .Returns(drink);

            _robotLogic.RunDrinkScripts(drinkId);

            _ingredientUseCountLogicMock.Verify(m => m.AddIngredientUseCount(ing1), Times.Once);
            _ingredientUseCountLogicMock.Verify(m => m.AddIngredientUseCount(ing2), Times.Once);
        }

        [Test]
        public void RunDrinkScripts_QueuesScriptsInCorrectOrder()
        {
            var drinkId = Guid.NewGuid();

            var drink = new Drink
            {
                DrinkId = drinkId,
                DrinkContents = new List<DrinkContent>(),
                DrinkScripts = new List<DrinkScript>
                {
                    new DrinkScript { Number = 5, UrScript = "fifth" },
                    new DrinkScript { Number = 1, UrScript = "first" },
                    new DrinkScript { Number = 3, UrScript = "third" }
                }
            };

            _drinkLogicMock
                .Setup(m => m.GetDrinksWithScripts(drinkId))
                .Returns(drink);

            _robotLogic.RunDrinkScripts(drinkId);

            _scriptRunnerMock.Verify(s =>
                s.QueueScripts(It.Is<IEnumerable<string>>(list =>
                    list.SequenceEqual(new[] { "first", "third", "fifth" })
                )), Times.Once);
        }
    }
}
