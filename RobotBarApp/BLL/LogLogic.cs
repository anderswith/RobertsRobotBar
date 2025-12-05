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
    
    public void AddEventLog(Guid eventId, string logMsg, string type)
    {
        if (string.IsNullOrEmpty(logMsg))
        {
            throw new ArgumentException("Log message cannot be null or empty");
        }

        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Log type cannot be null or empty");
        }
        if(Guid.Empty == eventId)
        {
            throw new ArgumentException("Event ID must be specified");
        }

        Log log = new Log
        {
            LogId = Guid.NewGuid(),
            TimeStamp = DateTime.Now,
            LogMsg = logMsg,
            Type = type,
            EventId = eventId
        };

        _logRepository.AddLog(log);
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
            TimeStamp = DateTime.Now,
            LogMsg = logMsg,
            Type = type,
            
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
    public IEnumerable<Log> GetLogsInTimeFrame(Guid eventId,DateTime start, DateTime end)
    {
        if (start == default || end == default)
        {
            throw new ArgumentException("Start and end times must be specified");
        }
        if (start > end)
        {
            throw new ArgumentException("Start time must be earlier than end time");
        }
        if(Guid.Empty == eventId)
        {
            throw new ArgumentException("Event ID must be specified");
        }

        return _logRepository.GetLogsInTimeFrame(eventId, start, end);
    }
    public IEnumerable<Log> GetLogsByTypeInTimeFrame(Guid eventId, string type, DateTime start, DateTime end)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Log type cannot be null or empty");
        }
        if(Guid.Empty == eventId)
        {
            throw new ArgumentException("Event ID must be specified");
        }
        if(start == default || end == default)
        {
            throw new ArgumentException("Start and end times must be specified");
        }
        if (start > end)
        {
            throw new ArgumentException("Start time must be earlier than end time");
        }

        return _logRepository.GetLogsByTypeInTimeFrame(eventId, type, start, end);
    }
    
    public IEnumerable<Log> GetLogsForEvent(Guid eventId)
    {
        if(Guid.Empty == eventId)
        {
            throw new ArgumentException("Event ID must be specified");
        }

        return _logRepository.GetLogsForEvent(eventId);
    }
   

}

