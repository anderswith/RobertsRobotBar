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

        // -----------------------------------------------------------
        // GetAllDrinks
        // -----------------------------------------------------------

        [Test]
        public void GetAllDrinks_ReturnsRepositoryResult()
        {
            var list = new List<Drink>
            {
                new Drink { DrinkId = Guid.NewGuid(), Name = "A" },
                new Drink { DrinkId = Guid.NewGuid(), Name = "B" }
            };
            _repo.Setup(r => r.GetAllDrinks()).Returns(list);

            var result = _logic.GetAllDrinks();

            Assert.That(result, Is.EqualTo(list));
        }

        // -----------------------------------------------------------
        // GetDrinkById
        // -----------------------------------------------------------

        [Test]
        public void GetDrinkById_EmptyGuid_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => _logic.GetDrinkById(Guid.Empty));
            Assert.That(ex!.Message, Is.EqualTo("Invalid drink ID."));
        }

        [Test]
        public void GetDrinkById_ValidGuid_ReturnsRepoValue()
        {
            var id = Guid.NewGuid();
            var d = new Drink { DrinkId = id, Name = "Mojito" };
            _repo.Setup(r => r.GetDrinkById(id)).Returns(d);

            var result = _logic.GetDrinkById(id);

            Assert.That(result, Is.EqualTo(d));
        }

        // -----------------------------------------------------------
        // AddDrink – validation
        // -----------------------------------------------------------

        [TestCase(null)]
        [TestCase("")]
        public void AddDrink_InvalidName_Throws(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink(name!, "img.png", true, new List<Guid> { Guid.NewGuid() }, new List<string> { "S1" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink name cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddDrink_InvalidImage_Throws(string? image)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink("Name", image!, true, new List<Guid> { Guid.NewGuid() }, new List<string> { "S1" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink image cannot be null or empty."));
        }

        [Test]
        public void AddDrink_NullIngredients_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink("Name", "img.png", false, null!, new List<string> { "S1" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one ingredient."));
        }

        [Test]
        public void AddDrink_EmptyIngredients_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink("Name", "img.png", false, new List<Guid>(), new List<string> { "S1" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one ingredient."));
        }

        [Test]
        public void AddDrink_NullScripts_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink("Name", "img.png", false, new List<Guid> { Guid.NewGuid() }, null!));
            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one script."));
        }

        [Test]
        public void AddDrink_EmptyScripts_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink("Name", "img.png", false, new List<Guid> { Guid.NewGuid() }, new List<string>()));
            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one script."));
        }
    
        [Test]
        public void AddDrink_ShouldThrow_WhenScriptNameIsWhitespaceOrNull()
        {
            // Arrange
            var ingredientIds = new List<Guid> { Guid.NewGuid() };
            var invalidScriptNames = new List<string> { "   ", "\t", null };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.AddDrink("Mojito", "mojito.png", false, ingredientIds, invalidScriptNames));

            Assert.That(ex!.Message, Is.EqualTo("Script name cannot be null or whitespace."));
        }


        // -----------------------------------------------------------
        // AddDrink – success + structure
        // -----------------------------------------------------------

        [Test]
        public void AddDrink_Valid_CreatesDrinkWithContentsAndScripts_AndCallsRepo()
        {
            var ing1 = Guid.NewGuid();
            var ing2 = Guid.NewGuid();
            var scripts = new List<string> { "First", "Second", "Third" };

            Drink? saved = null;
            _repo.Setup(r => r.AddDrink(It.IsAny<Drink>()))
                 .Callback<Drink>(d => saved = d);

            _logic.AddDrink("Gin Tonic", "gt.png", false, new List<Guid> { ing1, ing2 }, scripts);

            _repo.Verify(r => r.AddDrink(It.IsAny<Drink>()), Times.Once);
            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.Name, Is.EqualTo("Gin Tonic"));
                Assert.That(saved.Image, Is.EqualTo("gt.png"));
                Assert.That(saved.IsMocktail, Is.False);
                Assert.That(saved.DrinkContents.Select(c => c.IngredientId),
                    Is.EquivalentTo(new[] { ing1, ing2 }));
                Assert.That(saved.DrinkScripts.Select(s => s.UrScript),
                    Is.EquivalentTo(scripts));
                Assert.That(saved.DrinkScripts.Select(s => s.Number),
                    Is.EquivalentTo(new[] { 1, 2, 3 })); 
                Assert.That(saved.DrinkScripts.All(s => s.DrinkId == saved.DrinkId), Is.True);
            });
        }

        // -----------------------------------------------------------
        // DeleteDrink
        // -----------------------------------------------------------

        [Test]
        public void DeleteDrink_NotFound_Throws()
        {
            _repo.Setup(r => r.GetDrinkById(It.IsAny<Guid>())).Returns((Drink?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() => _logic.DeleteDrink(Guid.NewGuid()));
            Assert.That(ex!.Message, Is.EqualTo("Drink not found."));
        }

        [Test]
        public void DeleteDrink_Found_CallsRepoDelete()
        {
            var d = new Drink { DrinkId = Guid.NewGuid(), Name = "DelMe" };
            _repo.Setup(r => r.GetDrinkById(d.DrinkId)).Returns(d);

            _logic.DeleteDrink(d.DrinkId);

            _repo.Verify(r => r.DeleteDrink(d), Times.Once);
        }

        // -----------------------------------------------------------
        // UpdateDrink – validation
        // -----------------------------------------------------------

        [TestCase(null)]
        [TestCase("")]
        public void UpdateDrink_InvalidName_Throws(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.UpdateDrink(Guid.NewGuid(), name!, "img.png", true,
                    new List<Guid> { Guid.NewGuid() }, new List<string> { "S1" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink name cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateDrink_InvalidImage_Throws(string? image)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.UpdateDrink(Guid.NewGuid(), "Name", image!, true,
                    new List<Guid> { Guid.NewGuid() }, new List<string> { "S1" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink image cannot be null or empty."));
        }

        [Test]
        public void UpdateDrink_NullIngredients_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.UpdateDrink(Guid.NewGuid(), "Name", "img.png", true, null!, new List<string> { "S1" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one ingredient."));
        }

        [Test]
        public void UpdateDrink_EmptyIngredients_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.UpdateDrink(Guid.NewGuid(), "Name", "img.png", true, new List<Guid>(), new List<string> { "S1" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one ingredient."));
        }

        [Test]
        public void UpdateDrink_NullScripts_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.UpdateDrink(Guid.NewGuid(), "Name", "img.png", true, new List<Guid> { Guid.NewGuid() }, null!));
            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one script."));
        }

        [Test]
        public void UpdateDrink_EmptyScripts_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.UpdateDrink(Guid.NewGuid(), "Name", "img.png", true, new List<Guid> { Guid.NewGuid() }, new List<string>()));
            Assert.That(ex!.Message, Is.EqualTo("Drink must have at least one script."));
        }

        // -----------------------------------------------------------
        // UpdateDrink – not found
        // -----------------------------------------------------------

        [Test]
        public void UpdateDrink_NotFound_Throws()
        {
            _repo.Setup(r => r.GetDrinkById(It.IsAny<Guid>())).Returns((Drink?)null);

            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _logic.UpdateDrink(Guid.NewGuid(), "Name", "img.png", false,
                    new List<Guid> { Guid.NewGuid() }, new List<string> { "ScriptA" }));
            Assert.That(ex!.Message, Is.EqualTo("Drink not found."));
        }

        // -----------------------------------------------------------
        // UpdateDrink – collection replacement (ingredients + scripts)
        // -----------------------------------------------------------

        [Test]
        public void UpdateDrink_ReplacesIngredientsAndScripts_AsExpected_AndCallsRepoUpdate()
        {
            // Existing drink with:
            // Ingredients: A, B
            // Scripts: "S1"(#1), "S2"(#2)
            var idA = Guid.NewGuid();
            var idB = Guid.NewGuid();
            var drink = new Drink
            {
                DrinkId = Guid.NewGuid(),
                Name = "Old",
                Image = "old.png",
                IsMocktail = true,
                DrinkContents = new List<DrinkContent>
                {
                    new DrinkContent { DrinkContentId = Guid.NewGuid(), DrinkId = Guid.NewGuid(), IngredientId = idA },
                    new DrinkContent { DrinkContentId = Guid.NewGuid(), DrinkId = Guid.NewGuid(), IngredientId = idB },
                },
                DrinkScripts = new List<DrinkScript>
                {
                    new DrinkScript { ScriptId = Guid.NewGuid(), DrinkId = Guid.NewGuid(), UrScript = "S1", Number = 1 },
                    new DrinkScript { ScriptId = Guid.NewGuid(), DrinkId = Guid.NewGuid(), UrScript = "S2", Number = 2 },
                }
            };

            _repo.Setup(r => r.GetDrinkById(drink.DrinkId)).Returns(drink);

            // New desired state:
            // Ingredients: keep A, remove B, add C
            // Scripts: keep S1, remove S2, add S3, S4
            var idC = Guid.NewGuid();
            var newIngredients = new List<Guid> { idA, idC };
            var newScripts = new List<string> { "S1", "S3", "S4" };

            Drink? updated = null;
            _repo.Setup(r => r.UpdateDrink(It.IsAny<Drink>()))
                 .Callback<Drink>(d => updated = d);

            _logic.UpdateDrink(drink.DrinkId, "New", "new.png", false, newIngredients, newScripts);

            _repo.Verify(r => r.UpdateDrink(It.IsAny<Drink>()), Times.Once);
            Assert.That(updated, Is.Not.Null);

            // Validate basic fields
            Assert.Multiple(() =>
            {
                Assert.That(updated!.Name, Is.EqualTo("New"));
                Assert.That(updated.Image, Is.EqualTo("new.png"));
                Assert.That(updated.IsMocktail, Is.False);
            });

            // Validate ingredients
            var ingIds = updated!.DrinkContents.Select(x => x.IngredientId).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(ingIds, Has.Count.EqualTo(2));
                Assert.That(ingIds, Does.Contain(idA));
                Assert.That(ingIds, Does.Contain(idC));
                Assert.That(ingIds, Does.Not.Contain(idB));
            });

            // Validate scripts
            var scriptNames = updated.DrinkScripts.Select(s => s.UrScript).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(scriptNames, Has.Count.EqualTo(3));
                Assert.That(scriptNames, Does.Contain("S1")); // kept
                Assert.That(scriptNames, Does.Contain("S3")); // added
                Assert.That(scriptNames, Does.Contain("S4")); // added
                Assert.That(scriptNames, Does.Not.Contain("S2")); // removed
            });

            // Validate numbering rule: new ones appended after max existing
            // After removal, remaining is only "S1" (#1). So new ones should be #2, #3.
            var s1 = updated.DrinkScripts.Single(s => s.UrScript == "S1");
            var s3 = updated.DrinkScripts.Single(s => s.UrScript == "S3");
            var s4 = updated.DrinkScripts.Single(s => s.UrScript == "S4");

            Assert.Multiple(() =>
            {
                Assert.That(s1.Number, Is.EqualTo(1));  // kept old number
                Assert.That(s3.Number, Is.EqualTo(2));  // appended after max remaining
                Assert.That(s4.Number, Is.EqualTo(3));
            });
        }

        // -----------------------------------------------------------
        // UpdateDrink – throw if script list include whitespace items
        // -----------------------------------------------------------
        [Test]
        public void UpdateDrink_ShouldThrow_WhenScriptNameIsWhitespaceOrNull()
        {
            // Arrange
            var existingDrink = new Drink
            {
                DrinkId = Guid.NewGuid(),
                Name = "Old Fashioned",
                Image = "old.png",
                IsMocktail = false,
                DrinkContents = new List<DrinkContent>
                {
                    new DrinkContent { IngredientId = Guid.NewGuid() }
                },
                DrinkScripts = new List<DrinkScript>
                {
                    new DrinkScript { ScriptId = Guid.NewGuid(), UrScript = "OldScript", Number = 1 }
                }
            };

            _repo.Setup(r => r.GetDrinkById(existingDrink.DrinkId)).Returns(existingDrink);

            var newIngredientIds = new List<Guid> { existingDrink.DrinkContents.First().IngredientId };
            var invalidScriptNames = new List<string> { "   ", null, "\n" };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _logic.UpdateDrink(existingDrink.DrinkId, "Updated", "updated.png", false, newIngredientIds, invalidScriptNames));

            Assert.That(ex!.Message, Is.EqualTo("Script name cannot be null or whitespace."));
        }

        
    }
}
