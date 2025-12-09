using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IBarSetupLogic
{ 
    void AddBarSetup(int positionNumber, Guid ingredientId, Guid eventId);
    void DeleteBarSetup(Guid eventId, int positionNumber);
    IEnumerable<BarSetup> GetBarSetupForEvent(Guid eventId);

}