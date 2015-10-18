using System;

namespace Jenkins2SkypeMsg.utils.configuration.notifications
{
    class BuildStillRedConfig
    {
        public String statuses { get; set; }
        public String message { get; set; }
        public String participantMsg { get; set; }
        public String subJobChangedMsg { get; set; }

        public String subJobClaimed { get; set; }
        public String subJobNotClaimed { get; set; }
    }
}
