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
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Framework;
using DotNetNuke.Services.Search;

using System.Security.Permissions;
using System.Net;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

namespace Findy.XsltDb
{
    public partial class ViewXsltDb : PortalModuleBase, IActionable
    {
        static Regex texteditor = new Regex(@"<texteditor[^>]*>.*?</texteditor>", RegexOptions.Singleline);
        static Regex mdoAsp = new Regex(@"<\s*mdo:asp\s*[^>]*>|<\s*/\s*mdo:asp\s*>");
        string xml = @"<root></root>";

        XsltDbUtils.aConfig config;

        protected void Page_Init(object sender, EventArgs e)
        {
            try
            {
                jQuery.RequestRegistration();
                string html = string.Empty;
                try
                {
                    config = XsltDbUtils.GetConfig(this.ModuleId, this.TabId);
                    if (config != null && config.ActiveXslt.Length > 0)
                        html = new Transformer(this).Transform(xml, config.ActiveXslt, config.IsSuper);
                    else
                        html = "Use <b>Edit XSLT</b> link to setup an XSL transformation.";
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is RedirectException)
                    {
                        Response.Redirect((ex.InnerException as RedirectException).URL, true);
                        Response.End();
                        return;
                    }
                    if (UserInfo.IsInRole("Administrators") || UserInfo.IsSuperUser)
                        html = XsltDbUtils.GetExceptionMessage(ex);
                }

                html = mdoAsp.Replace(html, string.Empty);

                string[] htmls = texteditor.Split(html);
                MatchCollection ms = texteditor.Matches(html);
                //System.Diagnostics.Debugger.Break();

                for (int i = 0; i < htmls.Length; i++)
                {
                    if (i > 0)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(ms[i - 1].Value);

                        switch (doc.DocumentElement.Name)
                        {
                            case "texteditor":
                                {
                                    TextEditor te = (TextEditor)LoadControl("~/controls/TextEditor.ascx");
                                    foreach (XmlAttribute a in doc.SelectNodes("/texteditor/@*"))
                                    {
                                        switch (a.Name)
                                        {
                                            case "width": te.Width = new Unit(a.Value); break;
                                            case "height": te.Height = new Unit(a.Value); break;
                                            case "id": te.ID = a.Value; break;
                                            case "choose-mode": te.ChooseMode = a.Value == "true"; break;
                                        }
                                    }
                                    Controls.Add(te);
                                    te.HtmlEncode = false;
                                    te.Text = doc.DocumentElement.InnerText;
                                } break;
                        }
                    }
                    string ctrls = htmls[i];
                    if (ctrls.Contains("telerik:"))
                    {
                        ctrls = "<%@ Register TagPrefix=\"telerik\" Namespace=\"Telerik.Web.UI\" Assembly=\"Telerik.Web.UI\" %>" + ctrls;
                        ctrls = "<%@ Register TagPrefix=\"telerik\" Namespace=\"Telerik.Charting\" Assembly=\"Telerik.Web.UI\" %>" + ctrls;
                    }
                    if (ctrls.Contains("dnn:"))
                        ctrls = "<%@ Register TagPrefix=\"dnn\" Assembly=\"DotNetNuke\" Namespace=\"DotNetNuke.UI.WebControls\"%>" + ctrls;
                    Controls.Add(ParseControl(ctrls));
                }
                TestSqldataSource(this);
            }

            catch (Exception ex)
            {
                Controls.Add(new LiteralControl( XsltDbUtils.GetExceptionMessage(ex)));
            }
        }

        void TestSqldataSource(Control c)
        {
            if (c is SqlDataSource)
            {
                ((SqlDataSource)c).Selected += new SqlDataSourceStatusEventHandler(ViewXsltDb_SqlCommand);
                ((SqlDataSource)c).Selecting += new SqlDataSourceSelectingEventHandler(ViewXsltDb_Selecting);

                ((SqlDataSource)c).Updated += new SqlDataSourceStatusEventHandler(ViewXsltDb_SqlCommand);
                ((SqlDataSource)c).Updating += new SqlDataSourceCommandEventHandler(ViewXsltDb_Updating);

                ((SqlDataSource)c).Deleted += new SqlDataSourceStatusEventHandler(ViewXsltDb_SqlCommand);
                ((SqlDataSource)c).Deleting += new SqlDataSourceCommandEventHandler(ViewXsltDb_Updating);

                if ( config == null || !config.IsSuper )
                    SqlDataSourceLiteral.Text = "<p style=\"color:red;font-weight:bold;\">the module must be a super module to use SqlDataSource</p>";
            }
            else
                foreach (Control cc in c.Controls)
                    TestSqldataSource(cc);
        }

        void ViewXsltDb_Updating(object sender, SqlDataSourceCommandEventArgs e)
        {
            if (config == null || !config.IsSuper)
                e.Cancel = true;
        }

        void ViewXsltDb_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            if (config == null || !config.IsSuper)
                e.Cancel = true;
        }

        void ViewXsltDb_SqlCommand(object sender, SqlDataSourceStatusEventArgs e)
        {
            if (e.Exception != null)
            {
                SqlDataSourceLiteral.Text = XsltDbUtils.GetExceptionMessage(e.Exception);
                e.ExceptionHandled = true;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            try
            {
                if (PageData.Data["Validated"] == null)
                {
                    Page.Validate();
                    PageData.Data["Validated"] = true;
                }
                if ( config != null )
                  new Transformer(this).PreRender(config.ActiveXslt, xml, config.IsSuper);

                base.OnPreRender(e);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is RedirectException)
                {
                    Response.Redirect((ex.InnerException as RedirectException).URL, true);
                    Response.End();
                    return;
                }
                Controls.Add(new LiteralControl(XsltDbUtils.GetExceptionMessage(ex)));
            }
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            if (PageData.Data["XsltDbGlobalsSession"] == null)
            {
                PageData.Data["XsltDbGlobalsSession"] = true;

                string qs;
                if (Page.IsPostBack)
                    qs = XsltDbUtils.GetQS(XsltDbUtils.globalControlName).ToString(false);
                else
                    qs = new MdoQueryString(Request.QueryString).GetGlobalParameters(false);

                writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
                writer.AddAttribute(HtmlTextWriterAttribute.Id, XsltDbUtils.globalControlName);
                writer.AddAttribute(HtmlTextWriterAttribute.Name, XsltDbUtils.globalControlName);
                writer.AddAttribute(HtmlTextWriterAttribute.Value, qs, true);
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();

                string appPath = Request.ApplicationPath;
                if (appPath == "/")
                    appPath = "";

                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
                writer.RenderBeginTag(HtmlTextWriterTag.Script);
                writer.Write("var ApplicationPath = \"{0}\";", appPath);
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
                writer.AddAttribute(HtmlTextWriterAttribute.Src, appPath + "/DesktopModules/XsltDb/xsltdb.js");
                writer.RenderBeginTag(HtmlTextWriterTag.Script);
                writer.RenderEndTag();
            }
            try
            {
                base.RenderControl(writer);
            }
            catch (Exception ex) {
                writer.Write(XsltDbUtils.GetExceptionMessage(ex));
            }
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection Actions = new ModuleActionCollection();
                Actions.Add(
                    this.GetNextActionID(),
                    "Edit XSLT",
                    ModuleActionType.AddContent,
                    "",
                    "",
                    this.EditUrl(),
                    false,
                    SecurityAccessLevel.Admin,
                    true,
                    false);
                return Actions;
            }
        }
    }
}
