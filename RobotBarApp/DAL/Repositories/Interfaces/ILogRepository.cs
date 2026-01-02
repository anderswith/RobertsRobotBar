using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface ILogRepository
{
    void AddLog(Log log);
    IEnumerable<Log> GetCommunicationLogsInTimeFrame(DateTime startTime, DateTime endTime);
    IEnumerable<Log> GetAllLogs();
    IEnumerable<Log> GetLogsInTimeFrame(Guid eventId, DateTime start, DateTime end);
    List<Log> GetLogsForEvent(Guid eventId);
}