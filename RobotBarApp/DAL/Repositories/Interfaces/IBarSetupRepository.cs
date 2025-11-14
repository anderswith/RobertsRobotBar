using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IBarSetupRepository
{
    void addBarSetup(BarSetup barSetup);
    IEnumerable<BarSetup> GetAllBarSetupsForEventById(Guid eventId);
    void updateBarSetup(BarSetup barSetup);
    void deleteBarSetup(BarSetup barSetup);
    BarSetup? GetBarSetupEventAndPosition(Guid eventId, int positionNumber);

}