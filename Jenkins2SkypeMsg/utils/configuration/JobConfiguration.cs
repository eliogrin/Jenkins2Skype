using Jenkins2SkypeMsg.utils.configuration.notifications;
using System;
using System.Collections.Generic;

namespace Jenkins2SkypeMsg.utils.configuration
{
    class JobConfiguration
    {
        // from config file
        public String name { get; set; }
        public String url { get; set; }
        public long timeout { get; set; }
        public String onTime { get; set; }
        public String onDate { get; set; }

        public String messengerChatId { get; set; }

        public Boolean bldStatusChanged { get; set; }
        public Boolean buildStatusTopicChange { get; set; }
        public List<BuildStatusConfig> bldStatusChangedConfigs { get; set; }

        public Boolean eachBuildStatus { get; set; }
        public List<BuildStatusConfig> eachBuildStatusConfigs { get; set; }

        public Boolean dailyReport { get; set; }
        public DailyReportConfig dailyReportConfig { get; set; }

        public Boolean bldStillRed { get; set; }
        public BuildStillRedConfig bldStillRedConfig { get; set; }

        public Boolean bldFroze { get; set; }
        public String bldFrozenMessage { get; set; }
        public int bldFrozenTimeout { get; set; }

        public Boolean grpStatusMonitoring { get; set; }
        public List<BuildStatusConfig> grpStatusMonitoringConfigs { get; set; }

        // during execution changes
        public Boolean enabled { get; set; }
        public Boolean activeMessaging { get; set; }
        public int lastBuildNumber { get; set; }
        public int lastFrozenBuild { get; set; }
        public String lastBuildStatus { get; set; }
        public long lastFinishTime { get; set; }
        public String lastFailedSubJobs { get; set; }
        public long lastChatMessageTimestamp { get; set; }
    }
}
