using SKYPE4COMLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

namespace Jenkins2SkypeMsg.utils.messenger
{
    class SkypeConnector
    {
        Skype skype;

        public SkypeConnector()
        {
            Trace.WriteLine("Try connect to skype API, check for message in Skype");
            skype = new Skype();
            skype.Attach(7, false);
            sendAdminMessage(String.Format("Today is {0}, I am start working at {1}",
                TimeUtils.getCurrentDate(), TimeUtils.getCurrentTime()));
            Trace.WriteLine("-- connection successful");
        }
        
        private Skype getSkype() 
        {
            int waitForSkype = 60; // seconds wait for skype launching

            do 
            {
                try
                {
                    String version = skype.Version;
                    if (!String.IsNullOrEmpty(version))
                        waitForSkype = 0;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Skype unavailable right now, with ex: " + ex.Message);
                    System.Threading.Thread.Sleep(1000);
                }
                waitForSkype--;
            }
            while (waitForSkype > 0);
            
            return skype;
        }

        public void sendAdminMessage(String message)
        {
            String adminId = ConfigurationManager.AppSettings["userAdmin"];
            if (!String.IsNullOrEmpty(adminId))
                sendMessage(adminId, message);
        }

        public void sendMessage(String id, String message)
        {
            if(id.Length > 25)
            {
                sendGroupMessage(id, message);
            }
            else
            {
                sendPrivateMessage(id, message);
            }
        }

        private void sendPrivateMessage(String userName, String message)
        {
            try
            {
                getSkype().SendMessage(userName, message);
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - Skype: can't send private message, we get exception here: '{1}'", 
                    TimeUtils.getCurrentTime(), ex.Message);
            }
        }

        public String getTopicName(String id)
        {
            return getSkype().FindChatUsingBlob(id).Topic;
        }

        public void clearRecentMessages(String chatId)
        {
            getSkype().FindChatUsingBlob(chatId).ClearRecentMessages();
        }

        public String createGroup(params String[] users)
        {
            UserCollection usersCollection = new UserCollection();
            foreach(String userName in users)
            {
                User user = skype.get_User(userName);
                usersCollection.Add(user);
            }
            Chat chat = skype.CreateChatMultiple(usersCollection);
            sendGroupMessage(chat.Blob, "Group chat created, blob id of chat: '" + chat.Blob +"'");
            return chat.Blob;
        }

        private void sendGroupMessage(String blob, String message)
        {
            try
            {
                getSkype().FindChatUsingBlob(blob).SendMessage(message);
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - Skype: can't send group message, we get exception here: '{1}'",
                    TimeUtils.getCurrentTime(), ex.Message);
            }
        }

        public String getStatus()
        {
            String status = "";

            try
            {
                status = getSkype().CurrentUserStatus.ToString();
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - Skype: we get error while trying get status: '{1}'", 
                    TimeUtils.getCurrentTime(), ex.Message);
            }

            return status;
        }

        public void goOnline()
        {
            getSkype().ChangeUserStatus(TUserStatus.cusOnline);
        }
    }
}
