using Jenkins2SkypeMsg.utils.fileReader;
using System;

namespace Jenkins2SkypeMsg.utils.CI.jenkins.connectors
{
    class BuildConnector : XmlFile
    {
        private String urlEnding = "api/xml";

        private const String lastBuild = "lastBuild";

        public const String redPattern = "failure,unstable";
        public const String greenPattern = "success";
        public const String abortedPattern = "aborted";
        public const String activeBuild = "in progress";

        int number = 0;
        String status = null;
        Boolean duractionChecked;
        long duraction = 0L;
        Boolean timestampChecked;
        long timestamp = 0L;
        String url = null;

        public BuildConnector(String url, Boolean getLastBuild)
        {
            String buildUrl;
            if (getLastBuild)
            {
                WebUtils webUtils = new WebUtils();
                buildUrl = webUtils.uniteUrl(url, lastBuild);
            }
            else
            {
                buildUrl = url;
            }
            initBuild(buildUrl);
        }

        public BuildConnector(String buildUrl)
        {
            initBuild(buildUrl);
        }

        public BuildConnector(String jobUrl, int buildNumber)
        {
            WebUtils webUtils = new WebUtils();
            String buildUrl = webUtils.uniteUrl(jobUrl, buildNumber.ToString());
            initBuild(buildUrl);
        }

        private void initBuild(String buildUrl)
        {
            WebUtils webUtils = new WebUtils();
            String response = webUtils.getResponse(buildUrl, urlEnding);
            initXml(response);
        }

        public Boolean isValid()
        {
            return isXmlValid();
        }

        public int getNumber()
        {
            if (number == 0)
            {
                number = Convert.ToInt16(getInnerText("//number"));
            }
            return number;
        }

        public long getDuraction()
        {
            if (!duractionChecked)
            {
                duraction = Convert.ToInt64(getInnerText("//duration"));
                duractionChecked = true;
            }
            return duraction;
        }

        public long getTimestamp()
        {
            if (!timestampChecked)
            {
                timestamp = Convert.ToInt64(getInnerText("//timestamp"));
                timestampChecked = true;
            }
            return timestamp;
        }

        public String getStatus()
        {
            if (String.IsNullOrEmpty(status))
            {
                if (getDuraction() > 0)
                {
                    status = getInnerText("//result").ToLower();
                }
                if (String.IsNullOrEmpty(status))
                {
                    status = activeBuild;
                }
            }
            return status;
        }

        public String getParticipant()
        {
            String userList = "";
            foreach (String participant in getInnerTexts("//changeSet/item/author/fullName"))
            {
                if (!userList.Contains(participant))
                {
                    if (userList != "")
                        userList += ";";
                    userList += participant;
                }
            }
            return userList;
        }

        public String getAborted()
        {
            String user = "";
            foreach (String description in getInnerTexts("//action/cause/shortDescription"))
            {
                if (description.ToLower().Contains(BuildConnector.abortedPattern))
                {
                    String[] descriptionsPart = description.Split(' ');
                    user = descriptionsPart[descriptionsPart.Length - 1];
                }
            }
            return user;
        }

        public String getInitiator()
        {
            return getInnerText("//action/cause/userName");
        }

        public String getUrl()
        {
            if (String.IsNullOrEmpty(url))
            {
                url = getInnerText("//url");
            }
            return url;
        }

        public String getClaimedPerson()
        {
            return getInnerText("//claimedBy");
        }
    }
}
