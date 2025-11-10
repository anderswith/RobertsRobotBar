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

        // ---------- ADD VALIDATION ----------

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient(invalidName, "Type", "Image.png", 10, "single", new List<string> { "Script" }));
            Assert.That(ex.Message, Is.EqualTo("Ingredient name cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenTypeIsNullOrEmpty(string? invalidType)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", invalidType, "Image.png", 10, "single", new List<string> { "Script" }));
            Assert.That(ex.Message, Is.EqualTo("Ingredient type cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddIngredient_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", invalidImage, 10, "single", new List<string> { "Script" }));
            Assert.That(ex.Message, Is.EqualTo("Ingredient image cannot be null or empty."));
        }

        [Test]
        public void AddIngredient_ShouldThrow_WhenSizeIsNegative()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", "Image.png", -5, "single", new List<string> { "Script" }));
            Assert.That(ex.Message, Is.EqualTo("Ingredient size cannot be negative."));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("InvalidDose")]
        public void AddIngredient_ShouldThrow_WhenDoseIsInvalid(string? invalidDose)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", "Image.png", 5, invalidDose, new List<string> { "Script" }));
            Assert.That(ex.Message, Is.EqualTo("Ingredient dose has to be single or double."));
        }

        [Test]
        public void AddIngredient_ShouldThrow_WhenScriptNamesIsNull()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", "Image.png", 10, "single", null));
            Assert.That(ex.Message, Is.EqualTo("Ingredient must have at least one script."));
        }

        [Test]
        public void AddIngredient_ShouldThrow_WhenScriptNamesIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", "Image.png", 10, "single", new List<string>()));
            Assert.That(ex.Message, Is.EqualTo("Ingredient must have at least one script."));
        }

        [Test]
        public void AddIngredient_ShouldThrow_WhenScriptContainsWhitespace()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.AddIngredient("Name", "Type", "Image.png", 10, "single", new List<string> { "  " }));
            Assert.That(ex.Message, Is.EqualTo("Script name cannot be null or whitespace."));
        }

        // ---------- ADD SUCCESS ----------

        [Test]
        public void AddIngredient_ShouldCallRepository_WithValidDataAndScripts()
        {
            var scripts = new List<string> { "ScriptA", "ScriptB" };

            _ingredientLogic.AddIngredient("Vodka", "Alcohol", "vodka.png", 750, "single", scripts);

            _ingredientRepositoryMock.Verify(r => r.AddIngredient(It.Is<Ingredient>(i =>
                i.Name == "Vodka" &&
                i.Type == "Alcohol" &&
                i.Image == "vodka.png" &&
                i.Dose == "single" &&
                i.IngredientScripts.Count == 2 &&
                i.IngredientScripts.First().Number == 1 &&
                i.IngredientScripts.Last().Number == 2
            )), Times.Once);
        }

        // ---------- DELETE ----------

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenNotFound()
        {
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(It.IsAny<Guid>())).Returns((Ingredient)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _ingredientLogic.DeleteIngredient(Guid.NewGuid()));

            Assert.That(ex.Message, Does.Contain("not found"));
        }

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenUsedInDrink()
        {
            var ingredient = new Ingredient { DrinkContents = new List<DrinkContent> { new DrinkContent() } };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(It.IsAny<Guid>())).Returns(ingredient);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _ingredientLogic.DeleteIngredient(Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("Cannot delete ingredient that is used in a drink."));
        }

        [Test]
        public void DeleteIngredient_ShouldThrow_WhenUsedInBarSetup()
        {
            var ingredient = new Ingredient { BarSetups = new List<BarSetup> { new BarSetup() } };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(It.IsAny<Guid>())).Returns(ingredient);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _ingredientLogic.DeleteIngredient(Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("Cannot delete ingredient that is used in a bar setup."));
        }

        [Test]
        public void DeleteIngredient_ShouldCallRepository_WhenValid()
        {
            var ingredient = new Ingredient { IngredientId = Guid.NewGuid() };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(ingredient.IngredientId)).Returns(ingredient);

            _ingredientLogic.DeleteIngredient(ingredient.IngredientId);

            _ingredientRepositoryMock.Verify(r => r.DeleteIngredient(ingredient), Times.Once);
        }

        // ---------- UPDATE VALIDATION ----------

        [TestCase(null)]
        [TestCase("")]
        public void UpdateIngredient_ShouldThrow_WhenNameIsNullOrEmpty(string? invalidName)
        {
            var id = Guid.NewGuid();
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id))
                .Returns(new Ingredient { IngredientId = id });

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(id, invalidName, "Type", "Image.png", 10, "single", new List<string> { "Script1" }));

            Assert.That(ex.Message, Is.EqualTo("Ingredient name cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateIngredient_ShouldThrow_WhenTypeIsNullOrEmpty(string? invalidType)
        {
            var id = Guid.NewGuid();
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id))
                .Returns(new Ingredient { IngredientId = id });

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(id, "Name", invalidType, "Image.png", 10, "single", new List<string> { "Script1" }));

            Assert.That(ex.Message, Is.EqualTo("Ingredient type cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateIngredient_ShouldThrow_WhenImageIsNullOrEmpty(string? invalidImage)
        {
            var id = Guid.NewGuid();
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id))
                .Returns(new Ingredient { IngredientId = id });

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(id, "Name", "Type", invalidImage, 10, "single", new List<string> { "Script1" }));

            Assert.That(ex.Message, Is.EqualTo("Ingredient image cannot be null or empty."));
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void UpdateIngredient_ShouldThrow_WhenSizeIsZeroOrNegative(double invalidSize)
        {
            var id = Guid.NewGuid();
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id))
                .Returns(new Ingredient { IngredientId = id });

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(id, "Name", "Type", "Image.png", invalidSize, "single", new List<string> { "Script1" }));

            Assert.That(ex.Message, Is.EqualTo("Ingredient size cannot be negative."));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("InvalidDose")]
        public void UpdateIngredient_ShouldThrow_WhenDoseIsInvalid(string? invalidDose)
        {
            var id = Guid.NewGuid();
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id))
                .Returns(new Ingredient { IngredientId = id });

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(id, "Name", "Type", "Image.png", 10, invalidDose, new List<string> { "Script1" }));

            Assert.That(ex.Message, Is.EqualTo("Ingredient dose has to be single or double."));
        }

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenScriptNamesIsNull()
        {
            var id = Guid.NewGuid();
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id))
                .Returns(new Ingredient { IngredientId = id });

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(id, "Name", "Type", "Image.png", 10, "single", null));

            Assert.That(ex.Message, Is.EqualTo("Ingredient must have at least one script."));
        }

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenScriptNamesIsEmpty()
        {
            var id = Guid.NewGuid();
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id))
                .Returns(new Ingredient { IngredientId = id });

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(id, "Name", "Type", "Image.png", 10, "single", new List<string>()));

            Assert.That(ex.Message, Is.EqualTo("Ingredient must have at least one script."));
        }

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenScriptNamesContainWhitespace()
        {
            var id = Guid.NewGuid();
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id))
                .Returns(new Ingredient { IngredientId = id });

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(id, "Name", "Type", "Image.png", 10, "single", new List<string> { "   ", "Valid" }));

            Assert.That(ex.Message, Is.EqualTo("Script name cannot be null or whitespace."));
        }

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenNotFound()
        {
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(It.IsAny<Guid>())).Returns((Ingredient)null);

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(Guid.NewGuid(), "Name", "Type", "Image.png", 10, "single", new List<string> { "Script" }));

            Assert.That(ex.Message, Is.EqualTo("Ingredient not found."));
        }

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenScriptListInvalid()
        {
            var existing = new Ingredient { IngredientId = Guid.NewGuid() };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(existing.IngredientId)).Returns(existing);

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(existing.IngredientId, "Name", "Type", "Image.png", 10, "single", new List<string>()));

            Assert.That(ex.Message, Is.EqualTo("Ingredient must have at least one script."));
        }

        [Test]
        public void UpdateIngredient_ShouldThrow_WhenScriptNameIsWhitespace()
        {
            var existing = new Ingredient { IngredientId = Guid.NewGuid() };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(existing.IngredientId)).Returns(existing);

            var ex = Assert.Throws<ArgumentException>(() =>
                _ingredientLogic.UpdateIngredient(existing.IngredientId, "Name", "Type", "Image.png", 10, "single", new List<string> { "   " }));

            Assert.That(ex.Message, Is.EqualTo("Script name cannot be null or whitespace."));
        }

        // ---------- UPDATE FUNCTIONALITY ----------

        [Test]
        public void UpdateIngredient_ShouldAddNewAndRemoveOldScripts()
        {
            var id = Guid.NewGuid();
            var existing = new Ingredient
            {
                IngredientId = id,
                IngredientScripts = new List<IngredientScript>
                {
                    new IngredientScript { UrScript = "OldScript", Number = 1 }
                }
            };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id)).Returns(existing);

            var newScripts = new List<string> { "NewScript" };

            _ingredientLogic.UpdateIngredient(id, "Name", "Type", "Image.png", 10, "single", newScripts);

            _ingredientRepositoryMock.Verify(r => r.UpdateIngredient(It.Is<Ingredient>(i =>
                i.IngredientScripts.Count == 1 &&
                i.IngredientScripts.First().UrScript == "NewScript"
            )), Times.Once);
        }

        [Test]
        public void UpdateIngredient_ShouldKeepExistingScripts_WhenSame()
        {
            var id = Guid.NewGuid();
            var existing = new Ingredient
            {
                IngredientId = id,
                IngredientScripts = new List<IngredientScript>
                {
                    new IngredientScript { UrScript = "SameScript", Number = 1 }
                }
            };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id)).Returns(existing);

            _ingredientLogic.UpdateIngredient(id, "Name", "Type", "Image.png", 10, "single", new List<string> { "SameScript" });

            _ingredientRepositoryMock.Verify(r => r.UpdateIngredient(It.Is<Ingredient>(i =>
                i.IngredientScripts.Count == 1 &&
                i.IngredientScripts.First().UrScript == "SameScript"
            )), Times.Once);
        }

        [Test]
        public void UpdateIngredient_ShouldIncrementScriptNumbers_WhenAddingNew()
        {
            var id = Guid.NewGuid();
            var existing = new Ingredient
            {
                IngredientId = id,
                IngredientScripts = new List<IngredientScript>
                {
                    new IngredientScript { UrScript = "Old", Number = 3 }
                }
            };
            _ingredientRepositoryMock.Setup(r => r.GetIngredientById(id)).Returns(existing);

            _ingredientLogic.UpdateIngredient(id, "Name", "Type", "Image.png", 10, "single", new List<string> { "Old", "New" });

            _ingredientRepositoryMock.Verify(r => r.UpdateIngredient(It.Is<Ingredient>(i =>
                i.IngredientScripts.Any(s => s.Number == 4)
            )), Times.Once);
        }
    }
}
