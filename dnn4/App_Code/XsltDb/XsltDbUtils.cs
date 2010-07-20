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
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;

using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Net;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke;
using DotNetNuke.Common;
using DotNetNuke.Data;


namespace Findy.XsltDb
{
    /// <summary>
    /// Summary description for XsltDbUtils
    /// </summary>
    public class XsltDbUtils
    {
        public const string globalControlName = "XsltDbGlobals";

        static XsltDbUtils()
        {
            Regex.CacheSize = 100;
        }

        public class aConfig
        {
            public aConfig(string XSLT, string Draft, bool IsSuper, string Name, string ServiceName)
            {
                this.XSLT = XSLT;
                this.Draft = Draft;
                this.IsSuper = IsSuper;
                this.Name = Name;
                this.ServiceName = ServiceName;
            }
            public string XSLT;
            public string Draft;
            public bool IsSuper;
            public string Name;
            public string ServiceName;
        }

        public static aConfig GetConfig(int ModuleID)
        {
            aConfig conf = null;
            using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_Select", ModuleID))
                if (r.Read())
                    conf = new aConfig(
                        r["XSLT"].ToString(),
                        r["Draft"].ToString(),
                        Convert.ToBoolean(r["IsSuper"]),
                        r["Name"].ToString(),
                        r["ServiceName"].ToString()
                        );
            return conf;

        }
        public static aConfig GetConfigByService(string ServiceName)
        {
            aConfig conf = null;
            using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_GetServiceConfig", PortalController.GetCurrentPortalSettings().PortalId, ServiceName))
                if (r.Read())
                    conf = new aConfig(
                        r["XSLT"].ToString(),
                        r["Draft"].ToString(),
                        Convert.ToBoolean(r["IsSuper"]),
                        r["Name"].ToString(),
                        r["ServiceName"].ToString()
                        );
            return conf;

        }
        private static aConfig GetConfig(int ModuleID, int TabID)
        {
            if (TabID == -1)
                return GetConfig(ModuleID);

            Dictionary<int, aConfig> tabConfigs;
            tabConfigs = (Dictionary<int, aConfig>)PageData.Get("tabConfigs");
            if (tabConfigs == null)
            {
                tabConfigs = new Dictionary<int, aConfig>();
                using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_Select_AllForTab", TabID))
                    while (r.Read())
                        tabConfigs[Convert.ToInt32(r["ModuleID"])] = new aConfig(
                            r["XSLT"].ToString(),
                            r["Draft"].ToString(),
                            (bool)r["IsSuper"],
                            r["Name"].ToString(),
                            r["ServiceName"].ToString());
                PageData.Set("tabConfigs", tabConfigs);
            }
            aConfig xslt;
            if (!tabConfigs.TryGetValue(ModuleID, out xslt))
                return null;
            return xslt;
        }

        public static string GetXSLT(int ModuleID, int TabID)
        {
            UserInfo ui = DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo();
            bool useDraft = ui.IsInRole("Administrators") || ui.IsSuperUser;

            aConfig c = GetConfig(ModuleID, TabID);
            if (c == null)
                return string.Empty;
            return useDraft ? c.Draft : c.XSLT;
        }

        public static bool GetIsSuper(int ModuleID, int TabID)
        {
            aConfig c = GetConfig(ModuleID, TabID);
            if (c == null)
                return false;
            return c.IsSuper;
        }
        public static string GetXSLT(int ModuleID)
        {
            UserInfo ui = DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo();
            bool useDraft = ui.IsInRole("Administrators") || ui.IsSuperUser;
            using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_Select", ModuleID))
                if (r.Read())
                    return r[useDraft ? "Draft" : "XSLT"].ToString();
            return string.Empty;
        }

        public static void AttachModuleToConfig(int ModuleID, string ConfigID)
        {
            DataProvider.Instance().ExecuteNonQuery("Findy_XsltDb_AttachModuleToConfig", ModuleID, ConfigID);
        }
        public static DataTable GetConfigs()
        {
            DataTable dt = new DataTable("conf");
            using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_GetAllConfigs"))
                dt.Load(r);
            return dt;
        }

        public static DataTable GetModulesWithSameConfig(int ModuleID)
        {
            DataTable dt = new DataTable("mod");
            using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_GetModulesWithSameConfig", ModuleID))
                dt.Load(r);
            return dt;
        }

        public static string GetXSLTData(int ModuleID)
        {
            using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_GetModuleData", ModuleID))
                if (r.Read())
                    return r[0].ToString();
            return "<xsltdb/>";
        }
        public static void ImportModule(int ModuleID, string data)
        {
            DataProvider.Instance().ExecuteNonQuery("Findy_XsltDb_ImportModule", ModuleID, data);
        }

        public static void SaveXSLT(int ModuleID, string xslt, bool isSuper, string Name, string ServiceName)
        {
            DataProvider.Instance().ExecuteNonQuery("Findy_XsltDb_Save", ModuleID, isSuper ? 1 : 0, xslt, Name, ServiceName);
        }
        public static void PublishAll()
        {
            DataProvider.Instance().ExecuteNonQuery("Findy_XsltDb_PublishAll",
                PortalController.GetCurrentPortalSettings().PortalId);
        }

        public static void Publish(int ModuleID)
        {
            DataProvider.Instance().ExecuteNonQuery("Findy_XsltDb_Publish", ModuleID);
        }

        public static DataTable GetModuleList(bool includeSuper)
        {
            string sql = @"
select
    m.ModuleID,
	t.TabName,
	t.Title as TabTitle,
	m.ModuleTitle,
	tm.PaneName
from {databaseOwner}[{objectQualifier}Findy_XsltDb_Modules] xslt
join {databaseOwner}[{objectQualifier}Findy_XsltDb_Configs] c on c.ConfigID = xslt.ConfigID
join {databaseOwner}[{objectQualifier}TabModules] tm on tm.ModuleID = xslt.ModuleID
join {databaseOwner}[{objectQualifier}Modules] m on m.ModuleID = xslt.ModuleID
join {databaseOwner}[{objectQualifier}Tabs] t on t.TabID = tm.TabID
where t.IsDeleted = 0 and m.Isdeleted = 0
and t.PortalID = {PortalID}
";

            if ( ! includeSuper ) sql += "and c.IsSuper = 0";

            sql = sql.Replace("{PortalID}", PortalController.GetCurrentPortalSettings().PortalId.ToString());

            DataTable dt = new DataTable("xsltdb");
            using (IDataReader dr = DataProvider.Instance().ExecuteSQL(sql))
                dt.Load(dr);

            return dt;
        }

        public static XsltDbPortalSettings GetPortalSettings(int PortalID)
        {
            XsltDbPortalSettings settings = new XsltDbPortalSettings();
            using (IDataReader r = DataProvider.Instance().ExecuteReader("Findy_XsltDb_SelectPortalSettings", PortalID))
            {
                if (r.Read())
                {
                    settings.UserName = r["xLogin"].ToString();
                    settings.DomainName = r["xDomain"].ToString();
                    settings.Password = r["xPassword"].ToString();
                    settings.ConnectionString = r["sqlConnectionString"].ToString();
                    return settings;
                }
            }
            return null;
        }

        public static string GetExceptionMessage(Exception ex)
        {
            return HttpUtility.HtmlEncode(ex.ToString()).Replace(Environment.NewLine, "<br />");
        }

        public static MdoQueryString GetQS(string slot)
        {
            return new MdoQueryString(HttpContext.Current.Request[slot] ?? string.Empty);
        }
        public static MdoQueryString GetQS(int TabModuleID)
        {
            return new MdoQueryString(HttpContext.Current.Request[GetHidName(TabModuleID)] ?? string.Empty);
        }

        public static string GetHidName(int TabModuleID)
        {
            return "mdo-hid-" + TabModuleID + "-value";
        }

        static Regex newline_r_n = new Regex("[^\\r]\\n");
        static Regex newline_r_rn = new Regex("\\r\\n");
        static Regex newline_r_r = new Regex("\\r[^\\n]");
        public static string UpdateRegexNewLinePattern(string pattern, string text)
        {
            int x_n = newline_r_n.Matches(text).Count;
            int x_rn = newline_r_rn.Matches(text).Count;
            int x_r = newline_r_r.Matches(text).Count;

            string nl = Environment.NewLine;

            if (x_rn >= x_n && x_rn >= x_r)
                nl = "\\r\\n";
            if (x_r >= x_n && x_r >= x_rn)
                nl = "\\r";
            if (x_n >= x_r && x_n >= x_rn)
                nl = "\\n";

            Regex r = new Regex(".\\\\n");

            return r.Replace(pattern, delegate(Match m)
            {
                if (m.Value.StartsWith("^"))
                    return "^" + nl[0] + nl[1];
                else
                    return m.Value[0] + nl;
            });

        }
    }

    public class XsltDbPortalSettings
    {
        public string UserName;
        public string DomainName;
        public string Password;
        public string ConnectionString;
    }

    public class Transformer
    {
        PortalModuleBase module = null;
        int mdoTabModuleID = -1;
        int mdoModuleID = -1;
        int mdoTabID = -1;
        int mdoPortalID = -1;
        int mdoUserID = -1;

        public Transformer(PortalModuleBase module)
        {
            this.module = module;
            dnnSettings = new theDnnSettings(ModuleID, UserID, PortalID, TabID);
        }
        public Transformer(int mdoTabModuleID, int mdoModuleID, int mdoTabID)
        {
            this.mdoUserID = UserController.GetCurrentUserInfo().UserID;
            this.mdoPortalID = PortalController.GetCurrentPortalSettings().PortalId;
            this.mdoTabID = mdoTabID;
            this.mdoModuleID = mdoModuleID;
            this.mdoTabModuleID = mdoTabModuleID;
            dnnSettings = new theDnnSettings(ModuleID, UserID, PortalID, TabID);
        }

        public class theDnnSettings
        {
            private ModuleInfo m = null;
            private UserInfo u = null;
            private PortalInfo p = null;
            private TabInfo t = null;

            private int ModuleID;
            private int UserID;
            private int PortalID;
            private int TabID;

            public theDnnSettings(int ModuleID, int UserID, int PortalID, int TabID)
            {
                this.TabID = TabID;
                this.ModuleID = ModuleID;
                this.PortalID = PortalID;
                this.UserID = UserID;
            }

            public ModuleInfo M { get {
                if (m == null)
                    m = new ModuleController().GetModule(ModuleID, TabID);
                return m;
            } }

            public UserInfo U { get {
                if (u == null)
                    u = new UserController().GetUser(PortalID, UserID);
                return u;
            } }

            public PortalInfo P { get {
                if (p == null)
                    p = new PortalController().GetPortal(PortalID);
                return p;
            } }

            public TabInfo T { get {
                if (t == null)
                    t = new TabController().GetTab(TabID);
                return t;
            } }
        }
        theDnnSettings dnnSettings = null;

        public theDnnSettings DnnSettings
        {
            get { return dnnSettings; }
        }

        internal int TabModuleID
        {
            get
            {
                if (module != null)
                    return module.TabModuleId;
                return mdoTabModuleID;
            }
        }

        internal int ModuleID
        {
            get
            {
                if (module != null)
                    return module.ModuleId;
                return mdoModuleID;
            }
        }

        internal int TabID
        {
            get
            {
                if (module != null)
                    return module.TabId;
                return mdoTabID;
            }
        }

        internal int PortalID
        {
            get
            {
                if (module != null)
                    return module.PortalId;
                return mdoPortalID;
            }
        }

        internal int UserID
        {
            get
            {
                if (module != null)
                    return module.UserId;
                return mdoUserID;
            }
        }

        public PortalModuleBase Module
        {
            get { return module; }
        }

        public bool IsNavigate
        {
            get
            {
                if (module == null)
                    return false;
                return ! module.Page.IsPostBack;
            }
        }

        public bool IsAjax
        {
            get { return module == null; }
        }

        public bool IsPostBack
        {
            get
            {
                if (module == null)
                    return false;
                return module.Page.IsPostBack;
            }
        }


        private const string xslLargeHeaderFooter = @"
<xsl:stylesheet version=""2.0""
   xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
   xmlns:msxsl=""urn:schemas-microsoft-com:xslt""
   exclude-result-prefixes=""msxsl mdo""
   xmlns:mdo=""urn:mdo""
>
  <xsl:output method=""html"" indent=""yes"" omit-xml-declaration=""yes""/>

  <xsl:template match=""/"">

{0}

  </xsl:template>

</xsl:stylesheet>
";

        private const string xslSmallHeaderFooter = @"
<xsl:stylesheet version=""2.0""
   xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
   xmlns:msxsl=""urn:schemas-microsoft-com:xslt""
   exclude-result-prefixes=""msxsl mdo""
   xmlns:mdo=""urn:mdo""
>

{0}

</xsl:stylesheet>
";
        static Regex PreRenderRegex = new Regex(@"<mdo:pre-render>(.*?)</mdo:pre-render>", RegexOptions.Singleline);
        public void PreRender(string xsl, string xml)
        {
            Match m = PreRenderRegex.Match(xsl);
            if (m.Success)
                JustTransform(m.Groups[1].Value, xml, false);
        }

        public string JustTransform(string xsl, string xml, bool debug)
        {
            xsl = PrepareXslt(xsl);

            Helper h = new Helper(this);
            string html = string.Empty;

            if ( debug )
                html = TransformDebug(xml, xsl, h, html);
            else
                html = TransformRelease(xml, xsl, h, html);

            return h.getWatchers() + html;
        }
        public string Transform(string xml, string xsl)
        {
            string inlineJS = string.Empty;
            Regex r = new Regex(@"<callable\s+js\s*=\s*""([^\(]+)\(([^\)]*)\)\""\s*>(.*?)</callable>", RegexOptions.Singleline);

            string callable = HttpContext.Current.Request["XsltDbCallable"];

            if (module == null && callable != null && callable.Length > 0)
                foreach (Match match in r.Matches(xsl))
                    if (match.Groups[1].Value == callable)
                        xsl = match.Groups[3].Value;

            xsl = r.Replace(xsl, delegate(Match match)
            {
                inlineJS += CreateHandlerJS(match.Groups[1].Value, match.Groups[2].Value, true);
                return "";
            });

            xsl = PreRenderRegex.Replace(xsl, "");

            string handlerJS = string.Empty;
            r = new Regex(@"\#\#handler\s*:\s*([^\(]+)\(([^\)]*)\)", RegexOptions.Multiline);
            Match m = r.Match(xsl);
            if (m.Success)
            {
                xsl = r.Replace(xsl, "").TrimStart();
                string fn = m.Groups[1].Value.Trim();
                string argList = m.Groups[2].Value.Trim();
                handlerJS = CreateHandlerJS(fn, argList, false);
            }

            r = new Regex(@"\#\#debug");
            bool debug = r.Match(xsl).Success;
            if ( debug )
                xsl = r.Replace(xsl, string.Empty).TrimStart();

            string html = string.Empty;
            if (handlerJS.Length == 0 || module == null)
                html = JustTransform(xsl, xml, debug);

            int noajaxBegin = html.IndexOf("<noajax>", StringComparison.CurrentCultureIgnoreCase);
            int noajaxEnd = html.IndexOf("</noajax>", StringComparison.CurrentCultureIgnoreCase);
            string noajax = string.Empty;
            if (noajaxBegin >= 0 && noajaxEnd >= 0)
            {
                if (!IsAjax)
                    noajax = html.Substring(noajaxBegin + 8, noajaxEnd - noajaxBegin - 8);
                html = html.Substring(0, noajaxBegin) + html.Substring(noajaxEnd + 9);
            }

            if ( IsAjax )
                return html;

            string qs;
            if (IsNavigate)
                qs = new MdoQueryString(HttpContext.Current.Request.QueryString).GetModuleParameters(TabModuleID, false);
            else
                qs = XsltDbUtils.GetQS("mdo-hid-" + TabModuleID + "-value").ToString(false);

            string comm_html = @"
<input type=""hidden"" id=""mdo-hid-{0}-value"" name=""mdo-hid-{0}-value"" value=""{1}"" />
<script type=""text/javascript"">
/*<![CDATA[*/
function mdo_submit_{0}(validator, map)
{
    if ( validator != null && !validator() )
        return;

    mdo_set_comm({0}, map);
    __doPostBack('{ClientID}', '');
}
function mdo_ajax_{0}(validator, map)
{
    if ( validator != null && !validator() )
        return;

    var p = {
        'TabID' : {TabID},
        'ModuleID' : {ModuleID}
    };

    mdo_ajax_comm({0}, map, p {Alias});
}
{handler}
/*]]>*/
</script>
{NOAJAX}
<div id=""xsltdb-{0}"" style=""padding:0; margin:0; border:0"">
";
            string handler_html = @"
<script type=""text/javascript"">
/*<![CDATA[*/
{handler}
/*]]>*/
</script>
<div id=""xsltdb-{0}"" style=""padding:0; margin:0; border:0"">
";
            string thehtml = handlerJS.Length > 0 ? handler_html : comm_html;

            string alias = module.PortalAlias.HTTPAlias.Contains("/") ? ", '" + module.PortalAlias.HTTPAlias + "'" : "";

            html = thehtml
                .Replace("{handler}", handlerJS + inlineJS)
                .Replace("{0}", module.TabModuleId.ToString())
                .Replace("{1}", qs)
                .Replace("{ModuleID}", module.ModuleId.ToString())
                .Replace("{ClientID}", module.ClientID.Replace("_", "$"))
                .Replace("{TabID}", module.TabId.ToString())
                .Replace("{Alias}", alias)
                .Replace("{NOAJAX}", noajax)
                + html + "</div>";

            return html;
        }

        public static string PrepareXslt(string xsl)
        {
            Regex rEv = new Regex(@"(\#\#\$)([^\n^\r]*)", RegexOptions.Multiline);
            xsl = rEv.Replace(xsl, @"<span id=""{mdo:watch('$2')}"" />");
            xsl = ReplaceValueOf(xsl);

            if (xsl.IndexOf("<xsl:stylesheet") < 0)
            {
                if (xsl.IndexOf("<xsl:template") < 0)
                    xsl = string.Format(xslLargeHeaderFooter, xsl);
                else
                    xsl = string.Format(xslSmallHeaderFooter, xsl);
            }

            return xsl;
        }

        private string TransformRelease(string xml, string xsl, Helper h, string html)
        {
            XslCompiledTransform t = (XslCompiledTransform)StaticCache.Get(xsl);
            if ( t == null )
            {
                t = new XslCompiledTransform();

                XsltSettings s = new XsltSettings(true, false);

                using (StringReader sr = new StringReader(xsl))
                    using (XmlReader xr = XmlReader.Create(sr))
                        t.Load(xr, s, new MdoResolver());

                StaticCache.Put(xsl, t);
            }

            using (StringReader sr = new StringReader(xml))
            {
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    using (StringWriter sw = new StringWriter())
                    {
                        XsltArgumentList xslArg = new XsltArgumentList();
                        xslArg.AddExtensionObject("urn:mdo", h);
                        DoTransform(t, xr, xslArg, sw);
                        sw.Flush();
                        html = sw.ToString();
                    }
                }
            }
            return html;
        }

        private string CreateTempFile(string content, string ext)
        {
            string f = Path.Combine(
                DnnSettings.P.HomeDirectoryMapPath,
                Guid.NewGuid().ToString() + "." + ext);
            using (StreamWriter sw = new StreamWriter(f))
                sw.Write(content);
            return f;
        }
        private string TransformDebug(string xml, string xsl, Helper h, string html)
        {
            XslCompiledTransform t = new XslCompiledTransform(true);
            XsltSettings s = new XsltSettings(true, false);

            string xslFile = CreateTempFile(xsl, "xslt");
            string xmlFile = CreateTempFile(xml, "xml");

            try
            {
                t.Load(xslFile, s, new MdoResolver());

                using (StringWriter sw = new StringWriter())
                {
                    XsltArgumentList xslArg = new XsltArgumentList();
                    xslArg.AddExtensionObject("urn:mdo", h);
                    t.Transform(xmlFile, xslArg, sw);
                    sw.Flush();
                    html = sw.ToString();
                }
                return html;
            }
            finally
            {
                File.Delete(xslFile);
                File.Delete(xmlFile);
            }
        }

        public static string ReplaceValueOf(string xsl)
        {
            Regex r = new Regex(@"mdo:[a-zA-Z0-9_]+\-[a-zA-Z0-9_\-]+\s*\(");
            xsl = r.Replace(xsl, delegate(Match m) {
                return m.Value.Replace("-", "");
            });

            r = new Regex(@"(?<!\\)(\{\{)([^\}]*)(\}\})", RegexOptions.Multiline);
            xsl = r.Replace(xsl, "<xsl:value-of select=\"$2\" />");

            r = new Regex(@"(?<!\\)(\{h\{)([^\}]*)(\}\})", RegexOptions.Multiline);
            xsl = r.Replace(xsl, "<xsl:value-of select=\"$2\" disable-output-escaping=\"yes\" />");

            xsl = xsl.Replace("\\{{", "{{");
            xsl = xsl.Replace("\\{h{", "{h{");

            return xsl;
        }

        private string CreateHandlerJS(string fn, string argList, bool callable)
        {
            string js = @"
function {fn}({argList})
{
    var p = {
        {callable}
        'TabID' : {TabID},
        'ModuleID' : {ModuleID}
    };
    {argAssign}
    return mdo_handler_comm({0}, p, callback);
}
";
            string argAssign = string.Empty;
            argList = argList.Trim();
            if (argList.Length > 0)
            {
                string[] args = argList.Split(',');
                string[] assigns = new string[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Trim();
                    assigns[i] = string.Format("p[\"{0}\"] = {0};", args[i]);
                }
                argAssign = string.Join(Environment.NewLine, assigns);
                argList = string.Join(", ", args).Trim();
            }
            if (argList.Length > 0) argList += ", ";
            argList += "callback";


            js = js
                .Replace("{fn}", fn.Trim())
                .Replace("{argList}", argList)
                .Replace("{argAssign}", argAssign.Trim())
                .Replace("{callable}", callable ? "'XsltDbCallable' : '" + fn + "'," : "");


            return js;
        }

        [FileIOPermission(SecurityAction.Deny)]
        [RegistryPermission(SecurityAction.Deny)]
        [UIPermission(SecurityAction.Deny)]
        [ReflectionPermission(SecurityAction.Deny)]
        [WebPermission(SecurityAction.Deny)]
        private void DoTransform(XslCompiledTransform t, XmlReader xr, XsltArgumentList xslArg, StringWriter sw)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            using (XmlWriter xw = XmlWriter.Create(sw, t.OutputSettings))
                t.Transform(xr, xslArg, xw, new MdoResolver());
        }
    }

    public class MdoQueryString : IEnumerable
    {
        NameValueCollection nvc;
        public MdoQueryString()
        {
            nvc = HttpUtility.ParseQueryString(string.Empty);
        }
        public MdoQueryString(NameValueCollection n) : this(n, string.Empty) { }
        public MdoQueryString(NameValueCollection n, string prefix)
        {
            nvc = HttpUtility.ParseQueryString(string.Empty);
            foreach (string key in n.AllKeys)
                nvc[prefix + key] = n[key];
        }
        public MdoQueryString(string qs)
        {
            nvc = HttpUtility.ParseQueryString(string.Empty);
            NameValueCollection n = HttpUtility.ParseQueryString(qs);
            foreach (string key in n.AllKeys)
                nvc[key] = n[key];
        }

        public object this[string pName, object dflt]
        {
            get
            {
                string pVal = this[pName];
                if (pVal == null || pVal.Length == 0)
                    return dflt;
                return pVal;
            }
        }
        public string this[string pName]
        {
            get { return nvc[pName]; }
            set { nvc[pName] = value; }
        }

        // Get{Global|Local}Parameters uses URL query string naming convension:
        //   - global parameters go as is
        //   - local module parameters go with -TabModuleID suffix
        private static Regex rParamTest = new Regex("^([^-]+)-(\\d+)$");
        public string GetGlobalParameters(bool includeCommands)
        {
            NameValueCollection n = HttpUtility.ParseQueryString(string.Empty);
            foreach (string key in nvc.AllKeys)
            {
                if (!includeCommands && (key.StartsWith("@")))
                    continue;
                if ( ! rParamTest.Match(key).Success )
                    n["$" + key] = nvc[key];
            }
            return n.ToString();
        }
        public string GetModuleParameters(int TabModuleID, bool includeCommands)
        {
            NameValueCollection n = HttpUtility.ParseQueryString(string.Empty);
            string tm = TabModuleID.ToString();
            foreach (string key in nvc.AllKeys)
            {
                if (!includeCommands && (key.StartsWith("@")))
                    continue;
                Match m = rParamTest.Match(key);
                if (m.Success && m.Groups[2].Value == tm)
                    n[m.Groups[1].Value] = nvc[key];
            }
            return n.ToString();
        }

        // ToString uses module parameters naming convension
        //  - global parameters start with "$"
        //  - local parameters go as is
        public override string ToString()
        {
            return nvc.ToString();
        }

        public string ToString(bool includeCommands)
        {
            if (includeCommands)
                return this.ToString();

            NameValueCollection n = HttpUtility.ParseQueryString(string.Empty);
            foreach (string key in nvc.AllKeys)
                if ((!key.StartsWith("@")) && (!key.StartsWith("$@")))
                    n[key] = nvc[key];
            return n.ToString();
        }

        internal void Append(string p)
        {
            NameValueCollection n = HttpUtility.ParseQueryString(p);
            foreach (string key in n.AllKeys)
                nvc[key] = n[key];
        }

        public IEnumerator GetEnumerator()
        {
            return nvc.GetEnumerator();
        }

        public string[] AllKeys
        {
            get { return nvc.AllKeys; }
        }

        internal string ParamsToUrl(bool includeCommands)
        {
            return ParamsToUrl(-1, includeCommands);
        }
        internal string ParamsToUrl(int TabModuleID, bool includeCommands)
        {
            NameValueCollection n = HttpUtility.ParseQueryString(string.Empty);
            foreach (string key in nvc.AllKeys)
            {
                if (includeCommands || ((!key.StartsWith("@")) && (!key.StartsWith("$@"))))
                {
                    if (key.StartsWith("$"))
                        n[key.Substring(1)] = nvc[key];
                    else if (TabModuleID != -1)
                        n[key + "-" + TabModuleID] = nvc[key];
                }
            }

            return n.ToString();
        }

        internal void Remove(string p)
        {
            nvc.Remove(p);
        }
    }
    public class RedirectException : Exception
    {
        private string url;
        public RedirectException()
        {
            url = Globals.NavigateURL();
        }
        public RedirectException(string url)
        {
            this.url = url;
        }
        public string URL { get { return url; } }
    }

    public class MdoResolver : XmlResolver
    {
        XmlUrlResolver resolver = new XmlUrlResolver();
        public override ICredentials Credentials
        {
            set {
                resolver.Credentials = value;
            }
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            Regex modR = new Regex(@"mdo:ModuleID\s*\(\s*(\d+)\s*\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = modR.Match(absoluteUri.AbsoluteUri);
            if (m.Success)
            {
                UserInfo ui = UserController.GetCurrentUserInfo();
                bool useDraft = ui.IsInRole("Administrators") || ui.IsSuperUser;

                XsltDbUtils.aConfig cnfg = Findy.XsltDb.XsltDbUtils.GetConfig(Convert.ToInt32(m.Groups[1].Value));
                MemoryStream ms = new MemoryStream();
                using (StreamWriter sw = new StreamWriter(ms))
                    sw.Write(Transformer.PrepareXslt(useDraft ? cnfg.Draft : cnfg.XSLT));
                return new MemoryStream(ms.ToArray());
            }

            Regex modS = new Regex(@"mdo:import\s*\(\s*(.+?)\s*\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match s = modS.Match(absoluteUri.AbsoluteUri);
            if (s.Success)
            {
                UserInfo ui = UserController.GetCurrentUserInfo();
                bool useDraft = ui.IsInRole("Administrators") || ui.IsSuperUser;

                XsltDbUtils.aConfig cnfg = Findy.XsltDb.XsltDbUtils.GetConfigByService(s.Groups[1].Value);
                MemoryStream ms = new MemoryStream();
                using (StreamWriter sw = new StreamWriter(ms))
                    sw.Write(Transformer.PrepareXslt(useDraft ? cnfg.Draft : cnfg.XSLT));
                return new MemoryStream(ms.ToArray());
            }
            if (absoluteUri.AbsoluteUri.StartsWith("mdo:"))
            {
                using (StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath("/Portals/_default/mdo/" + absoluteUri.AbsoluteUri.Substring(4))))
                {
                    string content = sr.ReadToEnd();
                    content = Transformer.ReplaceValueOf(content);
                    MemoryStream ms = new MemoryStream();
                    using (StreamWriter sw = new StreamWriter(ms))
                        sw.Write(content);
                    return new MemoryStream(ms.ToArray());
                }
            }
            return resolver.GetEntity(absoluteUri, role, ofObjectToReturn);
        }
    }

    public class PageData
    {
        static Dictionary<int, PageDataRequest> allData = new Dictionary<int, PageDataRequest>();

        public static object Get(string key)
        {
            lock (typeof(PageData))
            {
                int req = HttpContext.Current.Request.GetHashCode();
                PageDataRequest data;
                if (allData.TryGetValue(req, out data))
                    return data[key];
                return null;
            }
        }

        public static void Set(string key, object val)
        {
            lock (typeof(PageData))
            {
                int req = HttpContext.Current.Request.GetHashCode();
                PageDataRequest data;
                if (!allData.TryGetValue(req, out data))
                    allData[req] = data = new PageDataRequest(HttpContext.Current.Request);
                data[key] = val;

                if ((DateTime.Now - lastCollected).TotalSeconds > 2)
                    Collect();
            }
        }

        public static PageDataGetter Data { get { return new PageDataGetter(); } }

        static DateTime lastCollected = DateTime.Now;
        static void Collect()
        {
            lock (typeof(PageData))
            {
                foreach (int key in new List<int>(allData.Keys))
                    if (!allData[key].IsAlive)
                        allData.Remove(key);
                lastCollected = DateTime.Now;
            }
        }
    }

    internal class PageDataRequest
    {
        WeakReference request;
        Dictionary<string, object> data;

        public PageDataRequest(HttpRequest request)
        {
            this.request = new WeakReference(request, false);
            data = new Dictionary<string, object>();
        }

        public bool IsAlive { get { return request.IsAlive; } }
        public object this[string key]
        {
            get
            {
                object val;
                if (data.TryGetValue(key, out val))
                    return val;
                return null;
            }

            set
            {
                data[key] = value;
            }
        }
    }

    public class PageDataGetter
    {
        public object this[string key]
        {
            get { return PageData.Get(key); }
            set { PageData.Set(key, value); }
        }
    }

    public class StaticCache
    {
        class CacheItemContainer
        {
            public object key;
            public object value;
            public DateTime created;
        }
        static Dictionary<object, CacheItemContainer> Objects = new Dictionary<object, CacheItemContainer>();

        public static void Put(object key, object value)
        {
            lock (Objects)
            {
                CacheItemContainer c = new CacheItemContainer();
                c.key = key;
                c.value = value;
                c.created = DateTime.Now;
                Objects[key] = c;
            }
        }

        static DateTime LastCleared = DateTime.Now;
        public static object Get(object key)
        {
            lock (Objects)
            {
                if (LastCleared.AddHours(1) < DateTime.Now)
                {
                    foreach (object k in new List<object>(Objects.Keys))
                        if (Objects[k].created < LastCleared)
                            Objects.Remove(k);
                    LastCleared = DateTime.Now;
                }

                CacheItemContainer c = null;
                bool exists = Objects.TryGetValue(key, out c);
                if (exists)
                {
                    c.created = DateTime.Now;
                    return c.value;
                }
                return null;
            }



        }
    }
}