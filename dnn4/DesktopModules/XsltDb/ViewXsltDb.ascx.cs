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
    public partial class ViewXsltDb : PortalModuleBase, IActionable, ISearchable
    {
        static Regex texteditor = new Regex(@"<texteditor[^>]*>.*?</texteditor>|<treeview[^>]*>.*?</treeview>|<aspnet-control[^>]*>.*?</aspnet-control>", RegexOptions.Singleline);
        static Regex mdoAsp = new Regex(@"<\s*mdo:asp\s*[^>]*>|<\s*/\s*mdo:asp\s*>");
        string xml = @"<root></root>";
        string xsl;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                jQueryRequestRegistration();
                string html = string.Empty;
                try
                {
                    xsl = XsltDbUtils.GetXSLT(this.ModuleId, this.TabId);
                    if (xsl.Length > 0)
                        html = new Transformer(this).Transform(xml, xsl);
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
                        XPathQuery query = new XPathQuery(doc);
                        doc.LoadXml(ms[i - 1].Value);

                        switch (doc.DocumentElement.Name)
                        {
                            case "aspnet-control":
                                {
                                    string[] parts = query["/aspnet-control/@class"].Split(',');
                                    Control c = (Control)Activator.CreateInstance(parts[1], parts[0]).Unwrap();
                                    foreach (XmlAttribute a in doc.SelectNodes("/aspnet-control/@*"))
                                    {
                                        if (a.Name == "class") continue;
                                        SetPropertyValue(c, a.Name, a.Value);
                                    }

                                    Controls.Add(c);

                                } break;
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
                            case "treeview":
                                {
                                    object tv = TelerikFactory.Create("RadTreeView");
                                    foreach (XmlNode n in doc.SelectNodes("//node"))
                                    {
                                        object node = TelerikFactory.Create("RadTreeNode");
                                        foreach (XmlAttribute a in n.SelectNodes("@*"))
                                            SetPropertyValue(node, a.Name, a.Value);

                                        TelerikFactory.Add(tv, "Nodes", node);
                                    }

                                    foreach (XmlAttribute a in doc.SelectNodes("/treeview/@*"))
                                        SetPropertyValue(tv, a.Name, a.Value);

                                    if (doc.SelectSingleNode("/treeview/menu/item") != null)
                                    {
                                        object m = TelerikFactory.Create("RadTreeViewContextMenu");
                                        foreach (XmlElement el in doc.SelectNodes("/treeview/menu/item"))
                                        {
                                            object mi = TelerikFactory.Create("RadMenuItem");
                                            foreach (XmlAttribute a in el.SelectNodes("@*"))
                                                SetPropertyValue(mi, a.Name, a.Value);
                                            TelerikFactory.Add(m, "Items", mi);
                                        }
                                        TelerikFactory.Add(tv, "ContextMenus", m);
                                    }
                                    Controls.Add((Control)tv);

                                } break;
                        }
                    }
                    //Controls.Add(new LiteralControl(htmls[i]));
                    Controls.Add(ParseControl(htmls[i]));

                }
            }
            catch (Exception ex)
            {
                Controls.Add(new LiteralControl( XsltDbUtils.GetExceptionMessage(ex)));
            }
        }

		private void jQueryRequestRegistration()
		{
			if (Context.Items["jquery_registered"] == null)
			{
                HttpContext.Current.Items["jquery_registered"] = true;
                Literal headscript = new Literal();
                headscript.Text = "<script type='text/javascript' src='" + DotNetNuke.Common.Globals.ResolveUrl("~/Resources/Shared/Scripts/jquery/jquery.min.js") + "'/>";
				Page.Header.Controls.Add(headscript);
			}
		}

        protected override void OnPreRender(EventArgs e)
        {
            try
            {
                new Transformer(this).PreRender(xsl, xml);
            }
            catch (Exception ex)
            {
                Controls.Add(new LiteralControl(XsltDbUtils.GetExceptionMessage(ex)));
            }

            base.OnPreRender(e);
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

        #region ISearchable Members

        public SearchItemInfoCollection GetSearchItems(ModuleInfo ModInfo)
        {
            return new SearchItemInfoCollection();
        }

        #endregion

        private void SetPropertyValue(object obj, string name, object value)
        {
            string[] parts = name.Split('.');
            for (int i = 0; i < parts.Length - 1; i++)
                if (obj != null)
                    obj = obj.GetType().InvokeMember(
                        parts[i], BindingFlags.GetProperty, null, obj, new object[] { });
            if (obj == null)
                return;
            PropertyInfo pi = obj.GetType().GetProperty(parts[parts.Length - 1], BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding);
            object val;
            if (pi.PropertyType.IsEnum)
                val = Enum.Parse(pi.PropertyType, value.ToString());
            else
                val = Convert.ChangeType(value, pi.PropertyType);
            pi.SetValue(obj, val, null);
        }
    }

    public class XPathQuery
    {
        XmlDocument doc;
        public XPathQuery(XmlDocument doc)
        {
            this.doc = doc;
        }

        public string this[string xpath]
        {
            get {
                XmlNode n = doc.SelectSingleNode(xpath);
                if (n == null) return null;
                else
                    if (n is XmlAttribute)
                        return n.Value;
                    else
                        return n.InnerText;
            }
        }
    }

    public class TelerikFactory
    {
        public static object Create(string cls)
        {
            return Activator.CreateInstance("Telerik.Web.UI", "Telerik.Web.UI." + cls).Unwrap();
        }

        public static void Add(object obj, string collection, object objectToAdd)
        {
            object coll = obj.GetType().GetProperty(collection).GetValue(obj, null);
            coll.GetType().GetMethod("Add").Invoke(coll, new object[]{objectToAdd});
        }
    }
}
