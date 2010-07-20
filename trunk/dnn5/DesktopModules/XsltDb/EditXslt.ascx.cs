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
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.Services;
using System.Web.UI.WebControls;
using System.Web.Script.Services;

using DotNetNuke;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;

namespace Findy.XsltDb
{

    public partial class EditXsltDb : PortalModuleBase
    {
        protected bool IsConfigSelection = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    XsltDbUtils.aConfig config = XsltDbUtils.GetConfig(this.ModuleId);
                    if (config != null)
                    {
                        txtXSLT.Text = config.Draft;
                        txtConfigAlias.Text = config.ServiceName;
                        this.ViewState["IsSuper"] = chIsSuper.Checked = config.IsSuper;
                        txtConfigName.Text = config.Name;
                    }
                    else
                        this.ViewState["IsSuper"] = false;
                    txtXSLT.Text += string.Empty.PadRight(20, '\n');

                }
                catch (Exception ex)
                {
                    litErr.Text = XsltDbUtils.GetExceptionMessage(ex);
                }
            }
        }

        protected bool IsSuperModule
        {
            get
            {
                if (UserInfo.IsSuperUser)
                    return chIsSuper.Checked;
                else
                    return Convert.ToBoolean(ViewState["IsSuper"]);
            }
        }

        protected bool IsSharedConfig
        {
            get { return XsltDbUtils.GetModulesWithSameConfig(this.ModuleId).Rows.Count > 1; }
        }

        protected void linkUpdate_Command(object sender, CommandEventArgs e)
        {
            try
            {
                XsltDbUtils.SaveXSLT(this.ModuleId, txtXSLT.Text.Trim(), chIsSuper.Checked, txtConfigName.Text, txtConfigAlias.Text);
                Response.Redirect(Globals.NavigateURL());
            }
            catch (Exception ex)
            {
                litErr.Text = XsltDbUtils.GetExceptionMessage(ex);
            }
        }

        protected void linkCancel_Command(object sender, CommandEventArgs e)
        {
            Response.Redirect(Globals.NavigateURL());
        }
        protected void btnSelectConfig_Command(object sender, CommandEventArgs e)
        {
            IsConfigSelection = true;
            repConfigs.DataSource = XsltDbUtils.GetConfigs();
            repConfigs.DataBind();
        }
        protected void linkBackToXSLT_Command(object sender, CommandEventArgs e)
        {
            IsConfigSelection = false;
            repConfigs.DataSource = null;
            repConfigs.DataBind();
        }
        protected void SelectConfig(object sender, CommandEventArgs e)
        {
            XsltDbUtils.AttachModuleToConfig(this.ModuleId, e.CommandArgument.ToString());
            Response.Redirect(Globals.NavigateURL());
        }
        protected void linkPublishAll_Command(object sender, CommandEventArgs e)
        {
            try
            {
                XsltDbUtils.SaveXSLT(this.ModuleId, txtXSLT.Text.Trim(), chIsSuper.Checked, txtConfigName.Text, txtConfigAlias.Text);
                XsltDbUtils.PublishAll();
                Response.Redirect(Globals.NavigateURL());
            }
            catch (Exception ex)
            {
                litErr.Text = XsltDbUtils.GetExceptionMessage(ex);
            }
        }
        protected void linkPublish_Command(object sender, CommandEventArgs e)
        {
            try
            {
                XsltDbUtils.SaveXSLT(this.ModuleId, txtXSLT.Text.Trim(), chIsSuper.Checked, txtConfigName.Text, txtConfigAlias.Text);
                XsltDbUtils.Publish(this.ModuleId);
                Response.Redirect(Globals.NavigateURL());
            }
            catch (Exception ex)
            {
                litErr.Text = XsltDbUtils.GetExceptionMessage(ex);
            }
        }
    }
}