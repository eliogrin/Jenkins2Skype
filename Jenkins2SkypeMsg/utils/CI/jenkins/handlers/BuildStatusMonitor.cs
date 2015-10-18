using Jenkins2SkypeMsg.utils.CI.jenkins.connectors;
using Jenkins2SkypeMsg.utils.configuration.notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.handlers
{
    class BuildStatusMonitor
    {
        private BuildConnector build;
        private BuildStatusConfig config;
        private LogConnector log;
        private String failedJob;
        
        private Boolean validation = false;

        public BuildStatusMonitor(BuildConnector build)
        {
            this.build = build;
        }

        public void attachLog(LogConnector log)
        {
            this.log = log;
        }

        public void prepareData(List<BuildStatusConfig> configs)
        {
            foreach (BuildStatusConfig statusConfig in configs)
            {
                if (statusConfig.type == build.getStatus())
                {
                    this.config = statusConfig;
                    this.validation = true;
                }
            }
        }

        public Boolean isValid()
        {
            return validation;
        }

        public String getTopic()
        {
            String topic = null;
            if (config.topicChange)
            {
                topic += String.Format(config.topicText, build.getStatus());
            }
            return topic;
        }

        public String getMessage()
        {
            String message = null;
            if (config.messageSend)
            {
                message = String.Format(config.messageText, build.getStatus().ToUpper(), build.getNumber());
                message += getParticipant();
                message += getFailedJob();
                message += getLink();
            }
            return message;
        }

        private String getParticipant()
        {
            String participant = "";

            if(!String.IsNullOrEmpty(config.participantMsg))
            {
                String persons;
                String reason;

                if (BuildConnector.abortedPattern.Contains(build.getStatus()))
                {
                    persons = build.getAborted();
                    reason = "aborted by";
                }
                else
                {
                    persons = build.getInitiator();
                    reason = "started by";
                    if (String.IsNullOrEmpty(persons))
                    {
                        persons = build.getParticipant();
                        reason = "push of";
                    }
                }
                if (!String.IsNullOrEmpty(persons))
                {
                    persons = TextUtils.optimizeUserName(persons);
                    participant = String.Format(config.participantMsg, reason, persons);
                }
            }

            return participant;
        }

        private String getFailedJob()
        {
            String job = "";
            if (!String.IsNullOrEmpty(config.logMsg))
            {
                job = "\n   ";
                failedJob = log.getFailure();
                if (!String.IsNullOrEmpty(failedJob))
                    job += String.Format(config.logMsg, failedJob);
            }
            return job;
        }

        private String getLink()
        {
            String link = "\n   uri: ";

            if (String.IsNullOrEmpty(failedJob) || failedJob.Contains(", "))
            {
                link += build.getUrl();
            }
            else
            {
                link += WebUtils.getUrlFromJob(build.getUrl(), failedJob);
            }

            return link;
        }

    }
}
