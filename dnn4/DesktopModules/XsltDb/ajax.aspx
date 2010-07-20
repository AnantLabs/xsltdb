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
        Response.ContentType = "text/plain";
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
        int TabModuleID = Convert.ToInt32(Request.Form["TabModuleID"]);
        int ModuleID = Convert.ToInt32(Request.Form["ModuleID"]);
        int TabID = Convert.ToInt32(Request.Form["TabID"]);

        Findy.XsltDb.Transformer t = new Findy.XsltDb.Transformer(TabModuleID, ModuleID, TabID);

        string xml = @"<root></root>";
        string res;
        string xsl = Findy.XsltDb.XsltDbUtils.GetXSLT(ModuleID);
        if (xsl.Trim().Length > 0)
            res = t.Transform(xml, xsl);
        else
            res = "Use <b>Edit XSLT</b> link to setup an XSL transformation.";
        return res;
    }
</script>

