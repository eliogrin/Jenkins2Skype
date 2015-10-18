using Jenkins2SkypeMsg.utils.CI.jenkins.connectors;
using Jenkins2SkypeMsg.utils.configuration.notifications;
using System;
using System.Collections.Generic;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.handlers
{
    class GroupStatusMonitoring
    {
        private ViewConnector group;
        private String[,] groupStatuses;

        private String failedBuilds = "";
        private String status;
        private BuildStatusConfig config;

        public GroupStatusMonitoring(String url)
        {
            this.group = new ViewConnector(url);
            if (this.group.isValid())
                this.groupStatuses = group.getViews();
        }

        public Boolean isValid()
        {
            return group.isValid();
        }

        public void prepareData(List<BuildStatusConfig> configs)
        {
            List<String> listOfStatuses = new List<string>();
            for (int row = 0; row < groupStatuses.GetLength(0); row++)
            {
                String currentStatus = groupStatuses[row, ViewConnector.jobStatus].Split('_')[0];
                listOfStatuses.Add(currentStatus);

                if(ViewConnector.redPattern.Contains(currentStatus))
                {
                    if (!String.IsNullOrEmpty(failedBuilds))
                    {
                        failedBuilds += ", ";
                    }
                    failedBuilds += groupStatuses[row, ViewConnector.jobName].Split('_')[1].ToUpper();
                }
            }
            status = getEpicStatus(listOfStatuses);

            foreach(BuildStatusConfig statusConfig in configs)
            {
                if (statusConfig.type.Contains(status))
                {
                    config = statusConfig;
                }
            }
        }

        public String getStatus()
        {
            return status;
        }

        public String getFailedBuilds()
        {
            return failedBuilds;
        }

        public String getFormatedMessage()
        {
            String message = config.topicText;
            if (!String.IsNullOrEmpty(failedBuilds) && message.Contains("{0}"))
            {
                message = String.Format(message, failedBuilds);
            }
            return message;
        }

        private String getEpicStatus(List<String> statuses)
        {
            String status;
            
            if (statuses.Contains("red"))
                status = "red";
            else if (statuses.Contains("yellow"))
                status = "yellow";
            else if (statuses.Contains("blue"))
                status = "blue";
            else if (statuses.Contains("green"))
                status = "green";
            else status = "unknown";

            return status;
        }
    }
}
