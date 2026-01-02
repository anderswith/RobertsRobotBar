using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;
using RobotBarApp.Services.Application.Interfaces;

namespace RobotBarApp.BLL;

public class LogLogic : ILogLogic
{
    private readonly ILogRepository _logRepository;
    private readonly IEventSessionService _eventSessionService;

    public LogLogic(ILogRepository logRepository, IEventSessionService eventSessionService)
    {
        _logRepository = logRepository;
        _eventSessionService = eventSessionService;
    }
    
    public void AddEventLog(string logMsg, string type)
    {
        var eventId = _eventSessionService.CurrentEventId;
        
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
    
    public IEnumerable<Log> getAllCommunicationLogs()
    {
        return _logRepository.GetAllLogs().Where(Log => Log.EventId == null);
    }

    public IEnumerable<Log> GetCommunicationLogsInTimeFrame(DateTime startTime, DateTime endTime)
    {
        if (startTime == default || endTime == default)
        {
            throw new ArgumentException("Start and end times must be specified");
        }
        if (startTime > endTime)
        {
            throw new ArgumentException("Start time must be earlier than end time");
        }
        return _logRepository.GetCommunicationLogsInTimeFrame(startTime, endTime);
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
    
    public IEnumerable<Log> GetLogsForEvent(Guid eventId)
    {
        if(Guid.Empty == eventId)
        {
            throw new ArgumentException("Event ID must be specified");
        }
        return _logRepository.GetLogsForEvent(eventId);
    }
   

}

