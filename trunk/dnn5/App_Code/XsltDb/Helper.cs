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
using System.Collections;
using System.Security.Cryptography;
using System.Net;
using System.Globalization;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using DotNetNuke.Data;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Mail;

//using Ionic.Zip;

namespace Findy.XsltDb
{
    public class Helper
    {
        private Transformer transformer;
        public Helper(Transformer transformer)
        {
            this.transformer = transformer;
        }

        public Transformer Transformer
        {
            get { return transformer; }
        }

        public object net(string proc)
        { return net(proc, new object[] { }); }

        public object net(string proc, object p1)
        { return net(proc, new object[] { p1 }); }

        public object net(string proc, object p1, object p2)
        { return net(proc, new object[] { p1, p2 }); }

        public object net(string proc, object p1, object p2, object p3)
        { return net(proc, new object[] { p1, p2, p3 }); }

        public object net(string proc, object p1, object p2, object p3, object p4)
        { return net(proc, new object[] { p1, p2, p3, p4 }); }

        public object net(string proc, object p1, object p2, object p3, object p4, object p5)
        { return net(proc, new object[] { p1, p2, p3, p4, p5 }); }

        public object net(string proc, object p1, object p2, object p3, object p4, object p5, object p6)
        { return net(proc, new object[] { p1, p2, p3, p4, p5, p6 }); }

        static Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();
        static Dictionary<string, object> objectCache = new Dictionary<string, object>();
        Dictionary<string, object> objectHandles = new Dictionary<string, object>();

        private bool isSimpleType(Type t)
        {
            if (t == typeof(Int16)) return true;
            if (t == typeof(Int32)) return true;
            if (t == typeof(Int64)) return true;

            if (t == typeof(UInt16)) return true;
            if (t == typeof(UInt32)) return true;
            if (t == typeof(UInt64)) return true;

            if (t == typeof(Double)) return true;
            if (t == typeof(Single)) return true;
            if (t == typeof(Decimal)) return true;

            if (t == typeof(Char)) return true;
            if (t == typeof(String)) return true;
            if (t == typeof(Byte)) return true;
            if (t == typeof(DateTime)) return true;

            return false;
        }

        public object netcall(string instance, string method)
        { return netcall(instance, method, new object[] { }); }

        public object netcall(string instance, string method, object p1)
        { return netcall(instance, method, new object[] { p1 }); }

        public object netcall(string instance, string method, object p1, object p2)
        { return netcall(instance, method, new object[] { p1, p2 }); }

        public object netcall(string instance, string method, object p1, object p2, object p3)
        { return netcall(instance, method, new object[] { p1, p2, p3 }); }

        public object netcall(string instance, string method, object p1, object p2, object p3, object p4)
        { return netcall(instance, method, new object[] { p1, p2, p3, p4 }); }

        public object netcall(string instance, string method, object p1, object p2, object p3, object p4, object p5)
        { return netcall(instance, method, new object[] { p1, p2, p3, p4, p5 }); }

        public object netcall(string instance, string method, object p1, object p2, object p3, object p4, object p5, object p6)
        { return netcall(instance, method, new object[] { p1, p2, p3, p4, p5, p6 }); }

        private object netcall(string instance, string method, object[] p)
        {
            return net(instance + ".." + method, p);
        }

        private object net(string proc, object[] p)
        {
            string[] parts = proc.Split('.');

            List<object> _prms = new List<object>();
            if ( parts[1].Length > 0 )
                _prms.Add(transformer.DnnSettings.P.PortalID);
            _prms.AddRange(p);
            object[] prms = _prms.ToArray();

            string objectName = parts[0] + "." + parts[1];
            object h;
            if (parts[1].Length != 0)
            {
                if (!objectCache.ContainsKey(objectName))
                {
                    h = Activator.CreateInstance(parts[0], "Mdo.XsltDb." + parts[1]).Unwrap();
                    objectCache[objectName] = h;
                }
                else
                    h = objectCache[objectName];
            }
            else
                h = objectHandles[parts[0]];

            MethodInfo theMethod = null;
            if (!methodCache.ContainsKey(proc))
            {
                foreach (MethodInfo mi in h.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance))
                {
                    if (mi.Name != parts[2])
                        continue;

                    ParameterInfo[] pi = mi.GetParameters();

                    if (pi.Length != prms.Length)
                        continue;

                    if (theMethod != null)
                        throw new Exception("Ambigous " + proc);

                    theMethod = mi;
                }
                if (theMethod == null)
                    throw new Exception("Can't find assembly/method for " + proc);

                methodCache[proc] = theMethod;
            }
            else
                theMethod = methodCache[proc];

            ParameterInfo[] pi2 = theMethod.GetParameters();
            for (int i = 0; i < prms.Length; i++)
            {
                Type pType = pi2[i].ParameterType;
                if (isSimpleType(pType))
                    prms[i] = Convert.ChangeType(cast(prms[i]), pType);
                else
                    prms[i] = objectHandles[prms[i].ToString()];
            }

            object ret = theMethod.Invoke(h, prms);
            if (isSimpleType(ret.GetType()))
                return ret;
            string guid = Guid.NewGuid().ToString();
            objectHandles[guid] = ret;
            return guid;
        }

        public XPathNavigator nodeset(string text)
        {
            try
            {
                return CreateNavigator(text);
            }
            catch (Exception)
            {
                return CreateRootOnlyNavigator();
            }
        }


        public XPathNavigator xml(string procedure, string names)
        { return xml(procedure, names, new object[] { }); }

        public XPathNavigator xml(string procedure, string names, object p1)
        { return xml(procedure, names, new object[] { p1 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2)
        { return xml(procedure, names, new object[] { p1, p2 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2, object p3)
        { return xml(procedure, names, new object[] { p1, p2, p3 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2, object p3, object p4)
        { return xml(procedure, names, new object[] { p1, p2, p3, p4 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2, object p3, object p4, object p5)
        { return xml(procedure, names, new object[] { p1, p2, p3, p4, p5 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2, object p3, object p4, object p5, object p6)
        { return xml(procedure, names, new object[] { p1, p2, p3, p4, p5, p6 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        { return xml(procedure, names, new object[] { p1, p2, p3, p4, p5, p6, p7 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        { return xml(procedure, names, new object[] { p1, p2, p3, p4, p5, p6, p7, p8 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8, object p9)
        { return xml(procedure, names, new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 }); }

        public XPathNavigator xml(string procedure, string names, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8, object p9, object p10)
        { return xml(procedure, names, new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 }); }

        private const string prefix = "mdo_xslt_";
        private const string xmlroot = "root";
        private XPathNavigator xml(string procedure, string names, params object[] parameters)
        {
            procedure = procedure.Replace("-", "_");
            bool IsSuper = XsltDbUtils.GetIsSuper(transformer.ModuleID, transformer.TabID);
            procedure = procedure.Trim();

            if (procedure[0] == ':' && !IsSuper)
                throw new Exception("Only superusers allowed to call ':' procedures");

            List<object> parameterList = new List<object>();
            if ( procedure[0] != ':' )
                parameterList.Add(transformer.PortalID.ToString());
            parameterList.AddRange(parameters);

            string procname = IsSuper && procedure[0] == ':' ? procedure.Substring(1) : prefix + procedure;

            // Change type to correctly handle result of xslt-built stuff
            for (int i = 1; i < parameterList.Count; i++)
                parameterList[i] = cast(parameterList[i]);

            if (names.Trim() == "$xml" || names.Trim() == "$scalar")
            {
                using (IDataReader dr = Data.ExecuteReader(procname, parameterList.ToArray()))
                {
                    if (dr == null)
                        throw new Exception("SQL execution failed.");

                    if (dr.Read())
                    {
                        if (names.Trim() == "$xml")
                            return CreateNavigator(dr[0].ToString());
                        return CreateRootOnlyNavigator(dr[0].ToString()).SelectSingleNode("/root/text()");
                    }
                }
                return CreateRootOnlyNavigator();
            }
            else
            {
                using (IDataReader reader = Data.ExecuteReader(procname, parameterList.ToArray()))
                    return GetXmlFromReader(reader, names);
            }
        }

        static private XPathNavigator CreateNavigator(string xml)
        {
            return new XPathDocument(new StringReader(xml)).CreateNavigator();
        }
        static private XPathNavigator CreateRootOnlyNavigator()
        {
            return CreateRootOnlyNavigator(string.Empty);
        }
        static private XPathNavigator CreateRootOnlyNavigator(string text)
        {
            return new XPathDocument(
                new StringReader("<root>" + HttpUtility.HtmlEncode(text) + "</root>")
                ).CreateNavigator();
        }

        public string text(object data)
        {
            if (data is XPathNodeIterator)
            {
                StringBuilder sb = new StringBuilder();
                foreach (XPathNavigator navi in data as XPathNodeIterator)
                    sb.Append(navi.OuterXml);
                return sb.ToString();
            }
            else if (data is XPathNavigator)
                return (data as XPathNavigator).OuterXml;
            else
                return data.ToString();
        }

        public string html(object data)
        {
            string str = text(data);

            Regex rx = new Regex("\\n");
            return rx.Replace(HttpUtility.HtmlEncode(str).Replace(" ", "&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp;"), "<br />");
        }

        public bool save(string procedure, object data)
        {
            return save(procedure, transformer.PortalID, data);
        }
        public bool save(string procedure, int PortalID, object data)
        {
            if (data == null)
                return true;

            procedure = procedure.Replace("-", "_");

            procedure = procedure.Trim();

            bool IsSuper = XsltDbUtils.GetIsSuper(transformer.ModuleID, transformer.TabID);

            if ( !IsSuper && PortalID != transformer.PortalID )
                throw new Exception("You only allowed to access portal #" + transformer.DnnSettings.P.PortalID);

            if (procedure[0] == ':' && !IsSuper)
                throw new Exception("Only superusers allowed to call ':' procedures");

            string procname = IsSuper && procedure[0] == ':' ? procedure.Substring(1) : prefix + procedure;

            try
            {
                Data.ExecuteNonQuery(procname, PortalID, text(data));
                return true;
            }

            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        public XPathNavigator sql(string sql, string names)
        { return execSql(sql, names); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1)
        { return execSql(sql, names, pn1, pv1); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2)
        { return execSql(sql, names, pn1, pv1, pn2, pv2); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2, string pn3, object pv3)
        { return execSql(sql, names, pn1, pv1, pn2, pv2, pn3, pv3); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2, string pn3, object pv3, string pn4, object pv4)
        { return execSql(sql, names, pn1, pv1, pn2, pv2, pn3, pv3, pn4, pv4); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2, string pn3, object pv3, string pn4, object pv4, string pn5, object pv5)
        { return execSql(sql, names, pn1, pv1, pn2, pv2, pn3, pv3, pn4, pv4, pn5, pv5); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2, string pn3, object pv3, string pn4, object pv4, string pn5, object pv5, string pn6, object pv6)
        { return execSql(sql, names, pn1, pv1, pn2, pv2, pn3, pv3, pn4, pv4, pn5, pv5, pn6, pv6); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2, string pn3, object pv3, string pn4, object pv4, string pn5, object pv5, string pn6, object pv6, string pn7, object pv7)
        { return execSql(sql, names, pn1, pv1, pn2, pv2, pn3, pv3, pn4, pv4, pn5, pv5, pn6, pv6, pn7, pv7); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2, string pn3, object pv3, string pn4, object pv4, string pn5, object pv5, string pn6, object pv6, string pn7, object pv7, object pn8, object pv8)
        { return execSql(sql, names, pn1, pv1, pn2, pv2, pn3, pv3, pn4, pv4, pn5, pv5, pn6, pv6, pn7, pv7, pn8, pv8); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2, string pn3, object pv3, string pn4, object pv4, string pn5, object pv5, string pn6, object pv6, string pn7, object pv7, object pn8, object pv8, object pn9, object pv9)
        { return execSql(sql, names, pn1, pv1, pn2, pv2, pn3, pv3, pn4, pv4, pn5, pv5, pn6, pv6, pn7, pv7, pn8, pv8, pn9, pv9); }
        public XPathNavigator sql(string sql, string names, string pn1, object pv1, string pn2, object pv2, string pn3, object pv3, string pn4, object pv4, string pn5, object pv5, string pn6, object pv6, string pn7, object pv7, object pn8, object pv8, object pn9, object pv9, object pn10, object pv10)
        { return execSql(sql, names, pn1, pv1, pn2, pv2, pn3, pv3, pn4, pv4, pn5, pv5, pn6, pv6, pn7, pv7, pn8, pv8, pn9, pv9, pn10, pv10); }

        private XPathNavigator execSql(string sql, string names, params object[] parameters)
        {
            sql = sql.Trim();
            try
            {
                bool IsSuper = XsltDbUtils.GetIsSuper(transformer.ModuleID, transformer.TabID);
                if (sql[0] == ':' && !IsSuper)
                    throw new Exception("You have no permission to execute free sql statements");

                XsltDbPortalSettings settings = XsltDbUtils.GetPortalSettings(transformer.PortalID);

                if (sql[0] != ':' && settings != null && settings.ConnectionString != null
                    && settings.ConnectionString.Length > 0)
                {
                    using (new Impersonator(settings.UserName, settings.DomainName, settings.Password))
                    {
                        return doExecSql(sql, names, parameters, settings.ConnectionString);
                    }
                }
                else
                {
                    if (IsSuper) // Only supermodules can access DNN database
                        return doExecSql(sql[0] == ':' ? sql.Substring(1) : sql, names, parameters);
                    return CreateRootOnlyNavigator("Only Super Modules can access DNN database.");
                }
            }
            catch (SecurityException sex)
            {
                throw new Exception(@"

THERE IS NO PERMISSION TO IMPERSONATE.
Check 'trust' attribute in web.config. Basicly, it may look like '<trust level=""Full"" />'.
Or, you can setup only unmanaged call permission. http://msdn.microsoft.com/en-us/library/wyts434y.aspx

", sex);
            }
            catch (Exception ex)
            {
                return CreateRootOnlyNavigator(ex.Message);
            }
        }

        private static XPathNavigator doExecSql(string sql, string names, object[] parameters, string cs)
        {
            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                SqlCommand cmd = c.CreateCommand();
                cmd.CommandText = sql;
                for (int i = 0; i < parameters.Length; i += 2)
                    cmd.Parameters.AddWithValue(cast(parameters[i]).ToString(), cast(parameters[i + 1]));

                if (names.Trim() == "$script")
                {
                    cmd.ExecuteNonQuery();
                    return CreateRootOnlyNavigator("ok");
                }
                else if (names.Trim() == "$xml")
                {
                    return CreateNavigator(cmd.ExecuteScalar().ToString());
                }
                else if (names.Trim() == "$scalar")
                {
                    return CreateRootOnlyNavigator(cmd.ExecuteScalar().ToString()).SelectSingleNode("/root/text()");
                }
                else
                {
                    using (IDataReader reader = cmd.ExecuteReader())
                        return GetXmlFromReader(reader, names);
                }
            }
        }

        private static XPathNavigator doExecSql(string sql, string names, object[] parameters)
        {
            List<SqlParameter> prms = new List<SqlParameter>();
            for (int i = 0; i < parameters.Length; i += 2)
                prms.Add(new SqlParameter(cast(parameters[i]).ToString(), cast(parameters[i + 1])));

            if (names.Trim() == "$script")
            {
                using (IDataReader dr = Data.ExecuteSQL(sql, prms.ToArray()))
                {
                    if (dr == null)
                        throw new Exception("SQL execution failed.");
                    return CreateRootOnlyNavigator("ok");
                }
            }
            else if (names.Trim() == "$xml" || names.Trim() == "$scalar")
            {
                using (IDataReader dr = Data.ExecuteSQL(sql, prms.ToArray()))
                {
                    if (dr == null)
                        throw new Exception("SQL execution failed.");
                    if (dr.Read())
                    {
                        if (names.Trim() == "$xml")
                            return CreateNavigator(dr[0].ToString());
                        return CreateRootOnlyNavigator(dr[0].ToString()).SelectSingleNode("/root/text()");
                    }

                }
                return CreateRootOnlyNavigator();
            }
            else
            {
                using (IDataReader dr = Data.ExecuteSQL(sql, prms.ToArray()))
                {
                    if (dr == null)
                        throw new Exception("SQL execution failed.");
                    return GetXmlFromReader(dr, names);
                }
            }
        }

        private static XPathNavigator GetXmlFromReader(IDataReader reader, string names)
        {
            DataSet ds = new DataSet(xmlroot);
            string[] narray = names.Replace(" ", "").Split(',');
            ds.Load(reader, LoadOption.OverwriteChanges, narray);

            StringWriter sw = new StringWriter();
            ds.WriteXml(sw);
            sw.Flush();
            return CreateNavigator(sw.ToString());
        }


        public string watch(string pname)
        {
            return "watch-" + pname + "-" + transformer.TabModuleID;
        }

        public bool isinrole(string role)
        {
            if (transformer.UserID == -1)
                return false;
            return transformer.DnnSettings.U.IsInRole(role);
        }

        public bool issuperuser()
        {
            if (transformer.UserID == -1)
                return false;
            return transformer.DnnSettings.U.IsSuperUser;
        }

        public decimal max(decimal x, decimal y)
        {
            return Math.Max(x, y);
        }
        public decimal min(decimal x, decimal y)
        {
            return Math.Min(x, y);
        }

        public XPathNodeIterator sequence(int count)
        {
            return sequence(0, count - 1, 1);
        }
        public XPathNodeIterator sequence(decimal from, decimal to, decimal step)
        {
            StringBuilder sb = new StringBuilder("<items>");
            for (decimal x = from; x <= to; x += step)
            {
                sb.Append("<item>");
                sb.Append(x);
                sb.Append("</item>");
            }
            sb.Append("</items>");
            return CreateNavigator(sb.ToString()).Select("//item/text()");
        }
        public XPathNodeIterator split(string str, string pattern)
        {
            Regex r = new Regex(pattern);
            string[] items = r.Split(str);
            XmlDocument doc = new XmlDocument();
            XmlElement xItems = doc.CreateElement("items");
            foreach (string item in items)
            {
                XmlElement xItem = doc.CreateElement("item");
                xItem.InnerText = item;
                xItems.AppendChild(xItem);
            }

            doc.AppendChild(xItems);
            return doc.CreateNavigator().Select("//item/text()");
        }

        public XPathNodeIterator match(string str, string pattern)
        {
            Regex r = new Regex(pattern, RegexOptions.Multiline);
            XmlDocument doc = new XmlDocument();
            XmlElement xItems = doc.CreateElement("matches");
            foreach (Match m in r.Matches(str))
            {
                XmlElement xItem = doc.CreateElement("match");
                int n = 0;
                foreach (Group g in m.Groups)
                {
                    if (g.Success)
                    {
                        XmlElement xGroup = doc.CreateElement("group");
                        XmlAttribute xGroupN = doc.CreateAttribute("n");
                        xGroupN.Value = n.ToString();
                        xGroup.Attributes.Append(xGroupN);
                        xGroup.InnerText = g.Value;
                        xItem.AppendChild(xGroup);
                    }
                    n++;
                }
                xItems.AppendChild(xItem);
            }

            doc.AppendChild(xItems);
            return doc.CreateNavigator().Select("/");
        }

        public object iif(bool selector, object v1, object v2)
        {
            if (selector)
                return v1;
            return v2;
        }
        public object coalesce(object v1, object v2)
        {
            if (v1 == null || cast(v1).ToString().Length == 0)
                return v2;
            return v1;
        }
        public object coalesce(object v1, object v2, object v3)
        { return coalesce(coalesce(v1, v2), v3); }
        public object coalesce(object v1, object v2, object v3, object v4)
        { return coalesce(coalesce(coalesce(v1, v2), v3), v4); }

        public object coalescenull(object nv, object v1, object v2)
        {
            string snv = (cast(nv) ?? "").ToString();
            string sv1 = (cast(v1) ?? "").ToString();

            if (sv1 == snv)
                return v2;
            return v1;
        }
        public object coalescenull(object nv, object v1, object v2, object v3)
        { return coalescenull(nv, coalescenull(nv, v1, v2), v3); }
        public object coalescenull(object nv, object v1, object v2, object v3, object v4)
        { return coalescenull(nv, coalescenull(nv, coalescenull(nv, v1, v2), v3), v4); }

        public int tabid(string marker, string where)
        {
            string fieldName;
            switch(where)
            {
                case "name": fieldName = "TabName"; break;
                case "title": fieldName = "Title"; break;
                case "desc": fieldName = "Description"; break;
                case "kw": fieldName = "KeyWords"; break;
                case "head": fieldName = "PageHeaderText"; break;
                default: return -1;
            }
            using (IDataReader r = Data.ExecuteSQL("select * from {databaseOwner}[{objectQualifier}Tabs] where " + fieldName + " like '%' + @Marker + '%%' and PortalID = @PortalID",
                new SqlParameter("@Marker", marker), new SqlParameter("@PortalID", transformer.DnnSettings.P.PortalID)))
            {
                if (r != null && r.Read())
                    return Convert.ToInt32(r["TabID"]);
            }
            return -1;
        }
        public object dnn(string name)
        {
            return GetPropertyValue(transformer.DnnSettings, name);
        }

        public object dnn(string name, object defaultValue)
        {
            object v = GetPropertyValue(transformer.DnnSettings, name);
            if (v == null || v == string.Empty)
                return defaultValue;
            return v;
        }

        private object GetPropertyValue(object obj, string name)
        {
            string[] parts = name.Split('.');
            foreach (string part in parts)
                if (obj != null)
                    obj = obj.GetType().InvokeMember(
                        part, System.Reflection.BindingFlags.GetProperty, null, obj, new object[] { });
            if (obj == null)
                return string.Empty; //XSLT extensions can't return null

            return obj;
        }

        private void SetPropertyValue(object obj, string name, object value)
        {
            SetPropertyValue(obj, null, name, value);
        }
        private void SetPropertyValue(object obj, Type type, string name, object value)
        {
            string[] parts = name.Split('.');
            for (int i = 0; i < parts.Length-1; i++)
                if (obj != null)
                    obj = obj.GetType().InvokeMember(
                        parts[i], BindingFlags.GetProperty, null, obj, new object[] { });
            if (obj == null)
                return;
            PropertyInfo pi;
            FieldInfo fi;
            if (type == null)
            {
                type = obj.GetType();
                pi = type.GetProperty(parts[parts.Length - 1], BindingFlags.Public | BindingFlags.Instance);
                fi = type.GetField(parts[parts.Length - 1], BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                pi = type.GetProperty(parts[parts.Length - 1], BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                fi = type.GetField(parts[parts.Length - 1], BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (pi == null && fi == null)
                {
                    pi = type.GetProperty(parts[parts.Length - 1], BindingFlags.Public | BindingFlags.Instance);
                    fi = type.GetField(parts[parts.Length - 1], BindingFlags.Public | BindingFlags.Instance);
                }
            }
            if (pi == null && fi == null)
                throw new Exception(string.Format("Can't find property '{0}' for the type '{1}'.", parts[parts.Length-1], type.FullName));
            object val;
            Type memberType = pi != null ? pi.PropertyType : fi.FieldType;
            try
            {
                val = Convert.ChangeType(value, memberType);
            }
            catch (Exception)
            {
                val = memberType.GetConstructor(new Type[] { typeof(string) }).Invoke(new object[] { value.ToString() });
            }
            if (pi != null)
                pi.SetValue(obj, val, null);
            if (fi != null)
                fi.SetValue(obj, val);

        }

        public string submit()
        { return perform("submit", null, new string[] { }, new string[] { }); }
        public string submit(string n1, string v1)
        { return perform("submit", null, new string[] { n1 }, new string[] { v1 }); }
        public string submit(string n1, string v1, string n2, string v2)
        { return perform("submit", null, new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string submit(string n1, string v1, string n2, string v2, string n3, string v3)
        { return perform("submit", null, new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string submit(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return perform("submit", null, new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string submit(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return perform("submit", null, new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        public string submit(string validator)
        { return perform("submit", validator, new string[] { }, new string[] { }); }
        public string submit(string validator, string n1, string v1)
        { return perform("submit", validator, new string[] { n1 }, new string[] { v1 }); }
        public string submit(string validator, string n1, string v1, string n2, string v2)
        { return perform("submit", validator, new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string submit(string validator, string n1, string v1, string n2, string v2, string n3, string v3)
        { return perform("submit", validator, new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string submit(string validator, string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return perform("submit", validator, new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string submit(string validator, string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return perform("submit", validator, new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        public string jsubmit()
        { return "javascript:" + perform("submit", null, new string[] { }, new string[] { }); }
        public string jsubmit(string n1, string v1)
        { return "javascript:" + perform("submit", null, new string[] { n1 }, new string[] { v1 }); }
        public string jsubmit(string n1, string v1, string n2, string v2)
        { return "javascript:" + perform("submit", null, new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string jsubmit(string n1, string v1, string n2, string v2, string n3, string v3)
        { return "javascript:" + perform("submit", null, new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string jsubmit(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return "javascript:" + perform("submit", null, new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string jsubmit(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return "javascript:" + perform("submit", null, new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        public string jsubmit(string validator)
        { return "javascript:" + perform("submit", validator, new string[] { }, new string[] { }); }
        public string jsubmit(string validator, string n1, string v1)
        { return "javascript:" + perform("submit", validator, new string[] { n1 }, new string[] { v1 }); }
        public string jsubmit(string validator, string n1, string v1, string n2, string v2)
        { return "javascript:" + perform("submit", validator, new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string jsubmit(string validator, string n1, string v1, string n2, string v2, string n3, string v3)
        { return "javascript:" + perform("submit", validator, new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string jsubmit(string validator, string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return "javascript:" + perform("submit", validator, new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string jsubmit(string validator, string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return "javascript:" + perform("submit", validator, new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        public string ajax()
        { return perform("ajax", null, new string[] { }, new string[] { }); }
        public string ajax(string n1, string v1)
        { return perform("ajax", null, new string[] { n1 }, new string[] { v1 }); }
        public string ajax(string n1, string v1, string n2, string v2)
        { return perform("ajax", null, new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string ajax(string n1, string v1, string n2, string v2, string n3, string v3)
        { return perform("ajax", null, new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string ajax(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return perform("ajax", null, new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string ajax(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return perform("ajax", null, new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        public string jajax()
        { return "javascript:" + perform("ajax", null, new string[] { }, new string[] { }); }
        public string jajax(string n1, string v1)
        { return "javascript:" + perform("ajax", null, new string[] { n1 }, new string[] { v1 }); }
        public string jajax(string n1, string v1, string n2, string v2)
        { return "javascript:" + perform("ajax", null, new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string jajax(string n1, string v1, string n2, string v2, string n3, string v3)
        { return "javascript:" + perform("ajax", null, new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string jajax(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return "javascript:" + perform("ajax", null, new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string jajax(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return "javascript:" + perform("ajax", null, new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        public string ajax(string validator)
        { return perform("ajax", validator, new string[] { }, new string[] { }); }
        public string ajax(string validator, string n1, string v1)
        { return perform("ajax", validator, new string[] { n1 }, new string[] { v1 }); }
        public string ajax(string validator, string n1, string v1, string n2, string v2)
        { return perform("ajax", validator, new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string ajax(string validator, string n1, string v1, string n2, string v2, string n3, string v3)
        { return perform("ajax", validator, new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string ajax(string validator, string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return perform("ajax", validator, new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string ajax(string validator, string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return perform("ajax", validator, new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        public string jajax(string validator)
        { return "javascript:" + perform("ajax", validator, new string[] { }, new string[] { }); }
        public string jajax(string validator, string n1, string v1)
        { return "javascript:" + perform("ajax", validator, new string[] { n1 }, new string[] { v1 }); }
        public string jajax(string validator, string n1, string v1, string n2, string v2)
        { return "javascript:" + perform("ajax", validator, new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string jajax(string validator, string n1, string v1, string n2, string v2, string n3, string v3)
        { return "javascript:" + perform("ajax", validator, new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string jajax(string validator, string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return "javascript:" + perform("ajax", validator, new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string jajax(string validator, string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return "javascript:" + perform("ajax", validator, new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        private string perform(string action, string validator, string[] names, string[] vals)
        {
            if (names.Length == 0)
                return "mdo_" + action + "_" + transformer.TabModuleID + "(" + (validator == null ? "" : validator) + ");";

            List<string> pairs = new List<string>();
            for (int i = 0; i < names.Length; i++)
            {
                if (vals[i].StartsWith("@"))
                    pairs.Add(string.Format("'{0}' : {1}",
                        HttpUtility.HtmlAttributeEncode(names[i].Replace("\\", "\\\\").Replace("'", "\\'")),
                        HttpUtility.HtmlAttributeEncode(vals[i].Substring(1))));
                else
                    pairs.Add(string.Format("'{0}' : '{1}'",
                        HttpUtility.HtmlAttributeEncode(names[i].Replace("\\", "\\\\").Replace("'", "\\'")),
                        HttpUtility.HtmlAttributeEncode(vals[i].Replace("\\", "\\\\").Replace("'", "\\'"))));
            }

            return string.Format(
                "mdo_{0}_{1}({3}, {{{2}}});",
                action,
                transformer.TabModuleID,
                string.Join(",", pairs.ToArray()),
                validator == null ? "null" : validator );
        }

        public string navigate(string n1, string v1)
        { return navigate(new string[] { n1 }, new string[] { v1 }); }
        public string navigate(string n1, string v1, string n2, string v2)
        { return navigate(new string[] { n1, n2 }, new string[] { v1, v2 }); }
        public string navigate(string n1, string v1, string n2, string v2, string n3, string v3)
        { return navigate(new string[] { n1, n2, n3 }, new string[] { v1, v2, v3 }); }
        public string navigate(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4)
        { return navigate(new string[] { n1, n2, n3, n4 }, new string[] { v1, v2, v3, v4 }); }
        public string navigate(string n1, string v1, string n2, string v2, string n3, string v3, string n4, string v4, string n5, string v5)
        { return navigate(new string[] { n1, n2, n3, n4, n5 }, new string[] { v1, v2, v3, v4, v5 }); }

        private string navigate(string[] pname, string[] pvalue)
        {
            MdoQueryString qs = new MdoQueryString(HttpContext.Current.Request.QueryString, string.Empty); // Normallize query string
            qs = new MdoQueryString(qs.ToString(false)); // Remove commands
            if (!transformer.IsNavigate)
            {
                qs.Append(XsltDbUtils.GetQS(XsltDbUtils.globalControlName).ParamsToUrl(false));
                Regex r = new Regex("^mdo-hid-(\\d+)-value$");
                foreach (string key in HttpContext.Current.Request.Form.AllKeys)
                {
                    Match m = r.Match(key);
                    if ( m.Success )
                    {
                        int tm = Convert.ToInt32(m.Groups[1].Value);
                        qs.Append(XsltDbUtils.GetQS(tm).ParamsToUrl(tm, false));
                    }
                }
            }

            string url = Globals.NavigateURL();
            if (url.Contains("?"))
            {
                string[] parts = url.Split('?');
                url = parts[0];
                qs.Append(parts[1]);
            }

            MdoQueryString paramQs = new MdoQueryString();

            for (int i = 0; i < pname.Length; i++)
                paramQs[pname[i]] = pvalue[i];
            qs.Append(paramQs.ParamsToUrl(transformer.TabModuleID, false));

            string TabId = qs["TabId"];
            if (TabId != null && TabId.Length > 0 && url.Contains("tabid/" + TabId))
                qs.Remove("TabId");

            string strQs = qs.ToString();
            if (strQs.Trim().Length > 0)
                url += "?" + strQs;
            return url;
        }

        public object param(string name)
        {
            return param(name, "");
        }

        List<string> requestedParams = new List<string>();
        public object param(string name, object dflt)
        {
            if (name.StartsWith("$"))
            {
                string p = name.Substring(1);
                if (!requestedParams.Contains(p))
                    requestedParams.Add(p);

                if (transformer.IsNavigate)
                    return request(p, dflt);
            }
            else
                if (transformer.IsNavigate)
                    return request(name + "-" + transformer.TabModuleID, dflt);

            string slot;
            if (name.StartsWith("$"))
                slot = "XsltDbGlobals";
            else
                slot = XsltDbUtils.GetHidName(transformer.TabModuleID);

            MdoQueryString qs = new MdoQueryString(request(slot, string.Empty).ToString());
            return qs[name, dflt];
        }

        internal string getWatchers()
        {
            string w = "";
            foreach (string pName in requestedParams)
                w += @"<span id=""" + watch(pName) + @""" />";

            return w;
        }

        public string redirect()
        {
            throw new RedirectException();
        }
        public string redirect(string url)
        {
            throw new RedirectException(url);
        }

        public object session(string name)
        {
            return session(name, "");
        }
        public object session(string name, object dflt)
        {
            string ret = (HttpContext.Current.Session[name] ?? "").ToString();
            if (ret.Length == 0)
                return dflt;
            return ret;
        }

        public object aspnet(string name)
        {
            if (name.StartsWith("Request."))
                return GetPropertyValue(HttpContext.Current.Request, name.Substring(8));

            if (name.StartsWith("Module."))
                return GetPropertyValue(transformer.Module, name.Substring(7));

            if (name.StartsWith("Page."))
                return GetPropertyValue(transformer.Module.Page, name.Substring(5));

            if (name.StartsWith("#"))
            {
                string id = name.Substring(1, name.IndexOf(".")-1);
                string property = name.Substring(id.Length+2);
                return GetPropertyValue(transformer.Module.FindControl(id), property);
            }

            return string.Empty;
        }


        public object assign(string name, object value)
        {
            if (name.StartsWith("Request."))
                SetPropertyValue(HttpContext.Current.Request, name.Substring(8), cast(value));

            if (name.StartsWith("Module."))
                SetPropertyValue(transformer.Module, name.Substring(7), cast(value));

            if (name.StartsWith("Page."))
                SetPropertyValue(transformer.Module.Page, typeof(DotNetNuke.Framework.CDefault), name.Substring(5), cast(value));

            if (name.StartsWith("#"))
            {
                string id = name.Substring(1, name.IndexOf(".") - 1);
                string property = name.Substring(id.Length + 2);
                SetPropertyValue(transformer.Module.FindControl(id), property, cast(value));
            }

            return string.Empty;
        }

        public object request(string name)
        {
            return request(name, "");
        }
        public object request(string name, object dflt)
        {
            string ret = (HttpContext.Current.Request[name] ?? "").ToString();
            if (ret.Length == 0)
                return dflt;
            return ret;
        }

        public object form(string name)
        {
            return form(name, "");
        }
        public object form(string name, object dflt)
        {
            string ret = (HttpContext.Current.Request.Form[name] ?? "").ToString();
            if (ret.Length == 0)
                return dflt;
            return ret;
        }

        public object querystring(string name)
        {
            return querystring(name, "");
        }
        public object querystring(string name, object dflt)
        {
            string ret = (HttpContext.Current.Request.QueryString[name] ?? "").ToString();
            if (ret.Length == 0)
                return dflt;
            return ret;
        }

        public string mappath(string relativePath)
        {
            return Path.Combine(transformer.DnnSettings.P.HomeDirectoryMapPath, relativePath);
        }
        public string mappath(string relativeFolder, string relativePath)
        {
            string absoluteFolder = Path.Combine(transformer.DnnSettings.P.HomeDirectoryMapPath, relativeFolder);
            return Path.Combine(absoluteFolder, relativePath);
        }

        public static string getFilesRoot()
        {
            string root = ConfigurationManager.AppSettings["XsltDbFiles"] ?? string.Empty;
            string home = new PortalController().GetPortal(PortalController.GetCurrentPortalSettings().PortalId).HomeDirectory.Replace("/", "\\");

            if (root.Length == 0)
                root = Path.Combine(home, "XsltDbFiles");

            if (root.IndexOf("{0}") >= 0)
                root = string.Format(root, home);

            return root;
        }

        private FileMan CreateFileManager()
        {
            return new FileMan(getFilesRoot());
        }

        public string deletefile(string path)
        {
            string ext = Path.GetExtension(path);
            FileMan m = CreateFileManager();
            string fullPath = m.FullName(path);

            // First delete file from db to prevent new requests for the file.
            Data.ExecuteNonQuery("Findy_XsltDb_Files_Delete",
                transformer.DnnSettings.P.PortalID, path);

            // now try to delete file and retry if it is locked.
            int i = 0;
            while (i < 5)
            {
                try
                {
                    if ( File.Exists(fullPath) )
                        File.Delete(fullPath);
                    if (Directory.Exists(fullPath + ".unzipped"))
                        Directory.Delete(fullPath + ".unzipped", true);
                    if (Directory.Exists(fullPath + ".thumb"))
                        Directory.Delete(fullPath + ".thumb", true);
                    break;
                }
                catch(Exception ex)
                {
                    System.Threading.Thread.Sleep(200);
                }
                i++;
            }
            return "ok";
        }
        public int fileid(string path)
        {
            string sql = "select FileID from {databaseOwner}[{objectQualifier}Findy_XsltDb_Files] where FilePath = @FilePath and PortalID = @PortalID";
            using (IDataReader r = Data.ExecuteSQL(sql,
                new SqlParameter("@FilePath", path),
                new SqlParameter("@PortalID", transformer.DnnSettings.P.PortalID)))
            {
                if (r.Read())
                    return Convert.ToInt32(r["FileID"]);
            }
            return -1;
        }

        public bool addalias(string alias)
        {
            return addalias(transformer.PortalID, alias);
        }
        public bool addalias(int PortalID, string alias)
        {
            bool IsSuper = XsltDbUtils.GetIsSuper(transformer.ModuleID, transformer.TabID);

            if ( !IsSuper && PortalID != transformer.PortalID )
                throw new Exception("Only supermodule can access foreign portals");

            PortalAliasController pac = new PortalAliasController();
            PortalAliasInfo paInfo = new PortalAliasInfo();
            paInfo.PortalID = PortalID;
            paInfo.HTTPAlias = alias;
            pac.AddPortalAlias(paInfo);
            return true;
        }

        public bool deletealias(string alias)
        {
            return deletealias(transformer.PortalID, alias);
        }
        public bool deletealias(int PortalID, string alias)
        {
            bool IsSuper = XsltDbUtils.GetIsSuper(transformer.ModuleID, transformer.TabID);

            if (!IsSuper && PortalID != transformer.PortalID)
                throw new Exception("Only supermodule can access foreign portals");

            PortalAliasController pac = new PortalAliasController();
            PortalAliasInfo paInfo = pac.GetPortalAlias(alias, PortalID);
            pac.DeletePortalAlias(paInfo.PortalAliasID);
            return true;
        }

        public bool aliasexists(string alias)
        {
            PortalAliasController pac = new PortalAliasController();
            return pac.GetPortalAliases().Contains(alias);
        }

        public string createdir()
        {
            FileMan m = CreateFileManager();
            return m.BuildInsideDir();
        }
/*
        public string createdir(string dir)
        {
            if (dir == null)
                dir = Guid.NewGuid().ToString();
            FileMan m = CreateFileManager();
            return m.PrepareDir(dir);
        }
*/
        private static string getExtError(string ext)
        {
            if ( ext == null || ext.Length == 0 )
                return "error:wrong-ext: no file extension provided";

            string allowedExts = HostSettings.GetHostSetting("FileExtensions").Replace(" ", "").ToLower();
            List<string> exts = new List<string>(allowedExts.Split(','));
            if (ext[0] == '.')
                ext = ext.Substring(1);
            if (!exts.Contains(ext))
                return "error:wrong-ext:" + ext;

            return null;
        }

        public XPathNavigator requestxml(string name)
        {
            return request_xml(name);
        }
        public XPathNavigator request_xml(string name)
        {
            try
            {
                HttpPostedFile file = HttpContext.Current.Request.Files[name];
                if (file == null || file.FileName.Length == 0)
                    return CreateRootOnlyNavigator();
                XmlDocument doc = new XmlDocument();
                doc.Load(file.InputStream);
                return doc.CreateNavigator();
            }
            catch (Exception)
            {
                return CreateRootOnlyNavigator("");
            }
        }

        public XPathNavigator requeststring(string name)
        {
            return request_string(name);
        }
        public XPathNavigator request_string(string name)
        {
            try
            {
                HttpPostedFile file = HttpContext.Current.Request.Files[name];
                if (file == null || file.FileName.Length == 0)
                    return CreateRootOnlyNavigator();
                XmlDocument doc = new XmlDocument();
                doc.Load(file.InputStream);
                return doc.CreateNavigator();
            }
            catch (Exception)
            {
                return CreateRootOnlyNavigator();
            }
        }

        public string requestfile(string name)
        {
            return requestfile(name, null);
        }
        public string requestfile(string name, string fn)
        {
            if (fn != null && fn.Length == 0)
                fn = null;

            HttpPostedFile file = HttpContext.Current.Request.Files[name];
            if (file == null || file.FileName.Length == 0)
                return "error:file-not-selected";
            string ext = Path.GetExtension(fn == null ? file.FileName : fn).ToLower();
            if ( fn == null )
                fn = ext;

            string extErr = getExtError(ext);
            if (extErr != null)
                return extErr;

            //string root = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, getFilesRoot());
            FileMan m = CreateFileManager();
            string path = m.PreparePath(fn);
            string fullPath = m.FullName(path);
            file.SaveAs(fullPath);
            Data.ExecuteNonQuery("Findy_XsltDb_Files_Insert", transformer.DnnSettings.P.PortalID, path);
            //if (ext == ".zip")
            //{
            //    string dir = fullPath+".unzipped";
            //    if ( ! Directory.Exists(dir) )
            //        Directory.CreateDirectory(dir);

            //    new ZipFile(fullPath).ExtractAll(dir);

            //    InvalidateExtracted(dir, exts);
            //}
            return path;
        }

        public string requestfolderfile(string name, string folder)
        {
            HttpPostedFile file = HttpContext.Current.Request.Files[name];
            string fn = Path.GetFileName(file.FileName);
            string ext = Path.GetExtension(fn).ToLower();

            string extErr = getExtError(ext);
            if (extErr != null)
                return extErr;

            FileMan m = CreateFileManager();
            string path = Path.Combine(folder, fn);
            string fullPath = m.FullName(path);
            file.SaveAs(fullPath);
            Data.ExecuteNonQuery("Findy_XsltDb_Files_Insert", transformer.DnnSettings.P.PortalID, path);

            return path;
        }

        //private void InvalidateExtracted(string dir, List<string> exts)
        //{
        //    foreach (string extracted in Directory.GetFiles(dir))
        //        if (!exts.Contains(Path.GetExtension(extracted).Substring(1).ToLower()))
        //            File.Delete(extracted);
        //    foreach (string extracted_dir in Directory.GetDirectories(dir))
        //        InvalidateExtracted(extracted_dir, exts);
        //}

        public string virtualfile(string path)
        {
            string appPath = HttpContext.Current.Request.ApplicationPath;
            if (appPath.Length > 1 )
                appPath += "/";

            return appPath + Path.Combine(getFilesRoot(), path).Replace("\\", "/");
        }

        public string thumbnail(string path, decimal maxWidth, decimal maxHeight)
        {
            try
            {
                FileMan m = CreateFileManager();
                string origFile = m.FullName(path);
                string thumbPath = CreateThumbnail(origFile, maxWidth, maxHeight);
                string tpath = m.RelativeName(thumbPath);
                return virtualfile(tpath);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public string downloadfile(string url)
        {
            return downloadfile(url, url.Substring(url.LastIndexOf('.')));
        }
        public string downloadfile(string url, string ext)
        {
            string extErr = getExtError(ext);
            if (extErr != null)
                return extErr;

            if (ext[0] != '.')
                ext = "." + ext;

            string root = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, getFilesRoot());
            FileMan m = CreateFileManager();
            string path = m.PreparePath(ext);
            string fullPath = m.FullName(path);
            WebClient client = new WebClient();
            try
            {
                client.DownloadFile(url, fullPath);
            }
            catch (Exception e)
            {
                return "error:" + e.Message;
            }
            Data.ExecuteNonQuery("Findy_XsltDb_Files_Insert", transformer.DnnSettings.P.PortalID, path);

            return path;
        }

        public string download(string url, string path)
        {
            return download(url, path, url.Substring(url.LastIndexOf('.')));
        }
        public string download(string url, string path, string ext)
        {
            string extErr = getExtError(ext);
            if (extErr != null)
                return extErr;

            WebClient client = new WebClient();
            if (ext[0] != '.' )
                ext = "." + ext;

            if (path != null && path.Length == 0)
                path = null;

            string fn = Guid.NewGuid().ToString() + ext;
            string relativePath = path == null ? fn : Path.Combine(path, fn);

            string fullPath;
            bool IsSuper = XsltDbUtils.GetIsSuper(transformer.ModuleID, transformer.TabID);

            if (relativePath[0] == ':' && IsSuper)
            {
                fullPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, relativePath.Substring(1));
            }
            else
            {
                if (relativePath[0] == ':')
                    relativePath = relativePath.Substring(1);
                fullPath = mappath(relativePath);
            }
                
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                client.DownloadFile(url, fullPath);
            }
            catch (Exception e)
            {
                return "error:" + e.Message;
            }

            return fn;
        }

        public static string CreateThumbnail(string origFile, double x, double y, double w, double h, double px, double py)
        {
            if (!File.Exists(origFile))
                return "";

            string key = "k" + x + "|" + y + "|" + w + "|" + h + "|" + px + "|" + py;
            string fnPart = Math.Abs(key.GetHashCode()).ToString();

            string thumbDir = origFile + ".thumb";
            string thumbPath = thumbDir + "\\" + fnPart + Path.GetExtension(origFile);
            if (!File.Exists(thumbPath))
            {
                lock (typeof(Helper))
                {
                    System.Drawing.Image original = System.Drawing.Image.FromFile(origFile);
                    Rectangle sourceRect = new Rectangle(
                        (int)(original.Width * x),
                        (int)(original.Height * y),
                        (int)(original.Width * w),
                        (int)(original.Height * h));

                    int PX = ((int)(px * original.Width)) - sourceRect.Left-1;
                    int PY = ((int)(py * original.Height)) - sourceRect.Top-1;

                    Bitmap bmp = new Bitmap(sourceRect.Width, sourceRect.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(original,
                            new Rectangle(0, 0, sourceRect.Width, sourceRect.Height),
                            sourceRect,
                            GraphicsUnit.Pixel);

                        using (Pen p = new Pen(Color.FromArgb(128, 255, 0, 0), 15))
                        {
                            p.StartCap = LineCap.Round;
                            p.EndCap = LineCap.ArrowAnchor;

                            g.DrawLine(p, 10, 10, PX, PY);
                        }
                    }
                    if (!Directory.Exists(thumbDir))
                        Directory.CreateDirectory(thumbDir);
                    bmp.Save(thumbPath, ImageFormat.Jpeg);
                }
            }

            return thumbPath;
        }
        public static string CreateThumbnail(string origFile, decimal maxWidth, decimal maxHeight)
        {
            //if (origFile.ToLower().Contains("thumb"))
            //{
            //    lock (typeof(Helper))
            //    {
            //        using (StreamWriter sw = new StreamWriter("D:\\thumb-log.txt", true))
            //        {
            //            sw.WriteLine(DateTime.Now);
            //            sw.WriteLine(origFile);
            //            sw.WriteLine(HttpContext.Current.Request.Url.OriginalString);
            //            sw.WriteLine("--------------------------------");
            //        }
            //    }
            //}

            if (!File.Exists(origFile))
                return "";

            string thumbDir = origFile + ".thumb";
            string thumbPath = thumbDir + "\\" + maxWidth + "x" + maxHeight + Path.GetExtension(origFile);
            if (!File.Exists(thumbPath))
            {
                lock (typeof(Helper))
                {
                    System.Drawing.Image original = System.Drawing.Image.FromFile(origFile);
                    decimal k = Math.Min(maxHeight / original.Height, maxWidth / original.Width);
                    Bitmap bmp = new Bitmap((int)Math.Round(k * original.Width), (int)Math.Round(k * original.Height));
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(original, 0, 0, bmp.Width, bmp.Height);
                    }
                    if (!Directory.Exists(thumbDir))
                        Directory.CreateDirectory(thumbDir);
                    bmp.Save(thumbPath, ImageFormat.Jpeg);
                }
            }

            return thumbPath;
        }

        public static byte[] CreateThumbnail(string origFile, decimal maxWidth, decimal maxHeight, PointF[] arrows)
        {
            if (!File.Exists(origFile))
                return null;

            System.Drawing.Image original = System.Drawing.Image.FromFile(origFile);
            decimal k = Math.Min(maxHeight / original.Height, maxWidth / original.Width);
            Bitmap bmp = new Bitmap((int)Math.Round(k * original.Width), (int)Math.Round(k * original.Height));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(original, 0, 0, bmp.Width, bmp.Height);

                int alpha = k < 0.5m ? 255 : 128;
                using ( SolidBrush b = new SolidBrush(Color.FromArgb(alpha, 255, 0, 0)) )
                {
                    using(Pen p = new Pen(b, (float)(13 * k + 3)))
                    {
                        foreach (PointF point in arrows)
                        {
                            p.StartCap = LineCap.Round;
                            p.EndCap = LineCap.ArrowAnchor;
                            float delta_x = point.X > 0.15f ? -0.1f : 0.1f;
                            float delta_y = point.Y > 0.15f ? -0.03f : 0.03f;
                            int x1 = (int)((point.X + delta_x) * bmp.Width);
                            int x2 = (int)(point.X * bmp.Width);
                            int y1 = (int)((point.Y + delta_y) * bmp.Height);
                            int y2 = (int)(point.Y * bmp.Height);

                            g.DrawLine(p, x1, y1, x2, y2);
                        }
                    }
                }
            }

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public XPathNavigator files(string path)
        {
            FileMan m = CreateFileManager();
            return BuildFilesList(m.FullName(path));
        }

        public XPathNavigator portalfiles()
        {
            return portalfiles(null);
        }
        public XPathNavigator portalfiles(string path)
        {
            return BuildFilesList(Path.Combine(transformer.DnnSettings.P.HomeDirectoryMapPath, path ?? ""));
        }

        private XPathNavigator BuildFilesList(string origDir)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root/>");
            if (Directory.Exists(origDir))
            {
                foreach (string file in Directory.GetFiles(origDir))
                {
                    XmlElement f = doc.CreateElement("file");
                    f.InnerText = Path.GetFileName(file);
                    doc.DocumentElement.AppendChild(f);
                }
                foreach (string dir in Directory.GetDirectories(origDir))
                {
                    XmlElement f = doc.CreateElement("dir");
                    f.InnerText = Path.GetFileName(dir);
                    doc.DocumentElement.AppendChild(f);
                }
            }

            return doc.CreateNavigator();
        }

        public string deleteportalfile(string path)
        {
            try
            {
                string fullpath = Path.Combine(transformer.DnnSettings.P.HomeDirectoryMapPath, path);
                if (File.Exists(fullpath))
                    File.Delete(fullpath);
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public XPathNavigator readportalxml(string path)
        {
            string fullpath = Path.Combine(transformer.DnnSettings.P.HomeDirectoryMapPath, path);
            XmlDocument doc = new XmlDocument();
            doc.Load(fullpath);
            return doc.CreateNavigator();
        }

        public string readfile(string path)
        {
            return readfile(path, "utf-8");
        }
        public string readfile(string path, string encoding)
        {
            try
            {
                FileMan m = CreateFileManager();
                using (StreamReader sr = new StreamReader(m.FullName(path), Encoding.GetEncoding(encoding)))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public XPathNavigator readxml(string path)
        {
            try
            {
                FileMan m = CreateFileManager();
                XmlDocument doc = new XmlDocument();
                doc.Load(m.FullName(path));
                return doc.CreateNavigator();
            }
            catch (Exception ex)
            {
                return CreateRootOnlyNavigator(ex.Message);
            }
        }

        // read-portal-tabbed-txt
        public XPathNavigator readportaltabbedtxt(string path)
        {
            return readportaltabbedtxt(path, string.Empty);
        }
        public XPathNavigator readportaltabbedtxt(string path, string tags)
        {
            return readportaltabbedtxt(path, tags, "utf-8");
        }
        static Regex unescapeRegex = new Regex(@"(\\\\|\\n|\\t)");
        public XPathNavigator readportaltabbedtxt(string path, string tags, string encoding)
        {
            try
            {
                string[] names = tags.Split(',');

                for (int i = 0; i < names.Length; i++)
                    names[i] = names[i].Trim();

                string fullpath = Path.Combine(transformer.DnnSettings.P.HomeDirectoryMapPath, path);
                return ReadTabbedFile(encoding, names, fullpath);
            }
            catch (Exception ex)
            {
                return CreateRootOnlyNavigator(ex.Message);
            }
        }

        public XPathNavigator readtabbedtxt(string path)
        {
            return readtabbedtxt(path, string.Empty);
        }
        public XPathNavigator readtabbedtxt(string path, string tags)
        {
            return readtabbedtxt(path, tags, "utf-8");
        }

        public XPathNavigator readtabbedtxt(string path, string tags, string encoding)
        {
            try
            {
                string[] names = tags.Split(',');

                for (int i = 0; i < names.Length; i++)
                    names[i] = names[i].Trim();

                FileMan m = CreateFileManager();
                string fullpath = m.FullName(path);
                return ReadTabbedFile(encoding, names, fullpath);
            }
            catch (Exception ex)
            {
                return CreateRootOnlyNavigator(ex.Message);
            }
        }

        
        private static XPathNavigator ReadTabbedFile(string encoding, string[] names, string fullpath)
        {
            using (StreamReader sr = new StreamReader(fullpath, Encoding.GetEncoding(encoding)))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<root/>");

                while (sr.Peek() >= 0)
                {
                    XmlElement row = doc.CreateElement("row");
                    string line = sr.ReadLine();
                    string[] values = line.Split('\t');
                    for (int i = 0; i < values.Length; i++)
                    {
                        string val = values[i];
                        val = unescapeRegex.Replace(val, delegate(Match m)
                        {
                            string s = m.Value;
                            switch (s)
                            {
                                case @"\\": return "\\";
                                case @"\n": return "\n";
                                case @"\t": return "\t";
                                default: return s;
                            }
                        });

                        string tag = i < names.Length && names[i].Length > 0 ? names[i] : "column-" + i;
                        XmlElement e = doc.CreateElement(tag);
                        e.InnerText = val;
                        row.AppendChild(e);
                    }
                    doc.DocumentElement.AppendChild(row);
                }
                return doc.CreateNavigator();
            }
        }

        public int writefile(string content, string path, string encoding)
        {
            try
            {
                FileMan m = CreateFileManager();
                using (StreamWriter sw = new StreamWriter(m.FullName(path), false, Encoding.GetEncoding(encoding)))
                {
                    sw.Write(content);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        public int writefile(string content, string path)
        {
            return writefile(content, path, "utf-8");
        }

        public string mail(string from, string to, string subject, string body)
        {
            return Mail.SendMail(
                from, to, "", "", "", MailPriority.Normal,
                subject, MailFormat.Html, Encoding.UTF8, body,
                new string[]{}, "", "", "", "", false);
        }

        public int userid(string login, string password)
        {
            UserLoginStatus status = new UserLoginStatus();
            UserInfo ui = UserController.ValidateUser(
                PortalController.GetCurrentPortalSettings().PortalId,
                login, password, "", "", "", ref status);

            if (ui == null)
                return -1;

            return ui.UserID;
        }

        public int login(string username, string password)
        {
            return login(username, password, false);
        }
        public int login(string username, string password, bool remember)
        {
            UserLoginStatus status = new UserLoginStatus();
            UserInfo ui = UserController.ValidateUser(PortalController.GetCurrentPortalSettings().PortalId, username, password, "", "", HttpContext.Current.Request.UserHostAddress, ref status);
            if (ui != null)
            {
                UserController.UserLogin(PortalController.GetCurrentPortalSettings().PortalId, ui, "", HttpContext.Current.Request.UserHostAddress, remember);
                return ui.UserID;
            }
            return -1;
        }

        public bool logout()
        {
            HttpContext.Current.Session.Abandon();
            FormsAuthentication.SignOut();
            return true;
        }

        public string password(int UserID)
        {
            return password(UserID, string.Empty);
        }
        public string password(int UserID, string passwordAnswer)
        {
            try
            {
                UserInfo ui = UserController.GetUserById(transformer.PortalID, UserID);
                return UserController.GetPassword(ref ui, passwordAnswer);
            }
            catch (Exception) { return string.Empty; }
        }

        public string createuser(string email, string password, string displayName, string phone)
        {
            UserInfo ui = new UserInfo();
            ui.PortalID = transformer.PortalID;
            ui.Username = email;
            ui.DisplayName = email;
            ui.Email = email;
            ui.DisplayName = displayName;
            ui.Profile.Telephone = phone;
            ui.Membership.Email = email;
            ui.Membership.Username = email;
            ui.Membership.Password = password;
            ui.Membership.Approved = true;
            return UserController.CreateUser(ref ui).ToString();
        }

        public string updateuser(int UserID, string displayName, string phone)
        {
            UserInfo ui = UserController.GetUserById(transformer.PortalID, UserID);
            ui.DisplayName = displayName;
            ui.Profile.Telephone = phone;

            UserController.UpdateUser(transformer.PortalID, ui);

            return string.Empty;
        }

        public bool changepassword(int UserID, string oldPassword, string newPassword)
        {
            try
            {
                UserInfo ui = UserController.GetUserById(transformer.PortalID, UserID);
                return UserController.ChangePassword(ui, oldPassword, newPassword);
            }
            catch (Exception) { }
            return false;
        }

        private Dictionary<string, object> variables = new Dictionary<string,object>();
        public object var(string name, object value)
        {
            object oldval = null;
            variables.TryGetValue(name, out oldval);
            if ( value != null )
                variables[name] = value;
            return oldval ?? "";
        }

        public object var(string name)
        {
            return var(name, null);
        }

        public string urlencode(string str)
        {
            return HttpContext.Current.Server.UrlEncode(str);
        }
        public string urlencodeunicode(string str)
        {
            return HttpUtility.UrlEncodeUnicode(str).Replace("+", "%u0020");
        }

        public string urldecode(string str)
        {
            return HttpContext.Current.Server.UrlDecode(str);
        }

        public string md5(string input)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            return sBuilder.ToString();
        }

        public object cookie(string name)
        {
            return cookie(name, "");
        }

        public object cookie(string name, object dflt)
        {
            HttpCookie c = HttpContext.Current.Request.Cookies[name];
            if (c == null)
                return dflt;
            return c.Value;
        }

        public object setcookie(string name, string value)
        {
            return setcookie(name, value, null);
        }

        public object setcookie(string name, string value, string expires)
        {
            HttpCookie c = new HttpCookie(name, value);
            if (expires == "delete")
                c.Expires = DateTime.Now.AddYears(-10);
            DateTime dtExpires;
            if (expires != null && expires.Length > 0 && DateTime.TryParse(expires, out dtExpires))
                c.Expires = dtExpires;

            HttpContext.Current.Response.Cookies.Add(c);

            return cookie(name);
        }
        public string cancelcookie(string name)
        {
            HttpCookie c = new HttpCookie(name, "delete");
            c.Expires = DateTime.Now.AddYears(-10);
            HttpContext.Current.Response.Cookies.Add(c);
            return "";
        }

        static Regex fieldregex = new Regex(@"^\d+\:");
        private static string GetDelim(string format)
        {
            Match m = fieldregex.Match(format);
            if (m.Success && m.Index == 0)
                return ",";
            else if (!format.StartsWith(":"))
                return ":";
            return "";
        }

        public string newid() { return Guid.NewGuid().ToString(); }

        public string fmtdate(object date, string format)
        {
            try
            {
                object trueDate = cast(date);
                if (trueDate.ToString().Trim().Length == 0)
                    return string.Empty;

                return string.Format(
                    "{0" + GetDelim(format) + format + "}",
                    Convert.ToDateTime(trueDate));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string fmtnumber(object num, string format)
        {
            try
            {
                object trueNum = cast(num);
                if (trueNum.ToString().Trim().Length == 0)
                    return string.Empty;

                return string.Format(
                    "{0" + GetDelim(format) + format + "}",
                    Convert.ToDecimal(trueNum));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public int indexof(string source, string pattern)
        {
            Regex r = new Regex(pattern);
            Match m = r.Match(source);
            if (m.Success)
                return m.Index;
            return -1;
        }

        public string replace(string source, string pattern, string replacer)
        {
            Regex r = new Regex(pattern);
            return r.Replace(source, replacer);
        }

        public string regexencode(string input)
        {
            const string regexchars = @"\^.+*?[](){}|";
            Regex r1 = new Regex(".");
            string group = r1.Replace(regexchars, "|\\$0");
            group = "(" + group.Substring(1) + ")";

            Regex r2 = new Regex(group);
            return r2.Replace(input, "\\$0");
        }

        public string htmlencode(string s)
        {
            return HttpUtility.HtmlEncode(s);
        }
        public string htmlattributeencode(string s)
        {
            return HttpUtility.HtmlAttributeEncode(s);
        }
        public string htmldecode(string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        public object editortext(string id)
        {
            return editortext(id, string.Empty);
        }

        static Regex tryUrl = new Regex("%3c|%3e");
        static Regex tryHtml = new Regex("&lt;|&gt;");
        public object editortext(string id, object dflt)
        {
            //string editorName = Transformer.Module.ClientID.Replace("_", "$") + "$" + id + "$" + id;
            string editorName = clientname(id) + "$" + id;
            string text = request(editorName, dflt).ToString();
            if ( text.Length == 0)
                return dflt;
            try
            {
                if (tryUrl.Matches(text).Count >= tryHtml.Matches(text).Count)
                    return urldecode(text);
                return htmldecode(text);
            }
            catch (Exception)
            {
                return text;
            }
        }

        public string clientid(string id)
        {
            return Transformer.Module.ClientID + "_" + id;
        }

        public string clientname(string id)
        {
            return Transformer.Module.UniqueID + "$" + id;
        }

        public DateTime date()
        {
            return DateTime.Now;
        }

        public string date(string format)
        {
            return fmtdate(date(), format);
        }

        public bool today(DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        public string transform(string xml, string xsl)
        {
            return this.transformer.Transform(xml, xsl, false);
        }

        public string test_with_delay(int delay)
        {
            System.Threading.Thread.Sleep(delay);
            return DateTime.Now.ToString();
        }

        public int delay(int delay)
        {
            System.Threading.Thread.Sleep(delay);
            return 0;
        }

        public XPathNavigator imagesize(string path)
        {
            FileMan m = CreateFileManager();
            using (System.Drawing.Image i = System.Drawing.Image.FromFile(m.FullName(path)))
            {
                return CreateNavigator("<root><width>" + i.Width + "</width><height>"+i.Height+"</height></root>");
            }
        }

        private static object cast(object val)
        {
            if (val is XPathNodeIterator)
            {
                XPathNodeIterator iterator = (XPathNodeIterator)val;
                if (iterator.MoveNext())
                    val = iterator.Current.Value;
                else
                    val = "";
            }
            if (val is XPathNavigator)
                val = (val as XPathNavigator).Value;

            return val;
        }

        public bool isajax() { return transformer.IsAjax; }
        public bool issubmit() { return transformer.IsPostBack; }
        public bool isnavigate() { return transformer.IsNavigate; }

        public string culture()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
        }

        public object gettabsetting(string key)
        {
            return gettabsetting(key, string.Empty);
        }
        public object gettabsetting(string key, object dflt)
        {
            TabController tc = new TabController();

            MethodInfo mi = tc.GetType().GetMethod("GetTabSettings", new Type[] { typeof(int) });
            Hashtable h = (Hashtable)mi.Invoke(tc, new object[] { Transformer.TabID });
            object val = h[key];
            if (val == null || val.ToString().Trim().Length == 0)
                return dflt;
            return val;
        }

        public object settabsetting(string key, string val)
        {
            TabController tc = new TabController();
            MethodInfo mi = tc.GetType().GetMethod("UpdateTabSetting", new Type[] { typeof(int), typeof(string), typeof(string), });
            mi.Invoke(tc, new object[] { Transformer.TabID, key, val });
            return true;
        }

        public object getmodulesetting(string key)
        {
            return getmodulesetting(key, string.Empty);
        }
        public object getmodulesetting(string key, object dflt)
        {
            ModuleController mc = new ModuleController();
            object val = mc.GetModuleSettings(Transformer.ModuleID)[key];
            if (val == null || val.ToString().Trim().Length == 0)
                return dflt;
            return val;
        }
        public bool setmodulesetting(string key, string val)
        {
            ModuleController mc = new ModuleController();
            mc.UpdateModuleSetting(Transformer.ModuleID, key, val);
            return true;
        }
        public object gettabmodulesetting(string key)
        {
            return gettabmodulesetting(key, string.Empty);
        }
        public object gettabmodulesetting(string key, object dflt)
        {
            ModuleController mc = new ModuleController();
            object val = mc.GetTabModuleSettings(Transformer.TabModuleID)[key];
            if (val == null || val.ToString().Trim().Length == 0)
                return dflt;
            return val;
        }
        public bool settabmodulesetting(string key, string val)
        {
            ModuleController mc = new ModuleController();
            mc.UpdateTabModuleSetting(Transformer.TabModuleID, key, val);
            return true;
        }

        public string dnnobjectqualifier()
        {
            return DataProvider.Instance().ObjectQualifier;
        }
        public string dnndatabaseowner()
        {
            return DataProvider.Instance().DatabaseOwner;
        }

        public string dnnpreparesql(string sql)
        {
            return sql
                .Replace("{databaseOwner}", DataProvider.Instance().DatabaseOwner)
                .Replace("{objectQualifier}", DataProvider.Instance().ObjectQualifier);
        }

        /*
        public object savefromrequest(string table, string prefix)
        {
            throw new NotImplementedException();

            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("data");
            XmlAttribute a = doc.CreateAttribute("table");
            a.Value = table;
            root.Attributes.Append(a);

            prefix += "_";

            foreach (string key in HttpContext.Current.Request.Params.AllKeys)
            {
                string col = null;
                if ( key.StartsWith(clientname(prefix)))
                    col = key.Substring(clientname(prefix).Length);
                if ( key.StartsWith(prefix) )
                    col = key.Substring(prefix.Length);
                if ( col != null )
                {
                    XmlElement f = doc.CreateElement("f");
                    a = doc.CreateAttribute("c");
                    a.Value = col;
                    f.Attributes.Append(a);
                    a = doc.CreateAttribute("v");
                    a.Value = HttpContext.Current.Request.Params[key];
                    f.Attributes.Append(a);
                    root.AppendChild(f);
                }
            }

            doc.AppendChild(root);

            
            try
            {
                return Data.ExecuteScalar("Findy_XsltDb_SaveEntity", doc.OuterXml);
            }
            catch(Exception)
            {
                if ( isinrole("Administrators") )
                    throw;
            }
            return string.Empty;
        }
         * */

        public string eventsource()
        {
            return eventsource(false);
        }
        public string eventsourcefull()
        {
            return eventsource(true);
        }
        public string eventsource(bool full)
        {
            Type t = typeof(System.Web.UI.Page);

            FieldInfo fi = t.GetField("_registeredControlThatRequireRaiseEvent", BindingFlags.NonPublic | BindingFlags.Instance);

            object v = fi.GetValue(HttpContext.Current.Handler);

            if (v != null)
            {
                Control c = (System.Web.UI.Control)v;
                return full ? c.UniqueID : c.ID;
            }
            
            return string.Empty;
        }

        public string resolveurl(string relative)
        {
            return transformer.Module.ResolveUrl(relative);
        }

        public string serviceurl(string serviceName)
        {
            return
                HttpContext.Current.Request.ApplicationPath +
                "DesktopModules/XsltDb/ws.aspx?service=" + urlencodeunicode(HttpUtility.HtmlDecode(serviceName)) + "&mod=" + transformer.ModuleID;
        }


    }

    public class XmlContainer
    {
        object xml;
        public XmlContainer(object xml)
        {
            this.xml = xml;
        }

        public XPathNodeIterator Select(string xpath)
        {
            if (xml is XPathNavigator)
                return (xml as XPathNavigator).Select(xpath);

            if (xml is XPathNodeIterator)
                return (xml as XPathNodeIterator).Current.Select(xpath);

            return null;
        }
    }

    public class Data
    {
        public static IDataReader ExecuteSQL(string SQL, params SqlParameter[] parameters)
        {
            return ExecuteSQL(SQL, true, parameters);
        }
        private static IDataReader ExecuteSQL(string SQL, bool prepare, params SqlParameter[] parameters)
        {
            if (prepare)
            {
                SQL = SQL.Replace("{databaseOwner}", DataProvider.Instance().DatabaseOwner);
                SQL = SQL.Replace("{objectQualifier}", DataProvider.Instance().ObjectQualifier);
            }
            SqlConnection cnn = new SqlConnection(DataProvider.Instance().ConnectionString);
            cnn.Open();
            SqlCommand cmd = new SqlCommand(SQL, cnn);
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        private static string BuildSQL(string procedure, params object[] parameters)
        {
            procedure = DataProvider.Instance().ObjectQualifier + procedure;
            string cacheKey = "procedures::" + procedure;
            bool valid = false;
            if (StaticCache.Get(cacheKey) == null)
            {

                string check = "select name from sys.procedures where name = '" + procedure.Replace("'", "''") + "'";
                using (IDataReader r = ExecuteSQL(check))
                    valid = r.Read();
                if (valid)
                    StaticCache.Put(cacheKey, true);
            }
            else
                valid = true;

            if (valid)
            {
                procedure = DataProvider.Instance().DatabaseOwner + procedure;
                string sql = "exec " + procedure;
                if (parameters != null && parameters.Length > 0)
                    foreach (object value in parameters)
                        sql += " '" + value.ToString().Replace("'", "''") + "',";
                if ( sql.EndsWith(",") )
                    sql = sql.Substring(0, sql.Length - 1);
                return sql;
            }
            else
                throw new Exception("Procedure '" + procedure + "' can't be found.");
        }
        public static IDataReader ExecuteReader(string procedure, params object[] parameters)
        {
            return ExecuteSQL(BuildSQL(procedure, parameters), false);
        }

        public static object ExecuteScalar(string procedure, params object[] parameters)
        {
            using (IDataReader r = ExecuteReader(procedure, parameters))
                if (r.Read())
                    return r[0];
            return string.Empty;
        }

        public static void ExecuteNonQuery(string procedure, params object[] parameters)
        {
            using (IDataReader r = ExecuteReader(procedure, parameters))
                return;
        }
    }
}