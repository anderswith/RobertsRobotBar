using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface ILogLogic
{
    void AddLog(string type, string message);
    IEnumerable<Log> GetAllLogs();
    IEnumerable<Log> GetLogsByType(string type);
    IEnumerable<Log> GetLogsInTimeFrame(DateTime start, DateTime end);
    IEnumerable<Log> GetLogsByTypeInTimeFrame(string type, DateTime start, DateTime end);
}