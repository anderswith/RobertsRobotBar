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
    public class DrinkAvailabilityServiceTests
    {
        private Mock<IDrinkRepository> _drinkRepoMock;
        private Mock<IBarSetupRepository> _barSetupRepoMock;
        private DrinkAvailabilityService _service;

        [SetUp]
        public void Setup()
        {
            _drinkRepoMock = new Mock<IDrinkRepository>();
            _barSetupRepoMock = new Mock<IBarSetupRepository>();

            _service = new DrinkAvailabilityService(
                _drinkRepoMock.Object,
                _barSetupRepoMock.Object);
        }

        // ------------------------------------------------------------
        // Validation
        // ------------------------------------------------------------

        [Test]
        public void GetAvailableDrinksForEvent_Throws_WhenEventIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                _service.GetAvailableDrinksForEvent(Guid.Empty));

            Assert.That(ex!.Message, Is.EqualTo("Event ID cannot be empty."));
        }

        // ------------------------------------------------------------
        // Bar setup edge cases
        // ------------------------------------------------------------

        [Test]
        public void GetAvailableDrinksForEvent_ReturnsEmpty_WhenNoBarSetup()
        {
            var eventId = Guid.NewGuid();

            _barSetupRepoMock
                .Setup(r => r.GetBarSetupForEvent(eventId))
                .Returns(new List<BarSetup>());

            var result = _service.GetAvailableDrinksForEvent(eventId);

            Assert.That(result, Is.Empty);
        }

        // ------------------------------------------------------------
        // Filtering logic
        // ------------------------------------------------------------

        [Test]
        public void GetAvailableDrinksForEvent_FiltersOutDrink_WhenIngredientNotOnBar()
        {
            var eventId = Guid.NewGuid();
            var ingredientId = Guid.NewGuid();

            _barSetupRepoMock
                .Setup(r => r.GetBarSetupForEvent(eventId))
                .Returns(new List<BarSetup>()); // no ingredients on bar

            _drinkRepoMock
                .Setup(r => r.GetAllDrinksWithContentAndIngredientPositions())
                .Returns(new List<Drink>
                {
                    new Drink
                    {
                        DrinkId = Guid.NewGuid(),
                        DrinkContents = new List<DrinkContent>
                        {
                            new DrinkContent
                            {
                                IngredientId = ingredientId,
                                Ingredient = new Ingredient
                                {
                                    IngredientPositions = new List<IngredientPosition>
                                    {
                                        new IngredientPosition { Position = 1 }
                                    }
                                }
                            }
                        }
                    }
                });

            var result = _service.GetAvailableDrinksForEvent(eventId);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAvailableDrinksForEvent_FiltersOutDrink_WhenPositionDoesNotMatch()
        {
            var eventId = Guid.NewGuid();
            var ingredientId = Guid.NewGuid();

            _barSetupRepoMock
                .Setup(r => r.GetBarSetupForEvent(eventId))
                .Returns(new List<BarSetup>
                {
                    new BarSetup { IngredientId = ingredientId, PositionNumber = 2 }
                });

            _drinkRepoMock
                .Setup(r => r.GetAllDrinksWithContentAndIngredientPositions())
                .Returns(new List<Drink>
                {
                    new Drink
                    {
                        DrinkId = Guid.NewGuid(),
                        DrinkContents = new List<DrinkContent>
                        {
                            new DrinkContent
                            {
                                IngredientId = ingredientId,
                                Ingredient = new Ingredient
                                {
                                    IngredientPositions = new List<IngredientPosition>
                                    {
                                        new IngredientPosition { Position = 1 } // mismatch
                                    }
                                }
                            }
                        }
                    }
                });

            var result = _service.GetAvailableDrinksForEvent(eventId);

            Assert.That(result, Is.Empty);
        }

        // ------------------------------------------------------------
        // Happy path
        // ------------------------------------------------------------

        [Test]
        public void GetAvailableDrinksForEvent_ReturnsDrink_WhenAllIngredientsMatchPositions()
        {
            var eventId = Guid.NewGuid();
            var ingredientId = Guid.NewGuid();

            _barSetupRepoMock
                .Setup(r => r.GetBarSetupForEvent(eventId))
                .Returns(new List<BarSetup>
                {
                    new BarSetup { IngredientId = ingredientId, PositionNumber = 1 }
                });

            var drink = new Drink
            {
                DrinkId = Guid.NewGuid(),
                DrinkContents = new List<DrinkContent>
                {
                    new DrinkContent
                    {
                        IngredientId = ingredientId,
                        Ingredient = new Ingredient
                        {
                            IngredientPositions = new List<IngredientPosition>
                            {
                                new IngredientPosition { Position = 1 }
                            }
                        }
                    }
                }
            };

            _drinkRepoMock
                .Setup(r => r.GetAllDrinksWithContentAndIngredientPositions())
                .Returns(new List<Drink> { drink });

            var result = _service.GetAvailableDrinksForEvent(eventId).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(drink));
        }

        // ------------------------------------------------------------
        // Data integrity protection
        // ------------------------------------------------------------

        [Test]
        public void GetAvailableDrinksForEvent_Throws_WhenIngredientIsMissing()
        {
            var eventId = Guid.NewGuid();
            var ingredientId = Guid.NewGuid();

            _barSetupRepoMock
                .Setup(r => r.GetBarSetupForEvent(eventId))
                .Returns(new List<BarSetup>
                {
                    new BarSetup { IngredientId = ingredientId, PositionNumber = 1 }
                });

               _drinkRepoMock
                .Setup(r => r.GetAllDrinksWithContentAndIngredientPositions())
                .Returns(new List<Drink>
                {
                    new Drink
                    {
                        DrinkId = Guid.NewGuid(),
                        DrinkContents = new List<DrinkContent>
                        {
                            new DrinkContent
                            {
                                IngredientId = ingredientId,
                                Ingredient = null // invalid aggregate
                            }
                        }
                    }
                });

            Assert.Throws<InvalidOperationException>(() =>
                _service.GetAvailableDrinksForEvent(eventId));
        }
    }
}
