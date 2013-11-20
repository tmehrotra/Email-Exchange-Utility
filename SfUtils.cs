using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ExchangeUtil
{
    class SfUtils
    {
        public string OBJ_ID = "";
        private List<SfPartner.sObject> ATTACHS;

        public SfUtils()
        {
            ATTACHS = new List<SfPartner.sObject>();
        }

        public Dictionary<string, string> getExprs(SfLogin loginInstance)
        {
            List<string> varNames = new List<string>();
            varNames.Add("Exchange_Regex");

            string response = loginInstance.NIMBUS_BINDING.getExprs();

            Dictionary<string, string> exprs = new Dictionary<string, string>();
            XmlDocument mainResponseXmlDoc = new XmlDocument();
            mainResponseXmlDoc.LoadXml(response);

            foreach (XmlNode responseNode in mainResponseXmlDoc.GetElementsByTagName("response"))
            {
                bool errorResp = true;

                System.Collections.IEnumerator attrs = responseNode.Attributes.GetEnumerator();

                while (attrs.MoveNext())
                {
                    XmlNode attr = (XmlNode)attrs.Current;

                    if (attr.Name == "level" && attr.Value == "0")
                    {
                        errorResp = false;
                    }
                }

                System.Collections.IEnumerator nodes = responseNode.GetEnumerator();

                while (nodes.MoveNext())
                {
                    XmlNode node = (XmlNode)nodes.Current;

                    if (errorResp)
                    {
                        if (node.Name == "message")
                        {
                            throw new Exception(node.InnerText);
                        }
                    }
                    else
                    {
                        if (node.Name == "data")
                        {
                            System.Collections.IEnumerator data = node.GetEnumerator();

                            while (data.MoveNext())
                            {
                                XmlNode expr = (XmlNode)data.Current;

                                if (expr.Name == "expr")
                                {
                                    string name = "";
                                    System.Collections.IEnumerator dataAttrs = expr.Attributes.GetEnumerator();

                                    while (dataAttrs.MoveNext())
                                    {
                                        XmlNode dataAttr = (XmlNode)dataAttrs.Current;

                                        if (dataAttr.Name == "name")
                                        {
                                            name = dataAttr.Value;
                                        }
                                    }

                                    exprs.Add(name, expr.InnerText);
                                }
                            }
                        }
                    }
                }
            }

            return exprs;
        }

        public void processEmail(SfLogin loginInstance, string emailAddress, string folderName, string matchesXML)
        {
            string response = loginInstance.NIMBUS_BINDING.processEmail(emailAddress, folderName, matchesXML);

            XmlDocument mainResponseXmlDoc = new XmlDocument();
            mainResponseXmlDoc.LoadXml(response);

            foreach (XmlNode responseNode in mainResponseXmlDoc.GetElementsByTagName("response"))
            {
                bool errorResp = true;

                System.Collections.IEnumerator attrs = responseNode.Attributes.GetEnumerator();

                while (attrs.MoveNext())
                {
                    XmlNode attr = (XmlNode)attrs.Current;

                    if (attr.Name == "level" && attr.Value == "0")
                    {
                        errorResp = false;
                    }
                }

                System.Collections.IEnumerator nodes = responseNode.GetEnumerator();

                while (nodes.MoveNext())
                {
                    XmlNode node = (XmlNode)nodes.Current;

                    if (errorResp)
                    {
                        if (node.Name == "message")
                        {
                            throw new Exception(node.InnerText);
                        }
                    }
                    else
                    {
                        if (node.Name == "data")
                        {
                            System.Collections.IEnumerator fields = node.GetEnumerator();

                            while (fields.MoveNext())
                            {
                                XmlNode field = (XmlNode)fields.Current;

                                if (field.Name == "objId")
                                {
                                    OBJ_ID = field.InnerText;

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void addAttachment(string filename, string body)
        {
            SfPartner.sObject attach = new SfPartner.sObject();
            System.Xml.XmlElement[] attachFs = new System.Xml.XmlElement[3];

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            attachFs[0] = doc.CreateElement("Name"); attachFs[0].InnerText = filename;
            attachFs[1] = doc.CreateElement("ParentId"); attachFs[1].InnerText = OBJ_ID;
            attachFs[2] = doc.CreateElement("Body"); attachFs[2].InnerText = EncodeTo64(body);
            attach.type = "Attachment";
            attach.Any = attachFs;

            ATTACHS.Add(attach);
        }

        public void createAttachments(SfLogin loginInstance)
        {
            Utils.writeLog(Utils.logLevel.INFO, "There are " + ATTACHS.Count + " attachment(s)", null);

            SfPartner.SaveResult[] srs = loginInstance.SF_BINDING.create((SfPartner.sObject[])ATTACHS.ToArray());

            string errorstring = "";

            foreach (SfPartner.SaveResult sr in srs)
            {
                if (!sr.success)
                {
                    foreach (SfPartner.Error error in sr.errors)
                    {
                        errorstring += error.statusCode + " - " + error.message + Environment.NewLine;
                    }
                }
            }

            if (errorstring.Length > 0)
            {
                throw new Exception(errorstring);
            }
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes
                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue
                  = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
    }
}
