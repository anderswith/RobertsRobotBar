using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface ILogRepository
{
    void AddLog(Log log);
    IEnumerable<Log> GetAllLogs();
    IEnumerable<Log> GetLogsByType(string type);
    IEnumerable<Log> GetLogsInTimeFrame(DateTime start, DateTime end);
    IEnumerable<Log> GetLogsByTypeInTimeFrame(string type, DateTime start, DateTime end);
}