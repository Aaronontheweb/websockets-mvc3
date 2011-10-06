(function () {
    var jXSockets = {
        WEBSOCKET: "WEBSOCKET",
        SILVERLIGHT: "SILVERLIGHT",
        FLASH: "FLASH",
        //WebSocket Constructor : url to handler and the bridge to use (optional)
        xWebSocket: function (url, preferedVerison) {

            var callbacks = {};
            var conn;
            var socketVersion = "unknown";
            var supportsPreferedVersion = false;

            //If no version selected. Get the browsers supported version
            if (preferedVerison != jXSockets.WEBSOCKET && preferedVerison != jXSockets.SILVERLIGHT && preferedVerison != jXSockets.FLASH) preferedVerison = getSupportedVersion();

            //Verify that selected version is supported (if selected is not supported, the preferedversion will be detected)
            if (preferedVerison == jXSockets.WEBSOCKET && webSocketSupport()) supportsPreferedVersion = true;
            else if (preferedVerison == jXSockets.FLASH && flashSupport()) supportsPreferedVersion = true;
            else if (preferedVerison == jXSockets.SILVERLIGHT && silverlightSupport()) supportsPreferedVersion = true;
            else {
                preferedVerison = getSupportedVersion();
                supportsPreferedVersion = true;
            }

            //Create instance of the prefered version    
            if (preferedVerison == jXSockets.WEBSOCKET && supportsPreferedVersion) {
                conn = new WebSocket(url);
                conn.onclose = function () {
                    dispatch('close', null);
                }; ;
                conn.onopen = function () {
                    dispatch('open', null);
                };
                conn.onmessage = function (a) {
                    messagehandler(a);
                };
                socketVersion = jXSockets.WEBSOCKET;
            } else if (preferedVerison == jXSockets.FLASH && supportsPreferedVersion) {
                //Setup flash container
                $('body').append('<div id="wsPlaceholder"></div>');
                $(window).bind("beforeunload", function () {
                    document.getElementById("wsPlaceholder").close();
                });
                var settings = {
                    host: url,
                    jsHost: "myWsHost"
                };

                var params = {
                    allowScripting: "always"
                };
                var attributes = {};
                var s = swfobject.embedSWF("/bridge/XSocketsFlashBridge.swf", "wsPlaceholder", "10", "10", "10.0.0", "expressInstall.swf", settings, params, attributes);
                myWsHost._setXSocket(this);
                socketVersion = jXSockets.FLASH;



            } else if (preferedVerison == jXSockets.SILVERLIGHT && supportsPreferedVersion) {
                $("body").WebSocketBridge(url, {
                    height: 0,
                    width: 0,
                    onLoad: function (sender, args) {
                        conn = sender.getHost().Content.Api;
                        conn.onclose = function () {
                            dispatch('close', null);
                        };
                        conn.onopen = function () {
                            dispatch('open', null);
                        };
                        conn.onmessage = function (a, b) {
                            messagehandler(b);
                        };

                    }
                });
                socketVersion = jXSockets.SILVERLIGHT;
            }
            //NO SUPPORT!!!
            else {
                alert('Your device does not support websocket api (aka HTML5 webSockets), flash or silverlight.');
                return null;
            }
            //The version running...
            this.version = function () {
                return socketVersion;
            };
            //Close socket...
            this.dispose = function () {
                if (socketVersion == jXSockets.FLASH) document.getElementById("wsPlaceholder").close();
                else conn.close();
            };

            function addMethod(object, name, fn) {
                var old = object[name];
                if (old) object[name] = function () {
                    if (fn.length == arguments.length) return fn.apply(this, arguments);
                    else if (typeof old == 'function') return old.apply(this, arguments);                    
                };
                else object[name] = fn;
            }

            addMethod(this, "bind", function (event_name, callback) {
                callbacks[event_name] = callbacks[event_name] || [];
                callbacks[event_name].push({
                    callback: callback,
                    options: null
                });
                return this;
            });
            addMethod(this, "bind", function (event_name, options, callback) {
                callbacks[event_name] = callbacks[event_name] || [];
                callbacks[event_name].push({
                    callback: callback,
                    options: options
                });
                return this;
            });

            addMethod(this, "many", function (event_name, count, callback) {
                callbacks[event_name] = callbacks[event_name] || [];
                callbacks[event_name].push({
                    callback: callback,
                    options: {
                        Counter: {
                            Messages: count,
                            Completed: function () {
                                for (var i = 0; i < callbacks[event_name].length; i++) {
                                    callbacks[event_name].splice(i, 1);
                                    break;
                                }
                            }
                        }
                    }
                });
                return this;
            });

            addMethod(this, "one", function (event_name, callback) {
                callbacks[event_name] = callbacks[event_name] || [];
                callbacks[event_name].push({
                    callback: callback,
                    options: {
                        Counter: {
                            Messages: 1,
                            Completed: function () {
                                for (var i = 0; i < callbacks[event_name].length; i++) {
                                    callbacks[event_name].splice(i, 1);
//                                    break;
                                }
                            }
                        }
                    }
                });
                return this;
            });
            addMethod(this, "unbind", function (eventName) {
                for (var i = 0; i < callbacks[eventName].length; i++) {
                    callbacks[eventName].splice(i, 1);
//                    break;
                }
            });

            addMethod(this, "unbind", function (eventName, callback) {
                for (var i = 0; i < callbacks[eventName].length; i++) {
                    if (callbacks[eventName][i] == callback) {
                        callbacks[eventName].splice(i, 1);
                        break;
                    }
                }
            });

            addMethod(this, "trigger", function (data) {

                var payload = JSON.stringify(data, null, 0);
                if (socketVersion == jXSockets.FLASH) document.getElementById("wsPlaceholder").send(payload);
                else conn.send(payload);
                return data;
            });

            addMethod(this, "trigger", function (message, data) {
                var payload = null;
                if (typeof (data) == "function") {
                    payload = JSON.stringify(message, null, 0)
                } else {
                    var event = new jXSockets.WebSocketMessage(message);
                    event.AddJson(data);
                    payload = event.toString();
                }

                if (socketVersion == jXSockets.FLASH) document.getElementById("wsPlaceholder").send(payload);
                else conn.send(payload);

                if (typeof (data) == "function") {
                    if (typeof (payload) == "object") {
                        data(payload);
                    } else {
                        data(JSON.parse(payload));
                    }
                }
                return message;
            });

            addMethod(this, "trigger", function (message, data, oncompleted) {
                var event = new jXSockets.WebSocketMessage(message);
                event.AddJson(data);
                var payload = event.toString();
                if (socketVersion == jXSockets.FLASH) document.getElementById("wsPlaceholder").send(payload);
                else conn.send(payload);
                oncompleted(event.Message);
                return event.Message;
            });

            this.dispatchFlash = function (f) {
                raiseEvent(f);
            };

            this.dispatchFlashEvent = function (f, data) {
                dispatch(f, data);
            };

            var messagehandler = function (message) {
                if (socketVersion == this.SILVERLIGHT) {
                    if (IsJsonString(message.data)) {
                        raiseEvent(message);
                    } else return;
                } else {
                    raiseEvent(message);
                }
            };

            var dispatch = function (event_name, message) {
                var chain = callbacks[event_name];
                if (typeof chain == 'undefined') return;
                for (var i = 0; i < chain.length; i++) {
                    chain[i].callback(message);
                    var opts = chain[i].options;
                    if (opts != null) {
                        if (opts.Counter !== undefined) {
                            chain[i].options.Counter.Messages--;
                            if (chain[i].options.Counter.Messages == 0) {
                                if (typeof opts.Counter.Completed !== 'undefined') {
                                    opts.Counter.Completed();
                                }
                            }

                        }
                    }
                }
            };

            var raiseEvent = function (message) {
                var msg;
                var event_name;
                if (socketVersion == jXSockets.FLASH) {
                    msg = JSON.parse(message);
                    event_name = msg.event;
                    dispatch(event_name, msg.data);
                } else {
                    msg = JSON.parse(message.data);
                    event_name = msg.event;
                    dispatch(event_name, msg.data);
                }
            };

            addMethod(this, "xWebSocketMessage", function () {
                return new jXSockets.WebSocketMessage("HelloWorld");
            });




            this.WebSocketMessage = function (name, options) {
                var message = new $$.WebSocketMessage(name);
                message._trigger = this.trigger;
                message.publish = function () {
                    this._trigger(message.Message);
                };
                this.bind(name, options.subscribe);
                return message;
            };



            return this;
        },

        IsJsonString: function (str) {
            try {

                JSON.parse(str);
            } catch (e) {
                return false;
            }
            return true;
        },




        WebSocketMessage: function (eventName) {
            this.EventName = eventName;
            this.Message = {
                event: eventName,
                data: null
            };

            function addMethod(object, name, fn) {
                var old = object[name];
                if (old) object[name] = function () {
                    if (fn.length == arguments.length) return fn.apply(this, arguments);
                    else if (typeof old == 'function') return old.apply(this, arguments);
                };
                else object[name] = fn;
            }


            this.AddJson = function (obj) {
                this.Message.data = obj;
                return this;
            };
            this.AddJsonString = function (jsonStr) {
                this.Message.Data = JSON.parse(jsonStr);
                return this.Message;
            };
            this.toString = function toString() {
                return JSON.stringify(this.Message);
            };
            this.PayLoad = function() {
                return this.Message;
            };





            return this;
        }
    };

    if (!window.jXSockets) {
        window.jXSockets = jXSockets;
    }
    if (!window.$$) {
        window.$$ = jXSockets;
    }
})();

function getSupportedVersion() {
    if (webSocketSupport()) return jXSockets.WEBSOCKET;

    if (flashSupport()) return jXSockets.FLASH;

    if (silverlightSupport()) return jXSockets.SILVERLIGHT;

    return "NO SUPPORT";
}
//Check for Silverlight
function silverlightSupport() {
    var isSilverlightInstalled = false;
    try {
        //check on IE
        try {
            var slControl = new ActiveXObject('AgControl.AgControl');
            isSilverlightInstalled = true;
        } catch (e) {
            //either not installed or not IE. Check Firefox
            if (navigator.plugins["Silverlight Plug-In"]) {
                isSilverlightInstalled = true;
            }
        }
    } catch (e) {
        //we don't want to leak exceptions. However, you may want
        //to add exception tracking code here.
    }
    return isSilverlightInstalled;
}
function flashSupport() {
    try {
        if (swfobject.hasFlashPlayerVersion("1")) return true;
        return false;
    } catch (e) {
        return false;
    }
}
function webSocketSupport() {
    if ("WebSocket" in window) return true;
    return false;
}
//Handle flash events
var flashDispatcher = function () {
    this.xSocket = null;
    this._setXSocket = function (s) {
        this.xSocket = s;
    };
    this._onopen = function () {
        return this;
    };
    this._onmessage = function (m) {
        return this;
    };
    this._onclose = function () {
        return this;
    };
    this._onerror = function(m) {
        return this;
    };
};
//FlashMessagehandler (dummy)
var myWsHost = new flashDispatcher();
myWsHost._onmessage = function (m) {
    this.xSocket.dispatchFlash(m.message);
};
myWsHost._onopen = function () {
    this.xSocket.dispatchFlashEvent('open', null);
};
myWsHost._onclose = function () {
    this.xSocket.dispatchFlashEvent('close', null);
};
myWsHost._onerror = function (m) {
    this.xSocket.dispatchFlashEvent('error', m);
};