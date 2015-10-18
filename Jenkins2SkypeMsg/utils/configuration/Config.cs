using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Jenkins2SkypeMsg.utils.configuration
{
    class Config
    {
        private static volatile List<JobConfiguration> instance;
        private static object syncRoot = new Object();

        private Config() { }

        public static List<JobConfiguration> Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(syncRoot)
                    {
                        if (instance == null)
                            instance = new List<JobConfiguration>();
                    }
                }

                return instance;
            }
        }

        public static void readConfig(String path)
        {
            Trace.WriteLine("Preparing to writing config file.");
            instance = JobsConfigReader.prepareJobs(path);
        }
    }
}
