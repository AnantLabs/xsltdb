/*
 XsltDb is powerful XSLT module for DotnetNuke.
 It offers safe database access, SEO-friendly AJAX support,
 visitor interactions, environment integration (dnn properties,
 request, cookie, session and form values), regular expressions, etc.

 Author:
 
    Anton Burtsev
    burtsev@yandex.ru

 Project home page: 
 
    http://xsltdb.codeplex.com
*/

using System;
using System.IO;
using System.Xml;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;

namespace Findy.XsltDb
{

    /// <summary>
    /// Summary description for XsltDbControler
    /// </summary>
    public class XsltDbController : IPortable
    {
        #region IPortable Members

        public string ExportModule(int ModuleID)
        {
            return XsltDbUtils.GetXSLTData(ModuleID);
        }

        public void ImportModule(int ModuleID, string Content, string Version, int UserID)
        {
            XmlNode content = Globals.GetContent(Content, "xsltdb");
            XsltDbUtils.ImportModule(ModuleID, content.OuterXml);
        }

        #endregion
    }
}