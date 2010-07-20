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

function mdo_fire(prms, self_tabmod)
{
    if ( prms == null )
        return;

    var subs = new Array();
    for ( var p in prms ) {
        var pName = prms[p];
        if ( pName.indexOf("$") === 0 ) {
            var search = "watch-" + pName.substring(1) + "-"; 
            jQuery("span[id^='" + search + "']").each(function(i){
                var tabmod = this.id.split("-")[2];
                if ( tabmod == self_tabmod ) return;
                var fun = "mdo_ajax_" + tabmod + "()";
                var found = false;
                for ( var i in subs )
                    if ( subs[i] == fun )
                        found = true;
                if ( !found )
                    subs[subs.length] = fun;
            });
        }
    }
    for ( var f in subs )
        eval(subs[f]);
}

function mdo_set_comm(mod, map)
{
    if ( map == null )
        return;

    for ( var pName in map )
    {
        var pVal = map[pName];

        if ( pVal == null )
            pVal = "";

        var hid;
        if ( pName.indexOf("$") === 0 )
           hid = document.getElementById("XsltDbGlobals");
        else
           hid = document.getElementById("mdo-hid-" + mod + "-value");

        if ( hid == null )
            return;

        pName = escape(pName);
        pVal = escape(pVal);

        var qs = "&" + hid.value;
        var regex = new RegExp("(&" + pName + "=)([^&]*)");

        if (qs.match(regex) == null)
        {
            if ( qs.length > 1 )
                qs += "&" + pName + "=" + pVal;
            else
                qs = pName + "=" + pVal;
        }
        else
            qs = qs.replace(regex, "$1" + pVal);

        if (qs.indexOf("&") === 0)
            qs = qs.substring(1);

        hid.value = qs;
    }
}

function mdo_serialize(p)
{
    jQuery("input,select,textarea").each(function(i) {

        if (this.name == "" || this.name == null || this.value == "" || this.value == null)
            return;
        if (this.name == "__VIEWSTATE")
            return;
        if (this.name.indexOf("__dnn") == 0)
            return;

        if ( (this.type == "checkbox" || this.type == "radio" ) && !this.checked)
            return;

        if (p[this.name])
            p[this.name] += "," + this.value;
        else
            p[this.name] = this.value;
    });
}

function mdo_ajax_comm(mod, map, p, alias)
{
    mdo_set_comm(mod, map);

    p["TabModuleID"] = mod;
    mdo_serialize(p);

    var search = window.location.search;
    if (alias != null) {
        if (search.length > 0)
            search += "&alias=" + alias;
        else
            search = "?alias=" + alias;
    }


    jQuery("#xsltdb-" + mod).load(ApplicationPath + "/DesktopModules/XsltDb/ajax.aspx" + search, p, function(responseText, textStatus, XMLHttpRequest) {
        if (map == null)
            return;
        var prms = new Array();
        for (var pName in map)
            prms[prms.length] = pName;

        mdo_fire(prms, mod);

        if (map != null) {
            var m = {};
            for (var k in map)
                if (k.indexOf("@") === 0)
                    m[k] = "";
            mdo_set_comm(mod, m);
        }
    });
}

function mdo_handler_comm(mod, p, callback) {

    p["TabModuleID"] = mod;

    mdo_serialize(p);


    var result = jQuery.ajax({
        url: ApplicationPath + "/DesktopModules/XsltDb/ajax.aspx" + window.location.search,
        data: p,
        type: 'POST',
        dataType: 'text',
        async: callback != null,
        success: function(data, textStatus, XMLHttpRequest) { if (callback != null) callback(data); }
    });

    if ( callback )
        return null;

    return result.responseText;
}

function mdo_falert(msg) {
    alert(msg);
    return false;
}

function mdo_enter(e, callback) {
    var code = e.charCode == null || e.charCode == 0 ? e.keyCode : e.charCode;
    if (code == 13) {
        if ( callback ) callback();
        return false;
    }
    return true;
}



//////////////////////////////// COOKIE STUFF ////////////////////////////////

function mdo_create_cookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toGMTString();
    }
    document.cookie = name + "=" + escape(value) + expires + "; path=/";
}
function mdo_read_cookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i].trim();
        if (c.indexOf(nameEQ) == 0) return unescape(c.substring(nameEQ.length, c.length));
    }
    return null;
}
function mdo_erase_cookie(name) {
    mdo_create_cookie(name, "", -1);
}
//////////////////////////////// COOKIE STUFF ////////////////////////////////