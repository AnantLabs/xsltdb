<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditXslt.ascx.cs" Inherits="Findy.XsltDb.EditXsltDb" %>

<script type="text/javascript" src="<%=DotNetNuke.Common.Globals.ResolveUrl("~/DesktopModules/XsltDb/edit_area_0_8_2/edit_area_loader.js") %>"></script>

	<script language="Javascript" type="text/javascript">
	    editAreaLoader.init({
	        id: '<%= FindControl("txtXSLT").ClientID %>'
            , start_highlight: true
            , autocompletion: true
            , allow_toggle: false
            , language: "en"
            , syntax: "xsl"
            , syntax_selection_allow: "js,xsl"
            , toolbar: "save, |, autocompletion, fullscreen, |, search, go_to_line, |, undo, redo, |, select_font, |, syntax_selection, |, change_smooth_selection, highlight, reset_highlight, help"
            , show_line_colors: true
            , plugins: "autocompletion"
            , save_callback: "saveConfig"
	    });

        function copytext() {
            document.getElementById('<%= FindControl("txtXSLT").ClientID %>').value = editAreaLoader.getValue('<%= FindControl("txtXSLT").ClientID %>');
        }
        function saveConfig() {
            copytext();
            __doPostBack('<%= FindControl("linkPublish").UniqueID %>', '');
        }

</script>

<style type="text/css">
.vchekmiddle, .vchekmiddle label, .vchekmiddle input{vertical-align:middle;}
</style>

<div class="Normal" style="text-align:left;">

<% if (this.IsSuperModule && !UserInfo.IsSuperUser) { %>
<span class="normal" style="color:red">
This is a super module. Only super users can edit XSLT.
You are only allowed to select another cofiguration.</span><br /><br />
<% } %>

<div style="display:<%= IsConfigSelection ? "none" : "block" %>">
<asp:LinkButton ID="btnSelectConfig" runat="server" oncommand="btnSelectConfig_Command">Select existing configuration</asp:LinkButton>
<br /><br />

<% if (UserInfo.IsSuperUser || !this.IsSuperModule) { %>
<table class="normal" style="width:100%"><tr><td>
    <b>XSL Transformation Editor</b> (Module ID = <%= this.ModuleId %>)
</td><td style="text-align:right">
<% if (UserInfo.IsSuperUser) { %>
<script type="text/javascript">
    function EnsureConfigName() {
        var ch = document.getElementById("<%= this.chIsSuper.ClientID %>");
        document.getElementById("name-table").style.display = ch.checked ? "block" : "none";
    }
</script>
<asp:CheckBox ID="chIsSuper" runat="server" Text="Super Module" CssClass="vchekmiddle" onclick="EnsureConfigName()" /><br />
<% } %>
</td></tr></table>
<% if (UserInfo.IsSuperUser) { %>
<div id="name-table" style="margin-bottom:3px">
<span style="width:200px;">Configuration Name:</span>
<asp:TextBox ID="txtConfigName" runat="server" Width="400" ></asp:TextBox>
</div>
<script type="text/javascript">
    EnsureConfigName();
</script>
<% } %>

<span style="width:200px;">Alias:</span><asp:TextBox ID="txtConfigAlias" runat="server"></asp:TextBox><br/>
<asp:TextBox ID="txtXSLT" runat="server" TextMode="MultiLine"  Width="100%" Height="500px"></asp:TextBox>

<span class="normal" style="color:red">
<asp:Literal ID="litErr" runat="server"></asp:Literal>
</span>
<br />
<br />
<asp:LinkButton ID="linkPublish" runat="server" CssClass="CommandButton" oncommand="linkPublish_Command" OnClientClick="copytext()">Update &amp; Publish</asp:LinkButton>
&nbsp;|&nbsp;
<asp:LinkButton ID="linkUpdate" runat="server" oncommand="linkUpdate_Command" CssClass="CommandButton" OnClientClick="copytext()">Update</asp:LinkButton>
&nbsp;
<asp:LinkButton ID="linkCancel" runat="server" oncommand="linkCancel_Command" CssClass="CommandButton">Cancel</asp:LinkButton>
&nbsp;|&nbsp;
<asp:LinkButton ID="linkPublishAll" runat="server" CssClass="CommandButton" oncommand="linkPublishAll_Command" OnClientClick="copytext()">Publish All</asp:LinkButton>
&nbsp;|&nbsp;
<a target="_blank" style="color:Black" href="http://xsltdb.codeplex.com/documentation">Help &amp; Docs</a>
&nbsp;|&nbsp;
<a target="_blank" style="color:Black" href="http://xsltdb.codeplex.com">XsltDb Project Home Page</a>
<% } %>
</div>
<% if ( IsConfigSelection ) { %>

<table class="normal" style="border-collapse:collapse" cellspacing="0" cellpadding="3">
<tr style="background-color:#dddddd;border:solid #dddddd 1px;font-weight:bold">
<td>Select Existing Configuration</td>
</tr>
<asp:Repeater runat="server" ID="repConfigs">
<ItemTemplate>
<tr style="border:solid #dddddd 1px">
<td>

<asp:LinkButton
    ID="linkTitle"
    CommandName="SelectConfig"
    CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ConfigID") %>'
    Text='<%# DataBinder.Eval(Container.DataItem, "Name").ToString() %>'
    runat="server"
    OnCommand="SelectConfig" />
</td>
</tr>
</ItemTemplate>
</asp:Repeater>
</table>
<br /><asp:LinkButton ID="linkBackToXSLT" runat="server" CssClass="CommandButton" 
        oncommand="linkBackToXSLT_Command">Back To XSLT Editor</asp:LinkButton>
<% } %>

</div>