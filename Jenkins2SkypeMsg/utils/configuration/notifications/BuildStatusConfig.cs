using System;

namespace Jenkins2SkypeMsg.utils.configuration.notifications
{
    class BuildStatusConfig
    {
        public String type { get; set; }

        public Boolean topicChange { get; set; }
        public String topicText { get; set; }

        public Boolean messageSend { get; set; }
        public String messageText { get; set; }
        public String participantMsg { get; set; }
        public String logMsg { get; set; }
    }
}
