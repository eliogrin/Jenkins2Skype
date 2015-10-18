using System;

namespace Jenkins2SkypeMsg.utils.configuration.notifications
{
    class DailyMessagesConfig
    {
        public String type { get; set; }
        public String condition { get; set; }
        public int minToGet { get; set; }
        public int maxToGet { get; set; }
        public String message { get; set; }
    }
}
