using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TaskProcessing.Functions.Functions;

public class TimerTriggerFunction(ILogger<TimerTriggerFunction> logger)
{
    // Fires every minute: {second} {minute} {hour} {day} {month} {day-of-week}
    [Function("CleanupTimer")]
    public void Run([TimerTrigger("0 */1 * * * *", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("Timer trigger fired at {Time}", DateTime.UtcNow);

        if (timer.ScheduleStatus is not null)
        {
            logger.LogInformation("Next scheduled run: {Next}", timer.ScheduleStatus.Next);
        }

        // In a real app: query DB for completed tasks older than 30 days and delete them
        logger.LogInformation("Cleanup job completed");
    }
}
