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

        private RobotLogic _robotLogic;

        [SetUp]
        public void Setup()
        {
            _scriptRunnerMock = new Mock<IRobotScriptRunner>();
            _ingredientLogicMock = new Mock<IIngredientLogic>();
            _drinkLogicMock = new Mock<IDrinkLogic>();

            _robotLogic = new RobotLogic(
                _scriptRunnerMock.Object,
                _ingredientLogicMock.Object,
                _drinkLogicMock.Object
            );
        }
        

        [Test]
        public void RunRobotScripts_CallsScriptRunner()
        {
            var scripts = new List<string> { "script1", "script2" };

            _robotLogic.RunRobotScripts(scripts);

            _scriptRunnerMock.Verify(s => s.QueueScripts(scripts), Times.Once);
        }



        [Test]
        public void RunIngredientScript_LoadsScriptsInCorrectOrder()
        {
            // Arrange
            var ingredientId = Guid.NewGuid();
            var ingredientIds = new List<Guid> { ingredientId };

            var ingredient = new Ingredient
            {
                IngredientId = ingredientId,
                IngredientScripts = new List<IngredientScript>
                {
                    new IngredientScript { Number = 2, UrScript = "second" },
                    new IngredientScript { Number = 1, UrScript = "first" }
                }
            };

            _ingredientLogicMock
                .Setup(m => m.GetIngredientsWithScripts(ingredientIds))
                .Returns(new List<Ingredient> { ingredient });

            // Act
            _robotLogic.RunIngredientScript(ingredientIds);

            // Assert
            _scriptRunnerMock.Verify(s =>
                s.QueueScripts(It.Is<IEnumerable<string>>(list =>
                    list.SequenceEqual(new[] { "first", "second" })
                )), Times.Once);
        }




        [Test]
        public void RunDrinkScripts_Throws_WhenDrinkNotFound()
        {
            Guid drinkId = Guid.NewGuid();

            _drinkLogicMock
                .Setup(m => m.GetDrinksWithScripts(drinkId))
                .Returns((Drink)null);

            Assert.Throws<ArgumentException>(() =>
                _robotLogic.RunDrinkScripts(drinkId));
        }

        [Test]
        public void RunDrinkScripts_EnqueuesScriptsInOrder()
        {
            Guid drinkId = Guid.NewGuid();

            var drink = new Drink
            {
                DrinkId = drinkId,
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

            // Act
            _robotLogic.RunDrinkScripts(drinkId);

            // Assert
            _scriptRunnerMock.Verify(s =>
                s.QueueScripts(It.Is<IEnumerable<string>>(list =>
                    list.SequenceEqual(new[] { "first", "third", "fifth" })
                )), Times.Once);
        }
    }
}
