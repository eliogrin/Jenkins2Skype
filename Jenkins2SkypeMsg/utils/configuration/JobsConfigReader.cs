using Jenkins2SkypeMsg.utils.configuration.notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;

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
            bool validationLoaded = false;
            XmlDocument configXml = new XmlDocument();
            try
            {
                configXml.Schemas.Add(null, @"files\config.xsd");
                validationLoaded = true;
            }
            catch (FileNotFoundException ex)
            {
                Trace.WriteLine("-- sorry, can't find validation file, so validation was skipped, with message: " + ex.Message);
            }
            try
            {
                bool validationResult = true;
                configXml.Load(path);
                if (validationLoaded)
                {
                    configXml.Validate((sender, vargs) =>
                    {
                        if (validationResult)
                        {
                            Trace.WriteLine("Sorry, but your config file is not valid:");
                            validationResult = false;
                        }
                        IXmlLineInfo info = sender as IXmlLineInfo;
                        Trace.WriteLine(String.Format("{0}: {1}; Line: {2}", vargs.Severity, vargs.Message, info != null ? info.LineNumber.ToString() : "not known"));
                    });
                }
                result = validationResult;
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
                
                XmlNode buildStatusNode = jobMonitoring.SelectSingleNode("buildStatusChanged");
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
                    String defaultParameterName = getValue(buildStatusNode, "defaultParameterName");
                    String defaultParameterMsg = getValue(buildStatusNode, "defaultParameterMsg");
                    String defaultTextFromLogKey = getValue(buildStatusNode, "defaultTextFromLogKey");
                    String defaultTextFromLogMsg = getValue(buildStatusNode, "defaultTextFromLogMsg");

                    foreach (XmlNode jobStatusNode in jobStatusNodes)
                    {
                        BuildStatusConfig buildStatus = new BuildStatusConfig();

                        buildStatus.type = getValue(jobStatusNode, "type");

                        buildStatus.messageSend = Convert.ToBoolean(tryGetValue(jobStatusNode, "messageSend", defaultMessageSend));
                        buildStatus.messageText = tryGetValue(jobStatusNode, "message", defaultMessageText);

                        buildStatus.topicChange = Convert.ToBoolean(tryGetValue(jobStatusNode, "topicChange", defaultTopicChange));
                        buildStatus.topicText = tryGetValue(jobStatusNode, "topic", defaultTopicText);

                        buildStatus.participantMsg = getValue(jobStatusNode, "participantMsg");

                        buildStatus.parameterName = tryGetValue(jobStatusNode, "parameterName", defaultParameterName);
                        buildStatus.parameterMsg = tryGetValue(jobStatusNode, "parameterMsg", defaultParameterMsg);

                        buildStatus.textFromLogKey = tryGetValue(jobStatusNode, "textFromLogKey", defaultTextFromLogKey);
                        buildStatus.textFromLogMsg = tryGetValue(jobStatusNode, "textFromLogMsg", defaultTextFromLogMsg);

                        buildStatuses.Add(buildStatus);
                    }                    
                    config.bldStatusChangedConfigs = buildStatuses;
                }

                XmlNode eachBuildStatusNode = jobMonitoring.SelectSingleNode("statusOfEachBuild");
                config.eachBuildStatus = Convert.ToBoolean(getValue(eachBuildStatusNode, "enabled"));
                if (config.eachBuildStatus)
                {
                    XmlNodeList singleBuildStatusNodes = eachBuildStatusNode.SelectNodes("status");
                    List<BuildStatusConfig> eachBuildStatuses = new List<BuildStatusConfig>(singleBuildStatusNodes.Count);

                    String defaultMessageSend = getValue(eachBuildStatusNode, "messageSend");
                    String defaultMessageText = getValue(eachBuildStatusNode, "defaultMessage");
                    String defaultParameterName = getValue(eachBuildStatusNode, "defaultParameterName");
                    String defaultParameterMsg = getValue(eachBuildStatusNode, "defaultParameterMsg");
                    String defaultTextFromLogKey = getValue(eachBuildStatusNode, "defaultTextFromLogKey");
                    String defaultTextFromLogMsg = getValue(eachBuildStatusNode, "defaultTextFromLogMsg");

                    foreach (XmlNode singleBuildStatusNode in singleBuildStatusNodes)
                    {
                        BuildStatusConfig buildStatus = new BuildStatusConfig();

                        buildStatus.type = getValue(singleBuildStatusNode, "type");

                        buildStatus.messageSend = Convert.ToBoolean(tryGetValue(singleBuildStatusNode, "messageSend", defaultMessageSend));
                        buildStatus.messageText = tryGetValue(singleBuildStatusNode, "message", defaultMessageText);

                        buildStatus.participantMsg = getValue(singleBuildStatusNode, "participantMsg");

                        buildStatus.parameterName = tryGetValue(singleBuildStatusNode, "parameterName", defaultParameterName);
                        buildStatus.parameterMsg = tryGetValue(singleBuildStatusNode, "parameterMsg", defaultParameterMsg);

                        buildStatus.textFromLogKey = tryGetValue(singleBuildStatusNode, "textFromLogKey", defaultTextFromLogKey);
                        buildStatus.textFromLogMsg = tryGetValue(singleBuildStatusNode, "textFromLogMsg", defaultTextFromLogMsg);

                        eachBuildStatuses.Add(buildStatus);
                    }
                    config.eachBuildStatusConfigs = eachBuildStatuses;
                }

                XmlNode buildStillRedNode = jobMonitoring.SelectSingleNode("buildStillRed");
                config.bldStillRed = Convert.ToBoolean(getValue(buildStillRedNode, "enabled"));
                if (config.bldStillRed)
                {
                    BuildStillRedConfig buildStillRedConfig = new BuildStillRedConfig();
                    buildStillRedConfig.statuses = getValue(buildStillRedNode, "type");
                    buildStillRedConfig.message = getValue(buildStillRedNode, "message");
                    buildStillRedConfig.participantMsg = getValue(buildStillRedNode, "participantMsg");
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
