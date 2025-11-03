using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface ISopLogic
{
    void AddSop(String name, String image, List<SopStep> sopSteps);
    void DeleteSop(Guid sopId);
    IEnumerable<Sop> GetAllSops();
    Sop? GetSopById(Guid sopId);
    void UpdateSop(String name, String image, List<SopStep> sopSteps, Guid sopId);

}