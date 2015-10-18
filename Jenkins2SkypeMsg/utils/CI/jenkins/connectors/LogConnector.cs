using System;
using System.IO;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.connectors
{
    class LogConnector
    {
        String log;
        String failure = "";

        private const String failureKey = "completed  : {0}";
        private const String urlEnding = "logText/progressiveText?start=0";

        public LogConnector(String url)
        {
            WebUtils webUtils = new WebUtils();
            this.log = webUtils.getResponse(url, urlEnding);
        }

        public String getFailure()
        {
            if (String.IsNullOrEmpty(failure))
            {
                foreach (String type in BuildConnector.redPattern.Split(','))
                {
                    String key = String.Format(failureKey, type.ToUpper());
                    using (StringReader sr = new StringReader(log))
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Contains(key))
                            {
                                if (!String.IsNullOrEmpty(failure))
                                    failure += ", ";
                                failure += line.Replace(key, "").Trim();
                            }
                        }
                    }
                }
            }
            return failure;
        }
    }
}
