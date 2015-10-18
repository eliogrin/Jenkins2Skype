using System;

namespace Jenkins2SkypeMsg.utils.messenger
{
    class Messenger
    {
        private static volatile SkypeConnector instance;
        private static object syncRoot = new Object();

        private Messenger() { }

        public static SkypeConnector Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SkypeConnector();
                    }
                }

                return instance;
            }
        }
    }
}
