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
        Response.ContentType = "text/javascript";
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

        string code = string.Format("var {0} = \"{1}\";",
            Request["var"], res.Replace("\r\n", "&#x0A;").Replace("\n", "&#x0A;").Replace("\r", "&#x0A;").Replace("\\", "\\\\").Replace("\"", "\\\""));
        
        Response.Write(code);
        Response.End();
    }

    private string Execute()
    {
        int ModuleID = Convert.ToInt32(Request["mod"]);

        Findy.XsltDb.Transformer t = new Findy.XsltDb.Transformer(-1, ModuleID, -1);

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

