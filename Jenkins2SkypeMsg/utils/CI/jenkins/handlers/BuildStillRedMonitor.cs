using Jenkins2SkypeMsg.utils.CI.jenkins.connectors;
using Jenkins2SkypeMsg.utils.configuration;
using Jenkins2SkypeMsg.utils.configuration.notifications;
using System;
using System.Linq;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.handlers
{
    class BuildStillRedMonitor
    {
        private BuildConnector build;

        public BuildStillRedMonitor(BuildConnector build)
        {
            this.build = build;
        }

        public String getMessage(JobConfiguration config)
        {
            String message = "";

            BuildStillRedConfig srConfig = config.bldStillRedConfig;
            String actualStatus = build.getStatus();
            
            long actualFinishTime = build.getDuraction() + build.getTimestamp();

            if (srConfig.statuses.Contains(actualStatus) && config.lastBuildStatus.Contains(actualStatus))
            {
                if (!String.IsNullOrEmpty(srConfig.message))
                {
                    message = String.Format(srConfig.message, actualStatus.ToUpper(), build.getNumber());
                }

                if (!String.IsNullOrEmpty(srConfig.participantMsg))
                {
                    String participant = build.getParticipant();
                    if (!String.IsNullOrEmpty(participant))
                    {
                        message += String.Format(srConfig.participantMsg, TextUtils.optimizeUserName(participant));
                    }
                }
            }
            return message;
        }

        private String optimizeJobs(String jobs)
        {
            String result = "";
            if (!String.IsNullOrEmpty(jobs))
            {
                foreach (String job in jobs.Split(','))
                {
                    if (!String.IsNullOrEmpty(result))
                        result += ",";
                    result += job.Split('#').First().Trim();
                }
            }
            return result;
        }

        private String getNewItems(String searchItems, String searchFrom)
        {
            String result = "";
            foreach(String searchItem in searchItems.Split(','))
            {
                if (!searchFrom.Contains(searchItem))
                {
                    if (!String.IsNullOrEmpty(result))
                        result += ", ";
                    result += searchItem;
                }
            }
            return result;
        }

        private String getFormatedSubJobs(String actualFailedSubJob, String lastFailedSubJob)
        {
            String subJobFormated = "";
            String actualJobOptimized = optimizeJobs(actualFailedSubJob);
            String lastJobOptimized = optimizeJobs(lastFailedSubJob);

            if (actualJobOptimized != lastJobOptimized)
            {
                String fixedSubJobs = getNewItems(lastJobOptimized, actualJobOptimized);
                String newSubJobs = getNewItems(actualJobOptimized, lastJobOptimized);
                
                if (!String.IsNullOrEmpty(fixedSubJobs))
                {
                    subJobFormated += String.Format("{0} was fixed", fixedSubJobs);
                }
                if (!String.IsNullOrEmpty(newSubJobs))
                {
                    if (!String.IsNullOrEmpty(fixedSubJobs))
                        subJobFormated += " and ";
                    subJobFormated += String.Format("{0} was failed", newSubJobs);
                }
            }
            return subJobFormated;
        }
    }
}