"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var webtyped_common_1 = require("@guimabdo/webtyped-common");
var param = require('jquery-param');
var WebTypedClient = (function () {
    function WebTypedClient(baseUrl, api) {
        if (baseUrl === void 0) { baseUrl = WebTypedClient.baseUrl; }
        if (api === void 0) { api = WebTypedClient.api; }
        this.baseUrl = baseUrl;
        this.api = api;
        this.baseUrl = this.baseUrl || "/";
        this.api = this.api || "";
    }
    WebTypedClient.prototype.invokeGet = function (info, action, search) {
        return this.invoke(info, action, 'get', null, search);
    };
    WebTypedClient.prototype.invokePatch = function (info, action, body, search) {
        return this.invoke(info, action, 'patch', body, search);
    };
    WebTypedClient.prototype.invokePost = function (info, action, body, search) {
        return this.invoke(info, action, 'post', body, search);
    };
    WebTypedClient.prototype.invokePut = function (info, action, body, search) {
        return this.invoke(info, action, 'put', body, search);
    };
    WebTypedClient.prototype.invokeDelete = function (info, action, search) {
        return this.invoke(info, action, 'delete', null, search);
    };
    WebTypedClient.prototype.invoke = function (info, action, httpMethod, body, search) {
        if (typeof (fetch) === 'undefined') {
            return Promise.resolve(null);
        }
        var baseUrl = this.baseUrl;
        if (baseUrl.endsWith('/')) {
            baseUrl = baseUrl.substr(0, baseUrl.length - 1);
        }
        var url = baseUrl + "/" + this.api + "/" + action;
        if (search) {
            if (url.indexOf('?') < 0) {
                url += '?';
            }
            else {
                url += '&';
            }
            url += param(search);
        }
        var req = fetch(url, {
            body: body ? JSON.stringify(body) : undefined,
            method: httpMethod,
            headers: new Headers({
                'Content-Type': 'application/json'
            })
        });
        var promise = new Promise(function (resolve, reject) {
            req.then(function (r) {
                resolve(r.json());
                var anyWebTyped = webtyped_common_1.WebTypedEventEmitter;
                anyWebTyped.single.emit(info);
            }, function (reason) { return reject(reason); });
        });
        return promise;
    };
    //Global setting
    WebTypedClient.baseUrl = null;
    WebTypedClient.api = null;
    return WebTypedClient;
}());
exports.WebTypedClient = WebTypedClient;
//# sourceMappingURL=webTypedClient.js.map