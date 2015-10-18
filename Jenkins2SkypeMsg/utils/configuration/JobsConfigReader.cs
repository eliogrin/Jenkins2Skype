﻿using Jenkins2SkypeMsg.utils.configuration.notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace Jenkins2SkypeMsg.utils.configuration
{
    class JobsConfigReader
    {
        static private XmlDocument readConfigFile(String path)
        {
            XmlDocument configXml = new XmlDocument();
            try
            {
                configXml.Load(path);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("-- can't read configuration file: " + ex.Message);
            }
            return configXml;
        }

        static public Boolean checkConfigFile(String path)
        {
            bool result = false;
            XmlDocument configXml = new XmlDocument();
            try
            {
                configXml.Load(path);
                result = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("-- can't read configuration file: " + ex.Message);
            }
            return result;
        }

        static public List<JobConfiguration> prepareJobs(String path)
        {
            XmlDocument configXml = readConfigFile(path);

            List<JobConfiguration> jobsConfig = new List<JobConfiguration>();
            
            Trace.Write("++ done, loaded: ");
            foreach (XmlNode jobNode in configXml.SelectNodes("//job"))
            {
                JobConfiguration config = new JobConfiguration();
                config.enabled = true;
                config.activeMessaging = true;

                config.name = getValue(jobNode, "ciUri", "name");
                config.url = getValue(jobNode, "ciUri", "url");
                config.timeout = Convert.ToInt64(getValue(jobNode, "ciUri", "timeout"));
                config.onTime = getValue(jobNode, "ciUri", "onTime");
                config.onDate = getValue(jobNode, "ciUri", "onDate");

                config.messengerChatId = getValue(jobNode, "messenger", "chatId");

                XmlNode jobMonitoring = jobNode.SelectSingleNode("notifications");
                
                XmlNode buildStatusNode = jobMonitoring.SelectSingleNode("buildStatusCnahged");
                config.bldStatusChanged = Convert.ToBoolean(getValue(buildStatusNode, "enabled"));
                config.buildStatusTopicChange = Convert.ToBoolean(getValue(buildStatusNode, "topicChange"));
                if (config.bldStatusChanged)
                {
                    XmlNodeList jobStatusNodes = buildStatusNode.SelectNodes("status");
                    List<BuildStatusConfig> buildStatuses = new List<BuildStatusConfig>(jobStatusNodes.Count);

                    String defaultMessageSend = getValue(buildStatusNode, "messageSend");
                    String defaultMessageText = getValue(buildStatusNode, "defaultMessage");
                    String defaultTopicChange = getValue(buildStatusNode, "topicChange");
                    String defaultTopicText = getValue(buildStatusNode, "defaultTopic");
                    
                    foreach(XmlNode jobStatusNode in jobStatusNodes)
                    {
                        BuildStatusConfig buildStatus = new BuildStatusConfig();

                        buildStatus.type = getValue(jobStatusNode, "type");

                        buildStatus.messageSend = Convert.ToBoolean(tryGetValue(jobStatusNode, "messageSend", defaultMessageSend));
                        buildStatus.messageText = tryGetValue(jobStatusNode, "message", defaultMessageText);

                        buildStatus.topicChange = Convert.ToBoolean(tryGetValue(jobStatusNode, "topicChange", defaultTopicChange));
                        buildStatus.topicText = tryGetValue(jobStatusNode, "topic", defaultTopicText);

                        buildStatus.participantMsg = getValue(jobStatusNode, "participantMsg");
                        buildStatus.logMsg = getValue(jobStatusNode, "logMsg");

                        buildStatuses.Add(buildStatus);
                    }                    
                    config.bldStatusChangedConfigs = buildStatuses;
                }

                XmlNode buildStillRedNode = jobMonitoring.SelectSingleNode("buildStillRed");
                config.bldStillRed = Convert.ToBoolean(getValue(buildStillRedNode, "enabled"));
                if (config.bldStillRed)
                {
                    BuildStillRedConfig buildStillRedConfig = new BuildStillRedConfig();
                    buildStillRedConfig.statuses = getValue(buildStillRedNode, "type");
                    buildStillRedConfig.message = getValue(buildStillRedNode, "message");
                    buildStillRedConfig.subJobChangedMsg = getValue(buildStillRedNode, "subJobChangedMsg");
                    buildStillRedConfig.participantMsg = getValue(buildStillRedNode, "participantMsg");
                    buildStillRedConfig.subJobClaimed = getValue(buildStillRedNode, "subJobClaimed");
                    buildStillRedConfig.subJobNotClaimed = getValue(buildStillRedNode, "subJobNotClaimed");
                    config.bldStillRedConfig = buildStillRedConfig;
                }

                XmlNode buildFrozen = jobMonitoring.SelectSingleNode("buildFrozen");
                config.bldFroze = Convert.ToBoolean(getValue(buildFrozen, "enabled"));
                if (config.bldFroze)
                {
                    config.bldFrozenTimeout = Convert.ToInt32(getValue(buildFrozen, "timeout"));
                    config.bldFrozenMessage = getValue(buildFrozen, "message");
                }

                XmlNode dailyReport = jobMonitoring.SelectSingleNode("dailyReport");
                config.dailyReport = Convert.ToBoolean(getValue(dailyReport, "enabled"));
                if (config.dailyReport)
                {
                    DailyReportConfig dailyReportConfig = new DailyReportConfig();
                    dailyReportConfig.dailyTimeFrom = getValue(dailyReport, "timeFrom");
                    dailyReportConfig.dailyTimeTo = getValue(dailyReport, "timeTo");
                    
                    XmlNodeList messageNodes = dailyReport.SelectNodes("achieve");
                    List<DailyMessagesConfig> messages = new List<DailyMessagesConfig>(messageNodes.Count);
                    foreach (XmlNode messageNode in messageNodes)
                    {
                        DailyMessagesConfig messageConfig = new DailyMessagesConfig();

                        messageConfig.type = getValue(messageNode, "type");
                        messageConfig.condition = getValue(messageNode, "condition");
                        String minToGet = getValue(messageNode, "minToGet");
                        if (!String.IsNullOrEmpty(minToGet))
                            messageConfig.minToGet = Convert.ToInt16(getValue(messageNode, "minToGet"));
                        String maxToGet = getValue(messageNode, "maxToGet");
                        if (!String.IsNullOrEmpty(maxToGet))
                            messageConfig.maxToGet = Convert.ToInt16(getValue(messageNode, "maxToGet"));
                        messageConfig.message = getValue(messageNode, "message");

                        messages.Add(messageConfig);
                    }
                    dailyReportConfig.messageConfig = messages;

                    config.dailyReportConfig = dailyReportConfig;
                }

                XmlNode groupStatusNode = jobMonitoring.SelectSingleNode("groupStatusMonitoring");
                config.grpStatusMonitoring = Convert.ToBoolean(getValue(groupStatusNode, "enabled"));
                if (config.grpStatusMonitoring)
                {
                    XmlNodeList jobStatusNodes = groupStatusNode.SelectNodes("status");
                    List<BuildStatusConfig> buildStatuses = new List<BuildStatusConfig>(jobStatusNodes.Count);

                    String defaultTopicChange = getValue(groupStatusNode, "topicChange");
                    String defaultTopicText = getValue(groupStatusNode, "defaultTopic");

                    foreach (XmlNode jobStatusNode in jobStatusNodes)
                    {
                        BuildStatusConfig buildStatus = new BuildStatusConfig();

                        buildStatus.type = getValue(jobStatusNode, "type");

                        buildStatus.topicChange = Convert.ToBoolean(tryGetValue(jobStatusNode, "topicChange", defaultTopicChange));
                        buildStatus.topicText = tryGetValue(jobStatusNode, "topic", defaultTopicText);

                        buildStatuses.Add(buildStatus);
                    }
                    config.grpStatusMonitoringConfigs = buildStatuses;
                }

                //other
                config.lastBuildStatus = "success";

                Trace.Write("'" + config.name + "', ");
                jobsConfig.Add(config);
            }
            Trace.WriteLine(" jobs");
            return jobsConfig;
        }
        
        static private String getValue(XmlNode node, String attribute)
        {
            String value = null;
            if (node != null)
            {
                XmlAttribute xmlAttribute = node.Attributes[attribute];
                if (xmlAttribute != null)
                {
                    value = xmlAttribute.Value;
                }
            }
            return value;
        }
        
        static private String getValue(XmlNode parentNode, String requiredNode, String attribute)
        {
            XmlNode node = parentNode.SelectSingleNode(requiredNode);
            return getValue(node, attribute);
        }
        
        static private String tryGetValue(XmlNode node, String attribute, String defaultValue)
        {
            String text = getValue(node, attribute);
            if (text == null)
            {
                text = defaultValue;
            }
            return text;
        }
    }
}
