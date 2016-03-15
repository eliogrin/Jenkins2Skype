using Jenkins2SkypeMsg.utils;
using Jenkins2SkypeMsg.utils.CI.jenkins.connectors;
using Jenkins2SkypeMsg.utils.CI.jenkins.handlers;
using Jenkins2SkypeMsg.utils.configuration;
using Jenkins2SkypeMsg.utils.messenger;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;

namespace Jenkins2SkypeMsg.core
{
    class TaskExecutors
    {
        JobConfiguration config;
        String updateConfigStatus;

        public TaskExecutors(JobConfiguration configuration)
        {
            config = configuration;
            updateConfigStatus = ConfigurationManager.AppSettings["updateConfigStatus"];
        }

        public void planTask()
        {
            while (config.enabled)
            {
                Trace.WriteLine("++ at " + TimeUtils.getCurrentTime() + " thread " + Thread.CurrentThread.Name + " is waiting fox execute");
                int seconds = 0;
                long timeout = config.timeout;

                if (timeout == 0)
                {
                    timeout = TimeUtils.getSeconsToTime(config.onTime);
                }

                do
                {
                    Thread.Sleep(1000);
                    seconds++;
                } while (seconds < timeout);

                Trace.WriteLine("// thread " + Thread.CurrentThread.Name + " start executing after " + timeout + " seconds of waiting");
                config = Config.Instance.Find(x => x.name == config.name);
                executeJob();
                config = Config.Instance.Find(x => x.name == config.name);
                Trace.WriteLine("-- thread " + Thread.CurrentThread.Name + " is finish work");
            }
        }

        public void executeJob()
        {
            String status = Messenger.Instance.getStatus();

            if (status.Contains("Online") && config.activeMessaging && isWorkToday())
            {
                if (config.bldStatusChanged || config.bldFroze)
                {
                    BuildConnector lastBuild = new BuildConnector(config.url, true);
                    if (lastBuild.isValid())
                    {
                        BuildConnector finishedBuild;
                        Trace.WriteLine(String.Format("\\\\ Checking {0} #{1} with actual status: {2} and previous status {3}",
                                config.name, lastBuild.getNumber(), lastBuild.getStatus().ToUpper(), config.lastBuildStatus.ToUpper()));

                        if (config.bldFroze) buildFrozen(lastBuild);

                        if (lastBuild.getStatus() == BuildConnector.activeBuild)
                        {
                            int prevBildNumber = lastBuild.getNumber() - 1;
                            finishedBuild = new BuildConnector(config.url, prevBildNumber);
                        }
                        else
                        {
                            finishedBuild = lastBuild;
                        }
                        if (finishedBuild.isValid())
                        {
                            if (config.bldStatusChanged) buildStatusCnahged(finishedBuild);
                            if (config.bldStillRed) buildStillRed(finishedBuild);

                            updateConfig(finishedBuild);
                        }
                    }
                }
                if (config.dailyReport) dailyReport();
                if (config.grpStatusMonitoring) groupStatusMonitoring();
            }

            if (!String.IsNullOrEmpty(updateConfigStatus) && status.Contains(updateConfigStatus))
            {
                TaskLauncher.closeThreads();
                Messenger.Instance.goOnline();
            }
        }

        private void buildStatusCnahged(BuildConnector build)
        {
            if (!config.lastBuildStatus.Contains(build.getStatus()) && config.lastBuildNumber < build.getNumber())
            {
                BuildStatusMonitor monitor = new BuildStatusMonitor(build);
                monitor.prepareData(config.bldStatusChangedConfigs);
                if (monitor.isValid())
                {
                    if (config.buildStatusTopicChange)
                    {
                        String newTopic = monitor.getTopic();
                        String actualTopic = Messenger.Instance.getTopicName(config.messengerChatId);
                        if (!String.IsNullOrEmpty(newTopic) && newTopic != actualTopic)
                        {
                            newTopic = "/topic " + newTopic;
                            Messenger.Instance.sendMessage(config.messengerChatId, newTopic);
                        }
                    }

                    String messageText = monitor.getMessage();
                    if (!String.IsNullOrEmpty(messageText))
                        Messenger.Instance.sendMessage(config.messengerChatId, messageText);
                }
            }
        }

        private void buildStillRed(BuildConnector build)
        {
            if (build.getNumber() != config.lastBuildNumber)
            {
                if (config.lastBuildNumber != 0)
                {
                    BuildStillRedMonitor buildStillRedMonitor = new BuildStillRedMonitor(build);
                    String message = buildStillRedMonitor.getMessage(config);

                    if (!String.IsNullOrEmpty(message))
                        Messenger.Instance.sendMessage(config.messengerChatId, message);
                }
            }
        }

        private void buildFrozen(BuildConnector build)
        {
            if (build.getStatus() == BuildConnector.activeBuild)
            {
                long workingTime = TimeUtils.timestampToSeconds(TimeUtils.getTimestamp() - build.getTimestamp());
                if (workingTime > config.bldFrozenTimeout && build.getNumber() != config.lastFrozenBuild)
                {
                    workingTime = workingTime / 60;
                    Messenger.Instance.sendMessage(config.messengerChatId,
                        String.Format(config.bldFrozenMessage, build.getNumber(), workingTime));
                    Config.Instance.Find(x => x.name == config.name).lastFrozenBuild = build.getNumber();
                }
            }
        }

        private void dailyReport()
        {
            DailyReport dailyReport = new DailyReport(config.url);
            if (dailyReport.isValid())
            {
                long timeFrom = TimeUtils.getTimestamp(config.dailyReportConfig.dailyTimeFrom);
                long timeTo = TimeUtils.getTimestamp(config.dailyReportConfig.dailyTimeTo);
                dailyReport.optimizeData(timeFrom, timeTo);

                String message = dailyReport.generateMessages(config.dailyReportConfig);
                if (!String.IsNullOrEmpty(message))
                    Messenger.Instance.sendMessage(config.messengerChatId, message);
            }
        }

        private void groupStatusMonitoring()
        {
            GroupStatusMonitoring groupStatusMonitoring = new GroupStatusMonitoring(config.url);
            if (groupStatusMonitoring.isValid())
            {
                groupStatusMonitoring.prepareData(config.grpStatusMonitoringConfigs);
                String newTopic = groupStatusMonitoring.getFormatedMessage();
                String oldTopic = Messenger.Instance.getTopicName(config.messengerChatId);
                
                if (oldTopic != newTopic)
                {
                    newTopic = "/topic " + newTopic;
                    Messenger.Instance.sendMessage(config.messengerChatId, newTopic);
                }
            }
        }

        private void updateConfig(BuildConnector build)
        {
            if(build.getNumber() != config.lastBuildNumber)
            {
                if (!BuildConnector.abortedPattern.Contains(build.getStatus()))
                    Config.Instance.Find(x => x.name == config.name).lastBuildStatus = getStatusPattern(build.getStatus());
                Config.Instance.Find(x => x.name == config.name).lastBuildNumber = build.getNumber();
                Config.Instance.Find(x => x.name == config.name).lastFinishTime = build.getTimestamp() + build.getDuraction();
            }
        }

        private Boolean isWorkToday()
        {
            Boolean workToday = true;
            if (!String.IsNullOrEmpty(config.onDate))
            {
                if (!config.onDate.Contains(TimeUtils.getCurrentWeek()))
                    workToday = false;
            }
            return workToday;
        }

        private String getStatusPattern(String status)
        {
            String statusPattern = status;
            if (BuildConnector.abortedPattern.Contains(status))
                statusPattern = BuildConnector.abortedPattern;
            if (BuildConnector.greenPattern.Contains(status))
                statusPattern = BuildConnector.greenPattern;
            if (BuildConnector.redPattern.Contains(status))
                statusPattern = BuildConnector.redPattern;
            return statusPattern;
        }
    }
}
