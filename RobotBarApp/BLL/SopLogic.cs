using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class SopLogic : ISopLogic
{
    private readonly ISopRepository _sopRepository;
    
    public SopLogic(ISopRepository sopRepository)
    {
        _sopRepository = sopRepository;
    }
    
    public void AddSop(String name, String image, List<SopStep> sopSteps)
    {
        if (String.IsNullOrEmpty(name) )
        {
            throw new ArgumentException("Name cannot be null or empty");
        }
        if (String.IsNullOrEmpty(image) )
        {
            throw new ArgumentException("Image cannot be null or empty");
        }
        if (sopSteps == null || sopSteps.Count <= 0)
        {
            throw new ArgumentException("SopSteps cannot be null or empty");
        }
        var sop = new Sop
        {
            SopId = Guid.NewGuid(),
            Name = name,
            Image = image,
            SopSteps = sopSteps
        };
        _sopRepository.AddSop(sop);
    }
    
    public void DeleteSop(Guid sopId)
    {
        var sop = _sopRepository.GetSopById(sopId);
        if (sop == null)
        {
            throw new KeyNotFoundException($"SOP with ID {sopId} not found.");
        }
        _sopRepository.DeleteSop(sop);
    }
    
    public IEnumerable<Sop> GetAllSops()
    {
        return _sopRepository.GetAllSops();
    }
    
    public Sop? GetSopById(Guid sopId)
    {
        return _sopRepository.GetSopById(sopId);
    }
    
    public void UpdateSop(String name, String image, List<SopStep> sopSteps, Guid sopId)
    {
        if (String.IsNullOrEmpty(name) )
        {
            throw new ArgumentException("Name cannot be null or empty");
        }
        if (String.IsNullOrEmpty(image) )
        {
            throw new ArgumentException("Image cannot be null or empty");
        }
        if (sopSteps == null || sopSteps.Count <= 0)
        {
            throw new ArgumentException("SopSteps cannot be null or empty");
        }
        
        var sop = _sopRepository.GetSopById(sopId);
        if (sop == null)
        {
            throw new KeyNotFoundException($"SOP with ID {sopId} not found.");
        }
        sop.Name = name;
        sop.Image = image;
        sop.SopSteps = sopSteps;
        _sopRepository.UpdateSop(sop);
    }

    
}