using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
//using System.Web.Security;
using System.Web.UI;
//using System.Web.UI.HtmlControls;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;

using DotNetNuke.Security.Membership;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;

using Findy.XsltDb;

public partial class DesktopModules_XsltDb_Rich : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/plain";
        Response.ContentEncoding = Encoding.UTF8;

        string res = string.Empty;
        try
        {
            string xml = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            res = Execute(xml);
        }
        catch (Exception)
        {
            res = "<root><result>General server error.</result></root>";
        }
        Response.Write(res);
        Response.End();
    }

    private string Execute(string xml)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        string function = doc.SelectSingleNode("//function").InnerText;
        string username = doc.SelectSingleNode("//username").InnerText;
        string password = doc.SelectSingleNode("//password").InnerText;

        if (!Validate(username, password))
        {
            return "<root><result>Authentication error.</result></root>";
        }

        switch (function)
        {
            case "Ping":
                return "<root><result>ok</result></root>";
            case "GetXsltDbList":
                return GetXsltDbList();
            case "GetXsltDb":
                return GetXsltDb(Convert.ToInt32(doc.SelectSingleNode("//ModuleID").InnerText));
            case "SaveXsltDb":
                int ModuleID = Convert.ToInt32(doc.SelectSingleNode("//ModuleID").InnerText);
                string xslt = doc.SelectSingleNode("//xslt").InnerText;
                return SaveXsltDb(ModuleID, xslt);
            case "PublishAll":
                XsltDbUtils.PublishAll();
                return "<root><result>ok</result></root>";
        }

        return "<root><result>Wrong function name.</result></root>";
    }

    private string SaveXsltDb(int ModuleID, string xslt)
    {
        XsltDbUtils.aConfig conf = XsltDbUtils.GetConfig(ModuleID);
        XsltDbUtils.SaveXSLT(ModuleID, xslt, conf.IsSuper, conf.Name, conf.ServiceName);
        return "<root><result>ok</result></root>";
    }

    private string GetXsltDb(int ModuleID)
    {
        return string.Format("<root><xslt>{0}</xslt><result>ok</result></root>",
            XmlElementEncode(XsltDbUtils.GetXSLT(ModuleID)));
    }

    private string GetXsltDbList()
    {
        DataTable dt = XsltDbUtils.GetModuleList(IsSuperUser);
        StringWriter sw = new StringWriter();
        dt.WriteXml(sw, XmlWriteMode.WriteSchema);
        return string.Format("<root><list>{0}</list><result>ok</result></root>",
            XmlElementEncode(sw.ToString()));
        
    }

    private string XmlElementEncode(string value)
    {
        XmlDocument doc = new XmlDocument();
        XmlElement root = doc.CreateElement("root");
        root.InnerText = value;
        return root.InnerXml;

    }

    bool IsSuperUser = false;

    private bool Validate(string login, string password)
    {
        UserLoginStatus status = new UserLoginStatus();
        UserInfo ui = UserController.ValidateUser(
            PortalController.GetCurrentPortalSettings().PortalId,
            login, password, "", "", "", ref status);

        IsSuperUser = ui != null && ui.IsSuperUser;
        return ui != null && (ui.IsSuperUser || ui.IsInRole("Administrators"));
    }
}
