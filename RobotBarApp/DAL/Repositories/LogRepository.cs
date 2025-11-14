using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.DAL;

public class LogRepository: ILogRepository
{
    private readonly RobotBarContext _context;
    
    public LogRepository(RobotBarContext context)
    {
        _context = context;
    }
    
    public void AddLog(Log log)
    {
        _context.Logs.Add(log);
        _context.SaveChanges();
    }
    
    public IEnumerable<Log> GetAllLogs()
    {
        return _context.Logs.ToList();
    }
    
    public IEnumerable<Log> GetLogsByType(string type)
    {
        return _context.Logs
            .Where(l => l.Type == type)
            .ToList();
    }
    
    public IEnumerable<Log> GetLogsInTimeFrame(DateTime start, DateTime end)
    {
        return _context.Logs
            .Where(l => l.TimeStamp >= start && l.TimeStamp <= end)
            .ToList();
    }
    
    public IEnumerable<Log> GetLogsByTypeInTimeFrame(string type, DateTime start, DateTime end)
    {
        return _context.Logs
            .Where(l => l.Type == type && l.TimeStamp >= start && l.TimeStamp <= end)
            .ToList();
    }
    
    
    
}