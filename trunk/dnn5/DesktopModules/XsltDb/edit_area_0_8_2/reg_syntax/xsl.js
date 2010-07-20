/*
* last update: 2010-06-15
*/

editAreaLoader.load_syntax["xsl"] = {
    'DISPLAY_NAME': 'XSL'
	, 'COMMENT_SINGLE': {}
	, 'COMMENT_MULTI': { '<!--': '-->' }
	, 'QUOTEMARKS': { 1: "'"/*, 2: '"'*/, dontescape:true }
	, 'KEYWORD_CASE_SENSITIVE': true
	, 'KEYWORDS': {
}
	, 'OPERATORS': ['+', '*', '-', ' div ', ' mod ', ' or ', ' and '
	]
	, 'DELIMITERS': [
	]
	, 'REGEXPS': {

	    'funcs': {
	        'search': '([^\\w-]|^)(position|last|current|document|count|sum|name|local-name|string|concat|contains|starts-with|substring|substring-before|substring-after|translate|lower-case|upper-case|ceiling|floor|number|round|false|true|not|boolean|lang|msxsl:node-set)(\\s*\\()'
			, 'class': 'funcs'
			, 'modifiers': 'g'
			, 'execute': 'before' // before or after
	    }
		, 'cdatas': {
	    'search': '()(<!\\[CDATA\\[[^ú]*?\\]\\]>)()'
			, 'class': 'cdatas'
			, 'modifiers': 'gm'
			, 'execute': 'before' // before or after
		}
		, 'asptags': {
		    'search': '(<)(/?asp:[\\w-]+)([^>]*>)'
			, 'class': 'asptags'
			, 'modifiers': 'gi'
			, 'execute': 'before' // before or after
		}
		, 'dnntags': {
		    'search': '(<)(/?dnn:[\\w-]+)([^>]*>)'
			, 'class': 'dnntags'
			, 'modifiers': 'gi'
			, 'execute': 'before' // before or after
		}
		, 'teleriktags': {
		    'search': '(<)(/?telerik:[\\w-]+)([^>]*>)'
			, 'class': 'teleriktags'
			, 'modifiers': 'gi'
			, 'execute': 'before' // before or after
		}
		, 'xsltags': {
		    'search': '(<)(/?xsl:[\\w-]+)([^>]*>)'
			, 'class': 'xsltags'
			, 'modifiers': 'gi'
			, 'execute': 'before' // before or after
		}
		, 'mdotags': {
		    'search': '(<)(/?mdo:[\\w-]+)([^>]*>)'
			, 'class': 'mdotags'
			, 'modifiers': 'gi'
			, 'execute': 'before' // before or after
		}
		, 'tags': {
		    'search': '(<)(/?[\\w:-]+)([^>]*>)'
			, 'class': 'tags'
			, 'modifiers': 'gi'
			, 'execute': 'before' // before or after
		}
		, 'attributes': {
		    'search': '(\\s*)([\\w:-]+)(\\s*=\\s*\")(?=[^<>]*?>)'
			, 'class': 'attributes'
			, 'modifiers': 'g'
			, 'execute': 'before' // before or after
		}
		, 'mdoext': {
		    'search': '()(mdo:[a-zA-Z0-9_-]+?)(\\()'
			, 'class': 'mdoext'
			, 'modifiers': 'g'
			, 'execute': 'before' // before or after
		}
		, 'localization': {
		    'search': '()(#\\s*[\\w\\.-]+\\s*/\\s*[\\w\\.-]+\\s*#)()'
			, 'class': 'localization'
			, 'modifiers': 'g'
			, 'execute': 'before' // before or after
		}
	}
	, 'STYLES': {
	    'COMMENTS': 'color: #AAAAAA;'
		, 'QUOTESMARKS': 'color: #63AAF8;'
		, 'KEYWORDS': {
		    'reserved': 'color: #808080;'
			, 'functions': 'color: #FF00FF;'
			, 'statements': 'color: #0000FF;'
		}
		, 'OPERATORS': 'color: #777777;'
		, 'DELIMITERS': ''
		, 'REGEXPS': {
		    'attributes': 'color: #B1AC41;'
		    , 'funcs': 'color: #FF00FF;'
		    , 'mdoext': 'color: #990000;'
		    , 'mdotags': 'color: #990000;'
			, 'tags': 'color: #009999;'
			, 'xsltags': 'color: #0055FF;'
			, 'asptags': 'color: #4BACC0;'
			, 'dnntags': 'color: #FF0000;'
			, 'teleriktags': 'color: #00AA00;'
			, 'xml': 'color: #8DCFB5;'
			, 'localization': 'color: #999999; font-weight:bold;'
			, 'cdatas': 'color: #50B020;'
		}
	}
	, 'AUTO_COMPLETION': {
	    "extensions": {	// the name of this definition group. It's posisble to have different rules inside the same definition file
	        "REGEXP": { "before_word": "[^a-zA-Z0-9_<-]|^" //"[^a-zA-Z0-9_<-]"	// \\s|\\.|
						, "possible_words_letters": "[a-zA-Z0-9_-]+"
						, "letter_after_word_must_match": "[^a-zA-Z0-9_-]|$"
						, "prefix_separator": ":"
	        }
			, "CASE_SENSITIVE": true
			, "MAX_TEXT_LENGTH": 100		// the maximum length of the text being analyzed before the cursor position
			, "KEYWORDS": {
			    '': [	// the prefix of thoses items
			    /**
			    * 0 : the keyword the user is typing
			    * 1 : (optionnal) the string inserted in code ("{@}" being the new position of the cursor, "§" beeing the equivalent to the value the typed string indicated if the previous )
			    * 		If empty the keyword will be displayed
			    * 2 : (optionnal) the text that appear in the suggestion box (if empty, the string to insert will be displayed)
			    */
                          ['msxsl', '§', '']
						, ['mdo', '§', '']
			    	]
		    	, 'mdo': [
			    		  ["ajax", "ajax({@})", "<span help='Assign values to parameters, sends ajax request to the server and replaces module content with renewed one.' url='http://xsltdb.codeplex.com/wikipage?title=mdo:ajax'>ajax([validator], param1, value1, ...)</span>"]
			    		, ["submit", "submit({@})", "<span help='Assign values to parameters and submits page to the server.' url='http://xsltdb.codeplex.com/wikipage?title=mdo:submit'>submit([validator], param1, value1, ...)</span>"]
			    		, ["jajax", "jajax({@})", "<span help='Same as href=\"javascript:{ajax(...)}\".' url='http://xsltdb.codeplex.com/wikipage?title=mdo:ajax'>jajax([validator], param1, value1, ...)</span>"]
			    		, ["jsubmit", "jsubmit({@})", "<span help='Same as href=\"javascript:{submit()}\".' url='http://xsltdb.codeplex.com/wikipage?title=mdo:submit'>jsubmit([validator], param1, value1, ...)"]
			    		, ["param", "param({@})", "<span help='returns current value of parameter assigned by ajax/submit/navigate' url='http://xsltdb.codeplex.com/wikipage?title=mdo:param'>param(param-name, [default-value])"]
			    		, ["request", "request({@})", "<span help='Returns value of query string or form parameter' url='http://xsltdb.com/XsltDb/Help/Common/Request.aspx'>request(param-name, [default-value])"]
			    		, ["form", "form({@})", "<span help='Returns value of form parameter' url='http://xsltdb.com/XsltDb/Help/Common/Request.aspx'>form(param-name, [default-value])"]
			    		, ["query-string", "query-string({@})", "<span help='Returns value of query string parameter' url='http://xsltdb.com/XsltDb/Help/Common/Request.aspx'>query-string(param-name, [default-value])"]
			    		, ["session", "session({@})", "<span help='Returns value stored in ASP.NET session'>session(param-name, [default-value])</span>"]
			    		, ["cookie", "cookie({@})", "<span help='Returns cookie value' url='http://xsltdb.com/XsltDb/Help/Common/Cookie.aspx'>cookie(cookie-name, [default-value])</span>"]
			    		, ["set-cookie", "set-cookie({@}, )", "<span help='Assings cookie value' url='http://xsltdb.com/XsltDb/Help/Common/Cookie.aspx'>set-cookie(cookie-name, value, expiration-date)</span>"]
			    		, ["cancel-cookie", "cancel-cookie({@})", "<span help='Deletes cookie' url='http://xsltdb.com/XsltDb/Help/Common/Cookie.aspx'>cancel-cookie(cookie-name)</span>"]
			    		, ["request-file", "request-file({@})", "<span help='Saves uploaded file to disk and returns path to file.' url='http://xsltdb.codeplex.com/wikipage?title=mdo:requestfile'>request-file(file-input-name)"]
			    		, ["xml", "xml({@}, )", "<span help='execute safe stored procedure' url='http://xsltdb.codeplex.com/wikipage?title=mdo:xml'>xml(procedure, output-names, param1, ...)"]
			    		, ["sql", "sql({@}, )", "<span help='Executes free sql statement' url='http://xsltdb.codeplex.com/wikipage?title=mdo:sql'>sql(sql-statement, output-names, param1, ...)"]
			    		, ["dnn", "dnn({@})", "<span help='Returns current portal, user, page, module properties.' url='http://xsltdb.codeplex.com/wikipage?title=mdo:dnn'>dnn(property-name, [default-value])</span>"]
			    		, ["aspnet", "aspnet({@})", "<span help='Returns ASP.NET object or control property value'>aspnet(property-path, [default-value])</span>"]
			    		, ["assign", "assign({@}, )", "<span help='Assigns a value to and ASP.NET object property.' url='http://xsltdb.com/XsltDb/Help/ASPNET/Assign.aspx'>assign(property-path, value)</span>"]

			    		, ["get-tab-setting", "get-tab-setting({@})", "<span help='Returns current tab setting' url='http://xsltdb.com/XsltDb/Help/DotNetNuke/TabAndModuleSettings.aspx'>get-tab-setting(setting-name, [default-value])"]
			    		, ["set-tab-setting", "set-tab-setting({@}, )", "<span help='Updates current tab setting' url='http://xsltdb.com/XsltDb/Help/DotNetNuke/TabAndModuleSettings.aspx'>set-tab-setting(setting-name, value)</span>"]
			    		, ["get-module-setting", "get-module-setting({@})", "<span help='Returns current module setting' url='http://xsltdb.com/XsltDb/Help/DotNetNuke/TabAndModuleSettings.aspx'>get-module-setting(setting-name, [default-value])</span>"]
			    		, ["set-module-setting", "set-module-setting({@}, )", "<span help='Updates current module setting' url='http://xsltdb.com/XsltDb/Help/DotNetNuke/TabAndModuleSettings.aspx'>set-module-setting(setting-name, value)</span>"]
			    		, ["get-tab-module-setting", "get-tab-module-setting({@})", "<span help='Returns current tab-module setting' url='http://xsltdb.com/XsltDb/Help/DotNetNuke/TabAndModuleSettings.aspx'>get-tab-module-setting(setting-name, [default-value])</span>"]
			    		, ["set-tab-module-setting", "set-tab-module-setting({@}, )", "<span help='Updates current tab-module setting' url='http://xsltdb.com/XsltDb/Help/DotNetNuke/TabAndModuleSettings.aspx'>set-tab-module-setting(setting-name, value)</span>"]

			    		, ["fmt-date", "fmt-date({@})", "<span help='Formats date according to format-string&lt;br/&gt;Example: mdo:fmt-date($var, &apos;yyyy-MM-dd&apos;)'>fmt-date(date, format-string)</span>"]
			    		, ["fmt-number", "fmt-number({@})", "<span help='Formats number according to format-string&lt;br/&gt;Example: fmt-number($var, &apos;C&apos;)'>fmt-number(number, format-string)"]
			    		, ["date", "date({@})", "<span help='Returns current date and time, optionaly you can specify a format.&lt;br/&gt;Example: mdo:date(&apos;yyyy-MM-dd&apos;)'>date([format-string])</span>"]
			    		, ["today", "today({@})", "<span help='Returns true if &apos;date&apos; is current date'>today(date)</span>"]
			    		, ["client-id", "client-id({@})", "<span help='Returns ASP.NET control ID that can be used in javascript code'>client-id(asp-net-control-id)</span>"]
			    		, ["client-name", "client-name({@})", "<span help='Returns name attribute value for the ASP.NET control input that stores the value of control and can be used in javascript code'>client-name(asp-net-control-id)</span>"]

			    		, ["is-ajax", "is-ajax()", "<span help='Returns true if current execution caused by mdo:ajax.'>is-ajax()</span>"]
			    		, ["is-submit", "is-submit()", "<span help='Returns true if current execution caused by page submission.'>is-submit()</span>"]
			    		, ["is-navigate", "is-navigate()", "<span help='Returns true if current execution caused by navigating to a page URL.'>is-navigate()</span>"]
			    		, ["redirect", "redirect({@})", "<span help='redirects browser to url. If url parameter is not supplied - redirects to current tab URL.'>redirect([url])</span>"]

			    		, ["iif", "iif({@})", "<span help='if bool-expr is true returns if-true value else returns if-false.'>iif(bool-expr, if-true, if-false)</span>"]
			    		, ["coalesce", "coalesce({@})", "<span help='Returns firts not empty argument'>coalesce(v1, v2, ...)</span>"]

			    		, ["html-encode", "html-encode({@})", "html-encode(text-to-encode)"]
			    		, ["html-attribute-encode", "html-attribute-encode({@})", "html-attribute-encode(text-to-encode)"]
			    		, ["html-decode", "html-decode({@})", "html-decode(text-to-decode)"]
			    		, ["url-encode", "url-encode({@})", "url-encode(text-to-encode)"]
			    		, ["url-encode-unicode", "url-encode-unicode({@})", "url-encode-unicode(text-to-encode)"]
			    		, ["url-decode", "url-decode({@})", "url-decode(text-to-decode)"]
			    		, ["regex-encode", "regex-encode({@})", "regex-encode(text-to-encode)"]

			    		, ["replace", "replace({@})", "replace(source-text, regex-pattern, replacer)"]
			    		, ["index-of", "index-of({@})", "index-of(source-text, regex-pattern)"]
			    		, ["match", "match({@})", "<span help='Returns collection of matches and matched groups as XML'>match(source-text, regex-pattern)</span>"]
			    		, ["sequense-1", "sequence({@})", "sequense(from, to, step)"]
			    		, ["sequense-2", "sequence({@})", "sequense(count)"]


			    		, ["new-id", "new-id()", "<span help='Generates a new GUID'>new-id()</span>"]
			    		, ["md5", "md5({@})", "<span help='calculates a MD5 hash code for string'>md5(string)"]
			    		, ["var", "var({@})", "<span help='Creates a named value. This value can be reassigned that gives you a workaround over XSLT variables that are not reassignable. If value is not specified - returns current variable value'>var(name, [value])</span>"]
			    		, ["text", "text({@})", "<span help='Returns an outer XML of supplied xml-fragment'>text(xml-fragment)</span>"]
			    		, ["html", "html({@})", "<span help='Returns a preformatted HTML for outer XML of xml-fragmet supplied. Can be used for outputting XML'>html(xml-fragment)</span>"]
			    		, ["node-set", "node-set({@})", "<span help='Creates a navigable node set from XML string'>node-set(text)</span>"]
			    		, ["culture", "culture()", "<span help='Returns currently selected site language' url='http://xsltdb.com/XsltDb/Help/Common/Localization.aspx'>culture()</span>"]

			    		, ["password", "password({@})", "password(user-id, [secret-answer])"]
			    		, ["login", "login({@})", "login(user-name, password, [remember-user])"]
			    		, ["logout", "logout()", "logout()"]
			    		, ["is-in-role", "is-in-role({@})", "is-in-role(security-role)"]
			    		, ["is-super-user", "is-super-user()", "is-super-user()"]

			    		, ["portal-files", "portal-files({@})", "portal-files([relative-path])"]
			    		, ["read-portal-xml", "read-portal-xml({@})", "read-portal-xml(relative-path)"]
			    		, ["read-portal-tabbed-txt", "read-portal-tabbed-txt({@})", "read-portal-tabbed-txt(relative-path, [column-names], [encoding] )"]

			    		, ["dnn-prepare-sql", "dnn-prepare-sql({@})", "<span help='Allow to safely access DotNetNuke tables by replacing {databaseOwner} and {objectQualifier}'>dnn-prepare-sql(sql)</span>"]
			    		, ["event-source", "event-source()", "<span help='Returns ASP.NET server-side ID of object that caused page submission'>event-source()</span>"]
			    		, ["resolve-url", "resolve-url({@})", "<span help='Create absolute url path by relative one'>resolve-url(url)</span>"]

					]
		    	, 'msxsl': [
			    		  ["node-set", "node-set({@})", "node-set(xml-fragment)"]
					]
			}
	    }



	    , "tags": {	// the name of this definition group. It's posisble to have different rules inside the same definition file
	        "REGEXP": { "before_word": "[^<]|^"	// \\s|\\.|
						, "possible_words_letters": "[a-zA-Z0-9_<-]+"
						, "letter_after_word_must_match": "[^a-zA-Z0-9_/-]|\\Z"
						, "prefix_separator": ":"
	        }
			, "CASE_SENSITIVE": true
			, "MAX_TEXT_LENGTH": 100		// the maximum length of the text being analyzed before the cursor position
			, "KEYWORDS": {
			    '': [	// the prefix of thoses items

			    //			     0 : the keyword the user is typing
			    //			     1 : (optionnal) the string inserted in code ("{@}" being the new position of the cursor, "§" beeing the equivalent to the value the typed string indicated if the previous )
			    //			     		If empty the keyword will be displayed
			    //			     2 : (optionnal) the text that appear in the suggestion box (if empty, the string to insert will be displayed)
			    //			    
                          ['<xsl', '<xsl', 'xsl']
						, ['<mdo', '<mdo', 'mdo']
						, ['<asp', '<asp', 'asp']
						, ['<msxsl', '<msxsl', 'msxsl']
						, ['<telerik', '<telerik', 'telerik']

                        , ['<![CDATA', '<![CDATA[{@}]]>', 'CDATA']
                        , ['<input>', '<input name="{@}" />', 'input']
                        , ['<button>', '<button>{@}<button>', 'button']
                        , ['<select>', '<select name="{@}" />', 'select']
                        , ['<textarea>-1', '<textarea name="{@}" />', 'textarea /']
                        , ['<textarea>-2', '<textarea name="{@}" ></textarea>', 'textarea']
                        , ['<div>', '<div>{@}</div>', 'div']
                        , ['<span>', '<span>{@}</span>', 'span']
                        , ['<table>', '<table><tr><td>{@}</td></tr></table>', 'table']
                        , ['<thead>', '<thead>{@}</thead>', 'thead']
                        , ['<tbody>', '<tbody>{@}</tbody>', 'thead']
                        , ['<td>', '<td>{@}</td>', 'td']
                        , ['<tr>', '<tr>{@}</tr>', 'tr']
                        , ['<ul>', '<ul>{@}</ul>', 'ul']
                        , ['<ol>', '<ol>{@}</ol>', 'ol']
                        , ['<li>', '<li>{@}</li>', 'li']
                        , ['<pre>', '<pre>{@}</pre>', 'pre']
                        , ['<img>', '<img src="{@}"/>', 'img']
                        , ['<script>', '<script type="text/javascript">{@}</script>', 'script']
                        , ['<style>', '<style type="text/css">{@}</style>', 'style']
                        , ['<a>', '<a href="{@}"></a>', 'a']
                        , ['<b>', '<b>{@}</b>', 'b']
                        , ['<strong>', '<strong>{@}</strong>', 'strong']
                        , ['<center>', '<center>{@}</center>', 'center']
                        , ['<iframe>', '<iframe>{@}</iframe>', 'iframe']
			    	]
		    	, '<xsl': [
			    		  ['text', 'text>{@}</xsl:text>', 'xsl:text']
			    		, ['element', 'element name="{@}"></xsl:element>', 'xsl:element']
			    		, ['attribute', 'attribute name="{@}"></xsl:attribute>', 'xsl:attribute']
			    		, ['value-of', 'value-of select="{@}"/>', 'xsl:value-of']
			    		, ['if', 'if test="{@}"></xsl:if>', 'xsl:if']
			    		, ['for-each', 'for-each select="{@}"></xsl:for-each>', 'xsl:for-each']
			    		, ['choose', 'choose>{@}</xsl:choose>', 'xsl:choose']
			    		, ['when', 'when test="{@}"></xsl:when>', 'xsl:when']
			    		, ['otherwise', 'otherwise>{@}</xsl:otherwise>', 'xsl:otherwise']
			    		, ['template', 'template {@}></xsl:template>', 'xsl:template']
			    		, ['call-template', 'call-template name="{@}"></xsl:call-template>', 'xsl:call-template']
			    		, ['with-param-1', 'with-param name="{@}" select="" />', 'xsl:with-param select']
			    		, ['with-param-2', 'with-param name="{@}"></xsl:with-param>', 'xsl:with-param']
			    		, ['variable-1', 'variable name="{@}" select=""/>', 'xsl:variable select']
			    		, ['variable-2', 'variable name="{@}"></xsl:variable>', 'xsl:variable']
			    		, ['param', 'param name="{@}" />', 'xsl:param']
			    		, ['copy', 'copy>{@}</xsl:copy>', 'xsl:copy']
			    		, ['copy-of', 'copy-of select="{@}"></xsl:copy-of>', 'xsl:copy-of']
			    		, ['import', 'import href="{@}" />', 'xsl:import']
			    		, ['include', 'include href="{@}"/>', 'xsl:include']
			    		, ['sort', 'sort select="{@}" />', 'xsl:sort']
			    		, ['execute', 'execute select="{@}" />', 'xsl:execute']
					]
		    	, '<mdo': [
			    		  ["callable", "callable js=\"{@}\"></mdo:callable>", "callable"]
			    		, ["service", "service name=\"{@}\" type=\"text/xml\"></mdo:service>", "service"]
			    		, ["pre-render", "pre-render>{@}</mdo:pre-render>", "pre-render"]
			    		, ["asp", "asp xmlns:asp=\"asp\" xmlns:telerik=\"telerik\">{@}</mdo:asp>", "asp"]

					]
		    	, '<msxsl': [
			    		  ["script", "script language=\"C#\" implements-prefix=\"script\">{@}</msxsl:script>", "script"]

					]

			}
	    }

	}
};
