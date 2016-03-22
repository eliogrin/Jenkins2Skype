using System;
using System.IO;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.connectors
{
    class LogConnector
    {
        String log;
        
        private const String urlEnding = "logText/progressiveText?start=0";
        
        public LogConnector(String url)
        {
            WebUtils webUtils = new WebUtils();
            this.log = webUtils.getResponse(url, urlEnding);
        }

        public String getLineByKey(String keyWord)
        {
            String result = "";
            using (StringReader sr = new StringReader(log))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains(keyWord))
                    {
                        String[] lineElements = line.Split(new string[] { keyWord }, StringSplitOptions.None);
                        String extractedText;
                        if (lineElements.Length > 0)
                        {
                            extractedText = lineElements[1].Trim();
                        }
                        else
                        {
                            extractedText = line.Replace(keyWord, "").Trim();
                        }

                        if(!String.IsNullOrEmpty(extractedText))
                        {
                            if (!String.IsNullOrEmpty(result))
                            {
                                result += "; \n ";
                            }
                            result += extractedText;
                        }
                    }
                }
            }
            return result;
        }
    }
}
