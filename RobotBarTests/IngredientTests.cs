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
    public class IngredientTests
    {
        private Mock<IIngredientRepository> _ingredientRepositoryMock;
        private IngredientLogic _ingredientLogic;

        [SetUp]
        public void Setup()
        {
            _ingredientRepositoryMock = new Mock<IIngredientRepository>();
            _ingredientLogic = new IngredientLogic(_ingredientRepositoryMock.Object);
        }

        // ---------- ADD ----------

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(invalidName, "Type", "Image.png", 10, 1));
            Assert.That(ex.Message, Is.EqualTo("Ingredient name cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenTypeIsNullOrEmpty(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", invalidType, "Image.png", 10, 1));
            Assert.That(ex.Message, Is.EqualTo("Ingredient type cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", invalidImage, 10, 1));
            Assert.That(ex.Message, Is.EqualTo("Ingredient image cannot be null or empty."));
        }

        [Test]
        public void AddIngredient_ShouldThrow_WhenSizeIsNegative()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", "Image.png", -5, 1));
            Assert.That(ex.Message, Is.EqualTo("Ingredient size cannot be negative."));
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void AddIngredient_ShouldThrow_WhenDoseIsZeroOrNegative(double invalidDose)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", "Image.png", 5, invalidDose));
            Assert.That(ex.Message, Is.EqualTo("Ingredient dose cannot be negative or 0."));
        }

        [Test]
        public void AddIngredient_ShouldCallRepository_WhenDataIsValid()
        {
            // Act
            _ingredientLogic.AddIngredient("Vodka", "Alcohol", "vodka.png", 750, 50);

            // Assert
            _ingredientRepositoryMock.Verify(r => r.AddIngredient(It.Is<Ingredient>(i =>
                i.Name == "Vodka" &&
                i.Type == "Alcohol" &&
                i.Image == "vodka.png" &&
                i.Size == 750 &&
                i.Dose == 50
            )), Times.Once);
        }

        // ---------- DELETE ----------

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenIngredientNotFound()
        {
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(It.IsAny<Guid>())).Returns((Ingredient)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _ingredientLogic.DeleteIngredient(Guid.NewGuid()));

            Assert.That(ex.Message, Does.Contain("not found"));
        }

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenIngredientIsUsedInDrink()
        {
            var ingredient = new Ingredient
            {
                IngredientId = Guid.NewGuid(),
                DrinkContents = new List<DrinkContent> { new DrinkContent() }
            };

            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(ingredient.IngredientId))
                .Returns(ingredient);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _ingredientLogic.DeleteIngredient(ingredient.IngredientId));

            Assert.That(ex.Message, Is.EqualTo("Cannot delete ingredient that is used in a drink."));
        }

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenIngredientIsUsedInBarSetup()
        {
            var ingredient = new Ingredient
            {
                IngredientId = Guid.NewGuid(),
                BarSetups = new List<BarSetup> { new BarSetup() }
            };

            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(ingredient.IngredientId))
                .Returns(ingredient);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _ingredientLogic.DeleteIngredient(ingredient.IngredientId));

            Assert.That(ex.Message, Is.EqualTo("Cannot delete ingredient that is used in a bar setup."));
        }

        [Test]
        public void DeleteIngredient_ShouldCallRepository_WhenIngredientIsValidAndUnused()
        {
            var ingredient = new Ingredient { IngredientId = Guid.NewGuid() };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(ingredient.IngredientId))
                .Returns(ingredient);

            _ingredientLogic.DeleteIngredient(ingredient.IngredientId);

            _ingredientRepositoryMock.Verify(r => r.DeleteIngredient(ingredient), Times.Once);
        }

        // ---------- UPDATE ----------

        [TestCase(null)]
        [TestCase("")]
        public void UpdateIngredient_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(Guid.NewGuid(), invalidName, "Type", "img.png", 10, 1));
            Assert.That(ex.Message, Is.EqualTo("Ingredient name cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateIngredient_ShouldThrow_WhenTypeIsNullOrEmpty(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(Guid.NewGuid(), "Name", invalidType, "img.png", 10, 1));
            Assert.That(ex.Message, Is.EqualTo("Ingredient type cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateIngredient_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(Guid.NewGuid(), "Name", "Type", invalidImage, 10, 1));
            Assert.That(ex.Message, Is.EqualTo("Ingredient image cannot be null or empty."));
        }

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenSizeIsNegative()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(Guid.NewGuid(), "Name", "Type", "img.png", -5, 1));
            Assert.That(ex.Message, Is.EqualTo("Ingredient size cannot be negative."));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void UpdateIngredient_ShouldThrow_WhenDoseIsZeroOrNegative(double invalidDose)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(Guid.NewGuid(), "Name", "Type", "img.png", 10, invalidDose));
            Assert.That(ex.Message, Is.EqualTo("Ingredient dose cannot be negative or 0."));
        }

        [Test]
        public void UpdateIngredient_ShouldCallAdd_WhenIngredientDoesNotExist()
        {
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(It.IsAny<Guid>()))
                .Returns((Ingredient)null);

            _ingredientLogic.UpdateIngredient(Guid.NewGuid(), "Name", "Type", "img.png", 10, 1);

            _ingredientRepositoryMock.Verify(r => r.AddIngredient(It.Is<Ingredient>(i =>
                i.Name == "Name" && i.Type == "Type" && i.Image == "img.png"
            )), Times.Once);
        }

        [Test]
        public void UpdateIngredient_ShouldCallUpdate_WhenIngredientExists()
        {
            var id = Guid.NewGuid();
            var existingIngredient = new Ingredient { IngredientId = id, Name = "Old" };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id)).Returns(existingIngredient);

            _ingredientLogic.UpdateIngredient(id, "New", "Type", "img.png", 10, 1);

            _ingredientRepositoryMock.Verify(r => r.UpdateIngredient(It.Is<Ingredient>(i =>
                i.Name == "New" &&
                i.Type == "Type" &&
                i.Image == "img.png" &&
                i.Size == 10 &&
                i.Dose == 1
            )), Times.Once);
        }
    }
}
