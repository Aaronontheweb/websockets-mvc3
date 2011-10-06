
(function ($) {
    if (!window.onSilverlightError) {
        window.onSilverlightError = function (sender, args) {
            var appSource =
                        (sender != null && sender != 0) ?
                        appSource = sender.getHost().Source : "";

            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;


            var errMsg = [];
            errMsg.push("Unhandled Error in Silverlight Application " + appSource);
            errMsg.push("Code: " + iErrorCode + "    ");
            errMsg.push("Category: " + errorType + "       ");
            errMsg.push("Message: " + args.ErrorMessage + "     ");

            if (errorType == "ParserError") {
                errMsg.push("File: " + args.xamlFile + "     ");
                errMsg.push("Line: " + args.lineNumber + "     ");
                errMsg.push("Position: " + args.charPosition + "     ");
            }
            else if (errorType == "RuntimeError") {
                if (args.lineNumber != 0) {
                    errMsg.push("Line: " + args.lineNumber + "     ");
                    errMsg.push("Position: " + args.charPosition + "     ");
                }
                errMsg.push("MethodName: " + args.methodName + "     ");
            }
            throw new Error(errMsg.join("\n"));
        }
    }

    if (!window.dtsl_FuncUid)
        window.dtsl_FuncUid = 1000;

    $.fn.WebSocketBridge = function (ws, options) {
        var defaults = {
            onError: "onSilverlightError",
            background: "white",
            minRuntimeVersion: "4.0.50826.0",
            autoUpgrade: "true",
            enableHtmlAccess: "true",        
            splashScreenSource: null,
            onSourceDownloadProgressChanged: null,
            onSourceDownloadComplete: null,
            onLoad: null,
            onResize: null,
            windowless: false,
            width: "100%",
            height: "100%"
        };

       // settings.initParams = "host=" + ws;

        var settings = $.extend(defaults, options);
        return this.each(function () {
            var a = [];
            a.push('<object data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="' + settings.width + '" height="' + settings.height + '">');
            a.push('<param name="source" value="/ClientBin/XSockets.SilverlightBridge.xap"/>');
            a.push('<param name="onError" value="onSilverlightError" />');
            a.push('<param name="initParams" value="ws=' + ws +'"/>');
            for (var p in settings) {
                if (p == "width" || p == "height") continue;
                var v = settings[p];
                if (v !== null) {
                    //event callback
                    if (p.indexOf("on") == 0 && $.isFunction(v)) {
                        var funcId = "ftsl_Func" + (window.dtsl_FuncUid++);
                        window[funcId] = v;
                        v = funcId;
                    }
                    a.push('<param name="' + p + '" value="' + v + '" />');
                }
            }
            a.push('<a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=' + settings.minRuntimeVersion + '" style="text-decoration:none">');
            a.push('<img src="http://go.microsoft.com/fwlink/?LinkId=108181" alt="Get Microsoft Silverlight" style="border-style:none"/>');
            a.push('</a>');
            a.push('</object>');
            a.push('<iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe>');
            $(this).prepend(a.join('\n'));
        });
    }
})(jQuery);