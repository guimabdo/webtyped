"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var http_1 = require("@angular/http");
var WebApiObservable = (function (_super) {
    __extends(WebApiObservable, _super);
    function WebApiObservable(info, eventEmmiter, http, httpMethod, basePath, api, action, body, search) {
        var _this = _super.call(this, function (sub) {
            var url = basePath + "/" + api + "/" + action;
            var fData = "FormData";
            var isFormData = body && typeof (body) === fData;
            //Creating headers
            var headers = new http_1.Headers();
            if (!isFormData) {
                headers.set("Content-Type", "application/json");
                //If not formadata, stringify
                if (body) {
                    body = JSON.stringify(body);
                }
            }
            //Creating options
            var options = {
                headers: headers,
                withCredentials: true //Cross-domain support?
            };
            var currentSearch = Object.assign({}, search);
            //Se tiver querystring
            if (currentSearch) {
                var s = new http_1.URLSearchParams();
                for (var p in currentSearch) {
                    s.set(p, currentSearch[p]);
                }
                options.search = s;
            }
            var httpObservable;
            switch (httpMethod) {
                case 'get':
                    httpObservable = http.get(url, options);
                    break;
                case 'put':
                    httpObservable = http.put(url, body, options);
                    break;
                case 'patch':
                    httpObservable = http.patch(url, body, options);
                    break;
                case 'delete':
                    httpObservable = http.delete(url, options);
                    break;
                case 'post':
                default:
                    httpObservable = http.post(url, body, options);
                    break;
            }
            var coreObs = httpObservable.map(function (r) {
                //Convert response
                var result = undefined;
                try {
                    result = r.json();
                }
                catch (err) {
                    //plain text
                    var plainText = r.text();
                    result = plainText;
                }
                return result;
            })
                .do(function () {
                eventEmmiter.emit(info);
            }, function (r) {
                //Would be appropiate an option to centralize error handling here?
            });
            coreObs.subscribe(function (r) { return sub.next(r); }, function (err) { return sub.error(err); }, function () { return sub.complete(); });
        }) || this;
        _this.info = info;
        _this.eventEmmiter = eventEmmiter;
        _this.http = http;
        _this.httpMethod = httpMethod;
        _this.basePath = basePath;
        _this.api = api;
        _this.action = action;
        _this.body = body;
        _this.search = search;
        return _this;
    }
    return WebApiObservable;
}(rxjs_1.Observable));
exports.WebApiObservable = WebApiObservable;
