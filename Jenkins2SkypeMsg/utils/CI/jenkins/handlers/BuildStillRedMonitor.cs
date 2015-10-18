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
        private LogConnector log;

        public BuildStillRedMonitor(BuildConnector build, LogConnector log)
        {
            this.build = build;
            this.log = log;
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

                if (!String.IsNullOrEmpty(log.getFailure()))
                {
                    if (!String.IsNullOrEmpty(srConfig.subJobChangedMsg))
                    {
                        String formatedSubJobs = getFormatedSubJobs(log.getFailure(), config.lastFailedSubJobs);
                        if (!String.IsNullOrEmpty(formatedSubJobs))
                            message += String.Format(srConfig.subJobChangedMsg, formatedSubJobs);
                    }

                    if (!String.IsNullOrEmpty(srConfig.subJobClaimed)
                        || !String.IsNullOrEmpty(srConfig.subJobNotClaimed))
                    {
                        foreach (String subJob in log.getFailure().Split(','))
                        {
                            if (!String.IsNullOrEmpty(subJob))
                            {
                                String subJobUrl = WebUtils.getUrlFromJob(config.url, subJob);
                                BuildConnector subJobBuild = new BuildConnector(subJobUrl);
                                String claimedPerson = subJobBuild.getClaimedPerson();
                                String subJobName = subJob.Split('#')[0].Trim();
                                if (String.IsNullOrEmpty(claimedPerson))
                                {
                                    if (!String.IsNullOrEmpty(config.bldStillRedConfig.subJobNotClaimed))
                                    {
                                        message += String.Format(config.bldStillRedConfig.subJobNotClaimed, subJobName);
                                    }
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(config.bldStillRedConfig.subJobClaimed))
                                    {
                                        message += String.Format(config.bldStillRedConfig.subJobClaimed,
                                            subJobName, claimedPerson);
                                    }
                                }
                            }
                        }
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