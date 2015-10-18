using Jenkins2SkypeMsg.utils.CI.jenkins.connectors;
using Jenkins2SkypeMsg.utils.configuration.notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.handlers
{
    class DailyReport
    {
        private JobConnector job;
        private String[,] builds;

        private long timeFrom;
        private long timeTo;
        private String lastStatus;

        public const String redPattern = "failure,unstable";
        public const String greenPattern = "success";

        public DailyReport(JobConnector job)
        {
            this.job = job;
            builds = job.getBuilds();
        }

        public DailyReport(String url)
        {
            this.job = new JobConnector(url);
            builds = job.getBuilds();
        }

        public Boolean isValid()
        {
            return job.isValid();
        }

        public void optimizeData(long timeFrom, long timeTo)
        {
            this.timeFrom = timeFrom;
            this.timeTo = timeTo;
            
            String[,] optimizedBuilds;
            int column = builds.GetLength(1);
            int row = 0;
            
            for(int i = 0; i < builds.GetLength(0); i++)
            {
                long buildTimestamp = Convert.ToInt64(builds[i, JobConnector.timestamp]);
                if (buildTimestamp >= timeFrom && buildTimestamp <= timeTo)
                    row++;
            }

            optimizedBuilds = new String[row, column];
            int optimizedColumn = 0;

            for (int c = 0; c < builds.GetLength(0); c++)
            {
                long buildTimestamp = Convert.ToInt64(builds[c, JobConnector.timestamp]);
                if (buildTimestamp >= timeFrom && buildTimestamp <= timeTo)
                {
                    for (int r = 0; r < builds.GetLength(1); r++)
                    {
                        optimizedBuilds[optimizedColumn, r] = builds[c, r];
                    }
                    optimizedColumn++;
                }
                else if (String.IsNullOrEmpty(lastStatus) && buildTimestamp < timeTo)
                {
                    lastStatus = builds[c, JobConnector.status];
                }
            }
            if (String.IsNullOrEmpty(lastStatus))
                lastStatus = greenPattern.Split(',').First();

            builds = optimizedBuilds;
        }

        public String generateMessages(DailyReportConfig reportConfig)
        {
            String message = "";
            String start = "";
            String end = "";

            foreach (DailyMessagesConfig config in reportConfig.messageConfig)
            {
                if (config.type == "start")
                {
                    start = String.Format(config.message, getBuildsCount(), 
                        getReportTime(reportConfig.dailyTimeFrom, reportConfig.dailyTimeTo));
                    start += "\n";
                }
                else if (config.type == "end")
                {
                    end += config.message;
                }
                else
                {
                    String addMessage = null;
                    if (config.type == "status")
                    {
                        int count = getCount(JobConnector.status, config.condition.ToUpper());
                        if (count >= config.minToGet && count <= config.maxToGet
                            || count >= config.minToGet && config.maxToGet == 0)
                        {
                            String time = getTimerOf(JobConnector.status, config.condition.ToUpper());
                            addMessage += String.Format(config.message, count, time);
                        }
                    }
                    else if (config.type.Contains("push"))
                    {
                        bool pushIn = false;
                        if (config.type == "pushIn") pushIn = true;

                        List<String> participant = getPersonsByStatus(config.condition.ToUpper(), pushIn);
                        if (participant.Count > 0)
                        {
                            String topPusher = getTopPerson(participant);
                            int topPusherCount = getCountOfPersons(participant, topPusher);
                            if (topPusherCount >= config.minToGet && topPusherCount <= config.maxToGet
                                || topPusherCount >= config.minToGet && config.maxToGet == 0)
                            {
                                addMessage += String.Format(config.message, 
                                    TextUtils.optimizeUserName(topPusher), topPusherCount);
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(addMessage))
                        message += addMessage + "\n";
                }
            }
            if (!String.IsNullOrEmpty(message))
                return start + message + end;
            else
                return message;
        }

        private int getBuildsCount()
        {
            return builds.GetLength(0);
        }

        private List<String> getPersonsByStatus(String expectedStatus, bool pushIn)
        {
            List<String> persons = new List<string>();
            String lastStatus = this.lastStatus;
            long lastTime = 0;
            String actualStatus;
            String brokeLastBuild = "";

            for (int i = builds.GetLength(0) - 1; i >= 0; i--)
            {
                actualStatus = builds[i, JobConnector.status];
                long actualTimestamp = Convert.ToInt64(builds[i, JobConnector.timestamp]);
                long actualDuraction = Convert.ToInt64(builds[i, JobConnector.duraction]);

                if (!String.IsNullOrEmpty(lastStatus))
                {
                    if (expectedStatus.Contains(actualStatus))
                    {
                        if (pushIn && expectedStatus.Contains(lastStatus))
                        {
                            if (actualTimestamp > lastTime)
                            {
                                foreach (string participant in builds[i, JobConnector.participant].Split(';'))
                                {
                                    if (!String.IsNullOrWhiteSpace(participant)
                                        && !brokeLastBuild.Contains(participant))
                                    {
                                        persons.Add(participant);
                                    }
                                }
                            }
                        }
                        else if (!pushIn && !expectedStatus.Contains(lastStatus))
                        {
                            brokeLastBuild = builds[i, JobConnector.participant];
                            foreach (string participant in brokeLastBuild.Split(';'))
                            {
                                persons.Add(participant);
                            }
                            
                        }
                    }
                }
                lastStatus = actualStatus;
                lastTime = actualTimestamp + actualDuraction + 60000;
            }
            return persons;
        }

        private String getTopPerson(List<String> persons)
        {
            String topPerson = "";
            int lastRepeats = 0;
            int actualRepeats;
            foreach(String person in persons)
            {
                actualRepeats = getCountOfPersons(persons, person);
                if (actualRepeats > lastRepeats)
                {
                    topPerson = person;
                }
            }
            return topPerson;
        }

        private int getCountOfPersons(List<String> persons, String person)
        {
            return persons.Count(i => i == person);
        }

        private int getCount(int column, String pattern)
        {
            int result = 0;
            for (int i = 0; i < getBuildsCount(); i++)
            {
                if (pattern.Contains(builds[i, column]))
                {
                    result++;
                }
            }
            return result;
        }

        private String getTimerOf(int column, String pattern)
        {
            long timer = 0;
            long lastBuildTiestamp = 0;
            Boolean firstItem = true;

            for (int i = 0; i < getBuildsCount(); i++)
            {
                long duraction = Convert.ToInt64(builds[i, JobConnector.duraction]);
                
                if (duraction != 0)
                {
                    long buildFinishTime = Convert.ToInt64(builds[i, JobConnector.timestamp]) + duraction;

                    if (pattern.Contains(builds[i, column]))
                    {
                        if (firstItem)
                        {
                            timer += timeTo - buildFinishTime;
                        }
                        else
                        {
                            timer += lastBuildTiestamp - buildFinishTime;
                        }
                    }

                    if (pattern.Contains(lastStatus) && i == getBuildsCount() - 1)
                    {
                        timer += buildFinishTime - timeFrom;
                    }

                    lastBuildTiestamp = buildFinishTime;
                    firstItem = false;
                }
            }
            return TimeUtils.convertMsToTime(timer);
        }

        private String getReportTime(String from, String to)
        {
            String reportTime;
            
            int fromDay = 0;
            String fromTime = from;
            String[] fromPattern = from.Split('|');
            if (fromPattern.Length > 0)
            {
                fromDay = Convert.ToInt16(fromPattern[0]);
                fromTime = fromPattern[1];
            }

            int toDay = 0;
            String toTime = to;
            String[] toPattern = to.Split('|');
            if (toPattern.Length > 0)
            {
                toDay = Convert.ToInt16(toPattern[0]);
                toTime = toPattern[1];
            }

            if (fromDay == toDay)
            {
                reportTime = String.Format("{0} from {1} to {2}",
                    TimeUtils.convertMoveDay(fromDay), fromTime, toTime);
            }
            else
            {
                reportTime = String.Format("from {0} {1} to {2} {3}",
                    TimeUtils.convertMoveDay(fromDay), fromTime, TimeUtils.convertMoveDay(toDay), toTime);
            }

            return reportTime;
        }
    }
}
