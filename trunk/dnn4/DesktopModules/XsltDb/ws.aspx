<%@ Page Language="C#" %>

<script runat="server">
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
    
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/xml";
        Response.ContentEncoding = Encoding.UTF8;
        
        string res = string.Empty;
        try
        {
            res = Execute();
        }
        catch (Exception ex)
        {
            res = Findy.XsltDb.XsltDbUtils.GetExceptionMessage(ex);
        }
        Response.Write(res);
        Response.End();
    }

    private string Execute()
    {
        string serviceName = Request["service"];
        string xsl = null;
        int ModuleID = -1;

        if (serviceName != null && serviceName.Length > 0)
        {
            using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_GetServiceConfig",
                PortalController.GetCurrentPortalSettings().PortalId,
                serviceName))
            {
                if (r.Read())
                {
                    xsl = r["XSLT"].ToString();
                    ModuleID = Convert.ToInt32(r["ModuleID"]);
                }
            }
        }

        if (ModuleID == -1)
        {
            ModuleID = Convert.ToInt32(Request["mod"]);
            xsl = Findy.XsltDb.XsltDbUtils.GetXSLT(ModuleID);
        }

        Findy.XsltDb.Transformer t = new Findy.XsltDb.Transformer(-1, ModuleID, -1);

        string xml = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
        if ( xml == null || xml.Trim().Length == 0 )
            xml = "<root />";

        string res;
        if (xsl.Trim().Length > 0)
            res = t.Transform(xml, xsl).Trim();
        else
            res = "Use <b>Edit XSLT</b> link to setup an XSL transformation.";
        return res;
    }
</script>

