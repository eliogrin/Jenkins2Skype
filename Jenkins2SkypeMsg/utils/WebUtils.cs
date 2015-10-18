using Jenkins2SkypeMsg.utils.messenger;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Jenkins2SkypeMsg.utils
{
    class WebUtils
    {
        public String getResponse(String url)
        {
            String response = "";

            Boolean error;
            int attempt = 10;
            do
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        response = client.DownloadString(url);
                        error = false;
                    }
                }
                catch (WebException ex)
                {
                    if (attempt > 0)
                    {
                        error = true;
                        attempt -= 1;
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Trace.TraceError("{0} - WebClient:Sorry, job '{1}' url: {2}, response is: {3}",
                            TimeUtils.getCurrentTime(), Thread.CurrentThread.Name, url, ex.Message);
                        Messenger.Instance.sendAdminMessage("Sorry, job " + Thread.CurrentThread.Name
                            + " url: " + url + ", response is: " + ex.Message);
                        error = false;
                    }
                }
            } while (error);

            return response;
        }

        public String getResponse(String url, String ending)
        {
            return getResponse(uniteUrl(url, ending));
        }

        public String uniteUrl(String firstPart, String secondPart)
        {
            String complexUrl = firstPart;
            if (!String.IsNullOrEmpty(complexUrl) && !String.IsNullOrEmpty(secondPart))
            {
                if (!complexUrl.EndsWith("/"))
                    complexUrl += "/";
                complexUrl += secondPart;
            }
            return complexUrl;
        }

        public static String getUrlFromJob(String jenkinsUrl, string jobDetails)
        {
            String url = "";

            int index = jenkinsUrl.IndexOf("job");
            if (index > 0)
            {
                url += jenkinsUrl.Substring(0, index + 4);
                String[] subJobs = jobDetails.Split('#');
                url += subJobs[0].Trim() + "/";
                url += subJobs[1].Trim() + "/";
            }

            return url;
        }
    }
}
