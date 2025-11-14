using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class LogLogic : ILogLogic
{
    private readonly ILogRepository _logRepository;

    public LogLogic(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public void AddLog(string logMsg, string type)
    {
        if (string.IsNullOrEmpty(logMsg))
        {
            throw new ArgumentException("Log message cannot be null or empty");
        }

        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Log type cannot be null or empty");
        }

        Log log = new Log
        {
            LogId = Guid.NewGuid(),
            LogMsg = logMsg,
            Type = type,
            TimeStamp = DateTime.Now
        };

        _logRepository.AddLog(log);
    }
    
    public IEnumerable<Log> GetAllLogs()
    {
        return _logRepository.GetAllLogs();
    }
    public IEnumerable<Log> GetLogsByType(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Log type cannot be null or empty");
        }

        return _logRepository.GetLogsByType(type);
    }
    public IEnumerable<Log> GetLogsInTimeFrame(DateTime start, DateTime end)
    {
        if (start == default || end == default)
        {
            throw new ArgumentException("Start and end times must be specified");
        }
        if (start > end)
        {
            throw new ArgumentException("Start time must be earlier than end time");
        }

        return _logRepository.GetLogsInTimeFrame(start, end);
    }
    public IEnumerable<Log> GetLogsByTypeInTimeFrame(string type, DateTime start, DateTime end)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Log type cannot be null or empty");
        }
        if(start == default || end == default)
        {
            throw new ArgumentException("Start and end times must be specified");
        }
        if (start > end)
        {
            throw new ArgumentException("Start time must be earlier than end time");
        }

        return _logRepository.GetLogsByTypeInTimeFrame(type, start, end);
    }

}

