using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface ISopRepository
{
    void AddSop(Sop sop);
    IEnumerable<Sop> GetAllSops();
    Sop? GetSopById(Guid sopId);
    void DeleteSop(Sop sop);
    void UpdateSop(Sop sop);
}