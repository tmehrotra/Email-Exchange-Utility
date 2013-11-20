using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ExchangeUtil
{
    class EspressoRegexUtils
    {
        private Dictionary<string, string> exprs;
        private Dictionary<string, List<string>> matches;

        public EspressoRegexUtils()
        {
            exprs = new Dictionary<string, string>();
            matches = new Dictionary<string, List<string>>();
        }

        public void regex_init()
        {
            SfLogin sfLogin = new SfLogin();
            sfLogin.login();

            SfUtils sfUtils = new SfUtils();
            exprs = sfUtils.getExprs(sfLogin);
        }

        public void regex_match(string searchStr)
        {
            string expr;
            foreach (string key in exprs.Keys)
            {
                List<string> matchesList;
                if (matches.TryGetValue(key, out matchesList))
                {
                    matches.Remove(key);
                }
                else
                {
                    matchesList = new List<string>();
                }

                if (exprs.TryGetValue(key, out expr))
                {
                    Regex myRegex = new Regex(expr);

                    MatchCollection regexMatches = myRegex.Matches(searchStr, 0);
                    System.Collections.IEnumerator regexMatch = regexMatches.GetEnumerator();

                    while (regexMatch.MoveNext())
                    {
                        Match id = (Match)regexMatch.Current;

                        matchesList.Add(id.Value);
                    }
                }

                matches.Add(key, matchesList);
            }
        }

        public string regex_getXML()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNode = xmlDoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            xmlDoc.AppendChild(xmlNode);

            XmlElement root = xmlDoc.CreateElement("matches");
            xmlDoc.AppendChild(root);

            foreach (string key in matches.Keys)
            {
                List<string> matchesList;

                if (matches.TryGetValue(key, out matchesList))
                {
                    XmlElement xmlElem = xmlDoc.CreateElement("match");
                    xmlElem.SetAttribute("name", key);
                    string values = "";

                    foreach (string value in matchesList)
                    {
                        if (values.Length > 0)
                        {
                            values += "," + value;
                        }
                        else
                        {
                            values += value;
                        }
                    }

                    xmlElem.InnerText = values;
                    root.AppendChild(xmlElem);
                }
            }

            return xmlDoc.InnerXml;
        }
    }
}
