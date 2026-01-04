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
    public class DrinkLogicTests
    {
        private Mock<IDrinkRepository> _repo;
        private DrinkLogic _logic;

        [SetUp]
        public void SetUp()
        {
            _repo = new Mock<IDrinkRepository>();
            _logic = new DrinkLogic(_repo.Object);
        }

        //GetAllDrinks

        [Test]
        public void GetAllDrinks_ReturnsRepositoryResult()
        {
            var drinks = new List<Drink>
            {
                new Drink { DrinkId = Guid.NewGuid(), Name = "A" },
                new Drink { DrinkId = Guid.NewGuid(), Name = "B" }
            };

            _repo.Setup(r => r.GetAllDrinks()).Returns(drinks);

            var result = _logic.GetAllDrinks();

            Assert.That(result, Is.EqualTo(drinks));
            _repo.Verify(r => r.GetAllDrinks(), Times.Once);
        }

        //GetDrinkById

        [Test]
        public void GetDrinkById_EmptyGuid_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => _logic.GetDrinkById(Guid.Empty));
            Assert.That(ex!.Message, Is.EqualTo("Invalid drink ID."));
        }

        [Test]
        public void GetDrinkById_ValidGuid_ReturnsDrink()
        {
            var id = Guid.NewGuid();
            var drink = new Drink { DrinkId = id, Name = "Mojito" };

            _repo.Setup(r => r.GetDrinkById(id)).Returns(drink);

            var result = _logic.GetDrinkById(id);

            Assert.That(result, Is.EqualTo(drink));
        }

        //AddDrink 

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void AddDrink_InvalidName_Throws(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink(
                    name!,
                    "img.png",
                    false,
                    ValidContents(),
                    new List<string> { "S1" }));

            Assert.That(ex!.Message, Is.EqualTo("Drink name cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void AddDrink_InvalidImage_Throws(string? image)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink(
                    "Name",
                    image!,
                    false,
                    ValidContents(),
                    new List<string> { "S1" }));

            Assert.That(ex!.Message, Is.EqualTo("Drink image cannot be null or empty."));
        }

        [Test]
        public void AddDrink_NullContents_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink(
                    "Name",
                    "img.png",
                    false,
                    null!,
                    new List<string> { "S1" }));

            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one ingredient."));
        }

        [Test]
        public void AddDrink_EmptyContents_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink(
                    "Name",
                    "img.png",
                    false,
                    new List<DrinkContent>(),
                    new List<string> { "S1" }));

            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one ingredient."));
        }

        [Test]
        public void AddDrink_InvalidDose_Throws()
        {
            var contents = new List<DrinkContent>
            {
                new DrinkContent
                {
                    IngredientId = Guid.NewGuid(),
                    Dose = "triple"
                }
            };

            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink(
                    "Name",
                    "img.png",
                    false,
                    contents,
                    new List<string> { "S1" }));

            Assert.That(ex!.Message, Is.EqualTo("Dose must be either 'single' or 'double'."));
        }

        [Test]
        public void AddDrink_InvalidScripts_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink(
                    "Name",
                    "img.png",
                    false,
                    ValidContents(),
                    new List<string> { " ", null! }));

            Assert.That(ex!.Message, Is.EqualTo("Script name cannot be null or whitespace."));
        }

        [Test]
        public void AddDrink_Valid_AddsDrinkWithContentsAndScripts()
        {
            Drink? saved = null;

            _repo.Setup(r => r.AddDrink(It.IsAny<Drink>()))
                .Callback<Drink>(d => saved = d);

            _logic.AddDrink(
                "Gin Tonic",
                "gt.png",
                false,
                ValidContents(),
                new List<string> { "S1", "S2" });

            _repo.Verify(r => r.AddDrink(It.IsAny<Drink>()), Times.Once);
            Assert.That(saved, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(saved!.DrinkId, Is.Not.EqualTo(Guid.Empty));
                Assert.That(saved.Name, Is.EqualTo("Gin Tonic"));
                Assert.That(saved.DrinkContents, Has.Count.EqualTo(1));
                Assert.That(saved.DrinkScripts.Select(s => s.Number),
                    Is.EquivalentTo(new[] { 1, 2 }));
            });
        }

        //DeleteDrink

        [Test]
        public void DeleteDrink_NotFound_Throws()
        {
            _repo.Setup(r => r.GetDrinkById(It.IsAny<Guid>())).Returns((Drink?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _logic.DeleteDrink(Guid.NewGuid()));

            Assert.That(ex!.Message, Is.EqualTo("Drink not found."));
        }

        [Test]
        public void DeleteDrink_Found_Deletes()
        {
            var drink = new Drink { DrinkId = Guid.NewGuid() };

            _repo.Setup(r => r.GetDrinkById(drink.DrinkId)).Returns(drink);

            _logic.DeleteDrink(drink.DrinkId);

            _repo.Verify(r => r.DeleteDrink(drink), Times.Once);
        }

        //UpdateDrink

        [Test]
        public void UpdateDrink_NotFound_Throws()
        {
            _repo.Setup(r => r.GetDrinkById(It.IsAny<Guid>())).Returns((Drink?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _logic.UpdateDrink(
                    Guid.NewGuid(),
                    "Name",
                    "img.png",
                    false,
                    ValidContents(),
                    new List<string> { "S1" }));

            Assert.That(ex!.Message, Is.EqualTo("Drink not found."));
        }

        [Test]
        public void UpdateDrink_ReplacesScriptsAndUpdatesContents()
        {
            var ingredientId = Guid.NewGuid();

            var drink = new Drink
            {
                DrinkId = Guid.NewGuid(),
                DrinkContents = new List<DrinkContent>
                {
                    new DrinkContent
                    {
                        IngredientId = ingredientId,
                        Dose = "single"
                    }
                },
                DrinkScripts = new List<DrinkScript>
                {
                    new DrinkScript { UrScript = "Old", Number = 1 }
                }
            };

            _repo.Setup(r => r.GetDrinkById(drink.DrinkId)).Returns(drink);

            _logic.UpdateDrink(
                drink.DrinkId,
                "Updated",
                "new.png",
                false,
                new List<DrinkContent>
                {
                    new DrinkContent
                    {
                        IngredientId = ingredientId,
                        Dose = "double"
                    }
                },
                new List<string> { "A", "B" });

            _repo.Verify(r => r.UpdateDrink(drink), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(drink.Name, Is.EqualTo("Updated"));
                Assert.That(drink.DrinkScripts.Count, Is.EqualTo(2));
                Assert.That(drink.DrinkScripts.Select(s => s.Number),
                    Is.EquivalentTo(new[] { 1, 2 }));
                Assert.That(drink.DrinkContents.Single().Dose, Is.EqualTo("double"));
            });
        }

        //Exists
        [Test]
        public void Exists_EmptyGuid_ReturnsFalse()
        {
            var result = _logic.Exists(Guid.Empty);
            Assert.That(result, Is.False);
        }

        [Test]
        public void Exists_ValidGuid_DelegatesToRepo()
        {
            var id = Guid.NewGuid();
            _repo.Setup(r => r.Exists(id)).Returns(true);

            var result = _logic.Exists(id);

            Assert.That(result, Is.True);
            _repo.Verify(r => r.Exists(id), Times.Once);
        }

        // Helpers
        private static List<DrinkContent> ValidContents() =>
            new()
            {
                new DrinkContent
                {
                    IngredientId = Guid.NewGuid(),
                    Dose = "single"
                }
            };
    }
}
