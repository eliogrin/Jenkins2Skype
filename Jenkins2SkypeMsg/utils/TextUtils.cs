using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenkins2SkypeMsg.utils
{
    class TextUtils
    {
        public static String optimizeUserName(String name)
        {
            String formatedPersons = "";
            if (!String.IsNullOrEmpty(name))
            {
                String[] listOfPersons = name.Split(';');
                int persons = 0;
                foreach (String singlePerson in listOfPersons)
                {
                    persons++;
                    if (persons > 1)
                    {
                        if (listOfPersons.Length == persons)
                            formatedPersons += " and ";
                        else
                            formatedPersons += ", ";
                    }
                    String formatedName = "";
                    foreach (String partName in singlePerson.Split('@')[0].Split('_'))
                    {
                        if (!String.IsNullOrEmpty(formatedName))
                            formatedName += " ";
                        formatedName += partName.First().ToString().ToUpper() + partName.Substring(1);
                    }
                    formatedPersons += formatedName;
                }
            }
            return formatedPersons;
        }
    }
}
