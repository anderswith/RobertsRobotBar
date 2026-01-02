using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface ILogLogic
{
    void AddLog(string type, string message);
    IEnumerable<Log> getAllCommunicationLogs();
    IEnumerable<Log> GetLogsByType(string type);
    IEnumerable<Log> GetCommunicationLogsInTimeFrame(DateTime startTime, DateTime endTime);
    IEnumerable<Log> GetLogsInTimeFrame(Guid eventId, DateTime start, DateTime end);
    IEnumerable<Log> GetLogsByTypeInTimeFrame(Guid eventId, string type, DateTime start, DateTime end);
    IEnumerable<Log> GetLogsForEvent(Guid eventId);
    void AddEventLog(string logMsg, string type);
}