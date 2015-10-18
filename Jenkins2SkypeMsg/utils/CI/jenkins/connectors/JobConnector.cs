using Jenkins2SkypeMsg.utils.fileReader;
using System;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.connectors
{
    class JobConnector : XmlFile
    {
        public const int status = 0;
        public const int timestamp = 1;
        public const int duraction = 2;
        public const int participant = 3;

        private String urlEnding = "api/xml?depth=0&tree=name,builds[number,timestamp,result,duration,changeSet[items[author[fullName]]]]";
        
        public JobConnector (String jobUrl)
        {
            WebUtils webUtils = new WebUtils();
            String response = webUtils.getResponse(jobUrl, urlEnding);
            initXml(response);
        }

        public Boolean isValid()
        {
            return isXmlValid();
        }

        public String[,] getBuilds()
        {
            return getNodesText("//build", "result", "timestamp", "duration", "changeSet/item/author/fullName");
        }
    }
}
