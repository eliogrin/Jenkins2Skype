using Jenkins2SkypeMsg.utils.fileReader;
using System;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.connectors
{
    class ViewConnector : XmlFile
    {
        private String urlEnding = "api/xml";

        public const String redPattern = "red,yellow";
        public const String greenPattern = "blue,green";

        public const int jobName = 0;
        public const int jobStatus = 1;

        public ViewConnector(String url)
        {
            WebUtils webUtils = new WebUtils();
            String response = webUtils.getResponse(url, urlEnding);
            initXml(response);
        }

        public Boolean isValid()
        {
            return isXmlValid();
        }

        public String[,] getViews()
        {
            return getNodesText("//job", "name", "color");
        }
    }
}
