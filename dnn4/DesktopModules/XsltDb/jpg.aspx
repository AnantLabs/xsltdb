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
        Response.ContentType = "image/jpeg";
        Response.ContentEncoding = Encoding.UTF8;

        string encoded = Request["enc"];
        string path = Request["file"];
        int w = Convert.ToInt32(Request["w"]);
        int h = Convert.ToInt32(Request["h"]);

        if ( encoded == "1" )
            path = path.Replace("_", "\\");

        string fullPath = System.IO.Path.Combine(Request.PhysicalApplicationPath, Findy.XsltDb.Helper.getFilesRoot());

        fullPath = System.IO.Path.Combine(fullPath, path);

        string thumbPath = Findy.XsltDb.Helper.CreateThumbnail(fullPath, w, h);
        using (System.IO.FileStream file = System.IO.File.OpenRead(thumbPath))
        {
            byte[] arr = new byte[file.Length];
            file.Read(arr, 0, arr.Length);
            Response.OutputStream.Write(arr, 0, arr.Length);
        }

        Response.Expires = 120;
        Response.End();
    }

</script>

