using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace Jenkins2SkypeMsg.utils.fileReader
{
    class XmlFile
    {
        private XmlDocument xmlDocument;
        private Boolean isValid;

        protected void initXml(String xmlText)
        {
            xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(xmlText);
                isValid = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - XmlReade: Can't read xml document: {1}",
                    TimeUtils.getCurrentTime(), ex.Message);
                isValid = false;
            }
        }

        protected Boolean isXmlValid()
        {
            return isValid;
        }

        private XmlNode getNode(String xpath)
        {
            XmlNode result = null;
            if (getNodes(xpath).Count > 0 )
            {
                result = xmlDocument.DocumentElement.SelectSingleNode(xpath);
            }
            return result;
        }

        private XmlNode getNode(XmlNode node, String xpath)
        {
            XmlNode result = null;
            if (node.SelectNodes(xpath).Count > 0)
            {
                result = node.SelectSingleNode(xpath);
            }
            return result;
        }

        private XmlNodeList getNodes(String xpath)
        {
            return xmlDocument.DocumentElement.SelectNodes(xpath);
        }

        private XmlNodeList getNodes(XmlNode node, String xpath)
        {
            XmlNodeList result = null;
            if (node != null)
            {
                result = node.SelectNodes(xpath);
            }
            return result;
        }

        protected String getInnerText(XmlNode node)
        {
            String result = null;
            if(node != null)
            {
                result = node.InnerText;
            }
            return result;
        }

        protected String getInnerText(String xpath)
        {
            return getInnerText(getNode(xpath));
        }

        private String getInnerText(XmlNode node, String xpath)
        {
            String result = "";
            XmlNodeList nodeList = getNodes(node, xpath);

            if (nodeList.Count == 1)
            {
                result = getInnerText(getNode(node, xpath));
            }
            else if (nodeList.Count > 1)
            {
                foreach (XmlNode subNode in nodeList)
                {
                    String innerText = getInnerText(subNode);
                    if (!result.Contains(innerText))
                    {
                        if (!String.IsNullOrEmpty(result))
                        {
                            result += ";";
                        }
                        result += subNode.InnerText;
                    }
                }
            }
            return result;
        }

        protected List<String> getInnerTexts(String xpath)
        {
            List<String> result = new List<string>();
            foreach (XmlNode node in getNodes(xpath))
            {
                String nodeText = getInnerText(node);
                if (!String.IsNullOrEmpty(nodeText))
                {
                    result.Add(nodeText);
                }
            }
            return result;
        }

        protected String[,] getNodesText(String mainXpath, params String[] nodesXpath)
        {
            XmlNodeList mainList = getNodes(mainXpath);
            String[,] nodesText = new String[mainList.Count, nodesXpath.Length];

            int column = 0;
            int row;
            foreach (XmlNode node in mainList)
            {
                row = 0;
                foreach(String nodeXpath in nodesXpath)
                {
                    nodesText[column, row] = getInnerText(node, nodeXpath);
                    row++;
                }
                column++;
            }
            return nodesText;
        }
    }
}
