"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var http_1 = require("@angular/common/http");
var WebApiClient = (function () {
    function WebApiClient(baseUrl, api, httpClient, eventEmmiter) {
        this.baseUrl = baseUrl;
        this.api = api;
        this.httpClient = httpClient;
        this.eventEmmiter = eventEmmiter;
    }
    WebApiClient.prototype.invokeGet = function (info, action, search) {
        return this.invoke(info, action, 'get', null, search);
    };
    WebApiClient.prototype.invokePatch = function (info, action, body, search) {
        return this.invoke(info, action, 'patch', body, search);
    };
    WebApiClient.prototype.invokePost = function (info, action, body, search) {
        return this.invoke(info, action, 'post', body, search);
    };
    WebApiClient.prototype.invokePut = function (info, action, body, search) {
        return this.invoke(info, action, 'put', body, search);
    };
    WebApiClient.prototype.invokeDelete = function (info, action, search) {
        return this.invoke(info, action, 'delete', null, search);
    };
    WebApiClient.prototype.invoke = function (info, action, httpMethod, body, search) {
        var _this = this;
        var baseUrl = this.baseUrl || "";
        var httpClient = this.httpClient;
        if (baseUrl.endsWith('/')) {
            baseUrl = baseUrl.substr(0, baseUrl.length - 1);
        }
        var url = baseUrl + "/" + this.api + "/" + action;
        //var fData = "FormData";
        //var isFormData = body && typeof (body) === fData;
        ////Creating headers
        //var headers = new HttpHeaders();
        //if (!isFormData) { // multiplart header is resolved by the browser
        //    headers.set("Content-Type", "application/json");
        //    //If not formadata, stringify
        //    if (body) {
        //        body = JSON.stringify(body);
        //    }
        //}
        //Creating options
        var options = {
            //headers: headers,
            params: undefined
            //withCredentials: true //Cross-domain support? -- with new HttClient, I think this kind of things should be managed with interceptors
        };
        if (search) {
            var params = new http_1.HttpParams();
            for (var p in search) {
                var val = search[p];
                if (val === undefined) {
                    continue;
                }
                params = params.set(p, search[p]);
            }
            options.params = params;
        }
        //var currentSearch = Object.assign({}, search);
        ////Se tiver querystring
        //if (currentSearch) {
        //    var s: URLSearchParams = new URLSearchParams();
        //    for (var p in currentSearch) {
        //        s.set(p, currentSearch[p]);
        //    }
        //    options.search = s;
        //}
        var httpObservable;
        switch (httpMethod) {
            case 'get':
                httpObservable = httpClient.get(url, options);
                break;
            case 'put':
                httpObservable = httpClient.put(url, body, options);
                break;
            case 'patch':
                httpObservable = httpClient.patch(url, body, options);
                break;
            case 'delete':
                httpObservable = httpClient.delete(url, options);
                break;
            case 'post':
            default:
                httpObservable = httpClient.post(url, body, options);
                break;
        }
        var coreObs = httpObservable //Emmit completed event
            .do(function () {
            _this.eventEmmiter.emit(info);
        }, function (r) {
        });
        return coreObs;
    };
    return WebApiClient;
}());
exports.WebApiClient = WebApiClient;
//# sourceMappingURL=webApiClient.js.map