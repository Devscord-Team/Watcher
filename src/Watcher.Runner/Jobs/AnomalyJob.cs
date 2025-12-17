using Watcher.Runner.Domain.AnomalyDetection;

namespace Watcher.Runner.Jobs
{
    public class AnomalyJob(IAnomalyDetector anomalyDetector)
    {
        public async Task Run()
        {
            //TODO
        }
    }
}
