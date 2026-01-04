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
    public class IngredientLogicTests
    {
        private Mock<IIngredientRepository> _ingredientRepositoryMock;
        private IngredientLogic _ingredientLogic;

        [SetUp]
        public void Setup()
        {
            _ingredientRepositoryMock = new Mock<IIngredientRepository>();
            _ingredientLogic = new IngredientLogic(_ingredientRepositoryMock.Object);
        }

        //AddIngredient

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(
                    invalidName!, "Type", "Image.png", "Red", 1,
                    new List<string> { "S1" }, new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient name cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenTypeIsNullOrEmpty(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(
                    "Name", invalidType!, "Image.png", "Red", 1,
                    new List<string> { "S1" }, new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient type cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(
                    "Name", "Type", invalidImage!, "Red", 1,
                    new List<string> { "S1" }, new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient image cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenColorIsNullOrEmpty(string? invalidColor)
        {
            var ex = Assert.Throws<AggregateException>(() =>
                _ingredientLogic.AddIngredient(
                    "Name", "Type", "Image.png", invalidColor!, 1,
                    new List<string> { "S1" }, new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient color cannot be null or empty."));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void AddIngredient_ShouldThrow_WhenPositionNumberIsInvalid(int invalidPosition)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(
                    "Name", "Type", "Image.png", "Red", invalidPosition,
                    new List<string> { "S1" }, new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient position number cannot be negative."));
        }

        [Test]
        public void AddIngredient_ShouldThrow_WhenSingleScriptsMissing()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(
                    "Name", "Type", "Image.png", "Red", 1,
                    new List<string>(), new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient must have at least one single script."));
        }

        [Test]
        public void AddIngredient_ShouldThrow_WhenDoubleScriptsMissing()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(
                    "Name", "Type", "Image.png", "Red", 1,
                    new List<string> { "S1" }, new List<string>()));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient must have atleast one double script."));
        }

        [Test]
        public void AddIngredient_ShouldThrow_WhenScriptIsWhitespace()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(
                    "Name", "Type", "Image.png", "Red", 1,
                    new List<string> { "   " }, new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Script name cannot be null or whitespace."));
        }

        [Test]
        public void AddIngredient_ShouldCallRepository_WithValidData()
        {
            _ingredientLogic.AddIngredient(
                "Vodka", "Alcohol", "vodka.png", "Clear", 3,
                new List<string> { "SingleA", "SingleB" },
                new List<string> { "DoubleA" });

            _ingredientRepositoryMock.Verify(r =>
                r.AddIngredient(It.Is<Ingredient>(i =>
                    i.Name == "Vodka" &&
                    i.Type == "Alcohol" &&
                    i.Image == "vodka.png" &&
                    i.Color == "Clear" &&
                    i.IngredientPositions.Single().Position == 3 &&
                    i.SingleScripts.Count == 2 &&
                    i.DoubleScripts.Count == 1 &&
                    i.SingleScripts.First().Number == 1 &&
                    i.DoubleScripts.First().Number == 1
                )),
                Times.Once);
        }

        //DeleteIngredient

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenNotFound()
        {
            _ingredientRepositoryMock
                .Setup(r => r.GetIngredientById(It.IsAny<Guid>()))
                .Returns((Ingredient?)null);

            Assert.Throws<KeyNotFoundException>(() =>
                _ingredientLogic.DeleteIngredient(Guid.NewGuid()));
        }

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenUsedInDrink()
        {
            var ingredient = new Ingredient
            {
                DrinkContents = new List<DrinkContent> { new DrinkContent() }
            };

            _ingredientRepositoryMock
                .Setup(r => r.GetIngredientById(It.IsAny<Guid>()))
                .Returns(ingredient);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _ingredientLogic.DeleteIngredient(Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Cannot delete ingredient that is used in a drink."));
        }

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenUsedInBarSetup()
        {
            var ingredient = new Ingredient
            {
                BarSetups = new List<BarSetup> { new BarSetup() }
            };

            _ingredientRepositoryMock
                .Setup(r => r.GetIngredientById(It.IsAny<Guid>()))
                .Returns(ingredient);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _ingredientLogic.DeleteIngredient(Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Cannot delete ingredient that is used in a bar setup."));
        }

        [Test]
        public void DeleteIngredient_ShouldCallRepository_WhenValid()
        {
            var ingredient = new Ingredient { IngredientId = Guid.NewGuid() };

            _ingredientRepositoryMock
                .Setup(r => r.GetIngredientById(ingredient.IngredientId))
                .Returns(ingredient);

            _ingredientLogic.DeleteIngredient(ingredient.IngredientId);

            _ingredientRepositoryMock.Verify(
                r => r.DeleteIngredient(ingredient),
                Times.Once);
        }

        //UpdateIngredient

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(
                    Guid.Empty, "Name", "Type", "Img", "Red", 1,
                    new List<string> { "S1" }, new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient ID cannot be empty."));
        }

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenNotFound()
        {
            _ingredientRepositoryMock
                .Setup(r => r.GetIngredientById(It.IsAny<Guid>()))
                .Returns((Ingredient?)null);

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(
                    Guid.NewGuid(), "Name", "Type", "Img", "Red", 1,
                    new List<string> { "S1" }, new List<string> { "D1" }));

            Assert.That(ex!.Message, Is.EqualTo("Ingredient not found."));
        }

        [Test]
        public void UpdateIngredient_ShouldUpdateFields_AndCallRepository()
        {
            var id = Guid.NewGuid();
            var ingredient = new Ingredient
            {
                IngredientId = id,
                IngredientPositions = new List<IngredientPosition>
                {
                    new IngredientPosition { Position = 1 }
                },
                SingleScripts = new List<SingleScript>
                {
                    new SingleScript { UrScript = "OldS", Number = 1 }
                },
                DoubleScripts = new List<DoubleScript>
                {
                    new DoubleScript { UrScript = "OldD", Number = 1 }
                }
            };

            _ingredientRepositoryMock
                .Setup(r => r.GetIngredientById(id))
                .Returns(ingredient);

            _ingredientLogic.UpdateIngredient(
                id, "New", "NewType", "new.png", "Blue", 2,
                new List<string> { "NewSingle" },
                new List<string> { "NewDouble" });

            _ingredientRepositoryMock.Verify(r =>
                r.UpdateIngredient(It.Is<Ingredient>(i =>
                    i.Name == "New" &&
                    i.Type == "NewType" &&
                    i.Image == "new.png" &&
                    i.Color == "Blue" &&
                    i.IngredientPositions.First().Position == 2 &&
                    i.SingleScripts.First().UrScript == "NewSingle" &&
                    i.DoubleScripts.First().UrScript == "NewDouble"
                )),
                Times.Once);
        }

        //GetByType 

        [Test]
        public void GetAlcohol_DelegatesToRepository()
        {
            var eventId = Guid.NewGuid();
            _ingredientLogic.GetAlcohol(eventId);

            _ingredientRepositoryMock.Verify(
                r => r.GetIngredientByType("Alkohol", eventId),
                Times.Once);
        }

        [Test]
        public void GetSyrups_DelegatesToRepository()
        {
            var eventId = Guid.NewGuid();
            _ingredientLogic.GetSyrups(eventId);

            _ingredientRepositoryMock.Verify(
                r => r.GetIngredientByType("Syrup", eventId),
                Times.Once);
        }

        [Test]
        public void GetSoda_DelegatesToRepository()
        {
            var eventId = Guid.NewGuid();
            _ingredientLogic.GetSoda(eventId);

            _ingredientRepositoryMock.Verify(
                r => r.GetIngredientByType("Soda", eventId),
                Times.Once);
        }

        [Test]
        public void GetMockohol_DelegatesToRepository()
        {
            var eventId = Guid.NewGuid();
            _ingredientLogic.getMockohol(eventId);

            _ingredientRepositoryMock.Verify(
                r => r.GetIngredientByType("Mock", eventId),
                Times.Once);
        }

        [Test]
        public void GetIngredientsForPositions_DelegatesToRepository()
        {
            _ingredientLogic.GetIngredientsForPositions();

            _ingredientRepositoryMock.Verify(
                r => r.GetIngredientsForPositions(),
                Times.Once);
        }
    }
}
