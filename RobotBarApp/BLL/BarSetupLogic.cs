using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class BarSetupLogic : IBarSetupLogic
{
    private readonly IBarSetupRepository _barSetupRepository;
    public BarSetupLogic(IBarSetupRepository barSetupRepository)
    {
        _barSetupRepository = barSetupRepository;
    }
    
    public void AddBarSetup(int positionNumber, Guid ingredientId, Guid eventId)
    {
        if (positionNumber <= 0)
        {
            throw new ArgumentException("Position number must be greater than zero.");
        }

        if (ingredientId == Guid.Empty)
        {
            throw new ArgumentException("Ingredient ID cannot be empty.");
        }

        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty.");
        }

        var existing = _barSetupRepository.GetBarSetupEventAndPosition(eventId, positionNumber);

        if (existing != null)
        {
            existing.IngredientId = ingredientId;
            _barSetupRepository.updateBarSetup(existing);
        }
        else
        {
            var newSetup = new BarSetup
            {
                EventBarSetupId = Guid.NewGuid(),
                PositionNumber = positionNumber,
                IngredientId = ingredientId,
                EventId = eventId
            };

            _barSetupRepository.addBarSetup(newSetup);
        }
    }
    
    public void DeleteBarSetup(Guid eventId, int positionNumber)
    {
        var existing = _barSetupRepository.GetBarSetupEventAndPosition(eventId, positionNumber);

        if (existing != null)
        {
            _barSetupRepository.deleteBarSetup(existing);
        }
        else
        {
            throw new ArgumentException("No bar setup found for the given event and position.");
        }
    }
    
    public IEnumerable<BarSetup> GetBarSetupsForEvent(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty.");
        }

        return _barSetupRepository.GetAllBarSetupsForEventById(eventId);
    }

    
    
    
}