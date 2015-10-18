using System;
using System.Collections.Generic;

namespace Jenkins2SkypeMsg.utils.configuration.notifications
{
    class DailyReportConfig
    {
        public Boolean dailyReport { get; set; }

        public String dailyTimeFrom { get; set; }
        public String dailyTimeTo { get; set; }

        public List<DailyMessagesConfig> messageConfig { get; set; }
    }
}
