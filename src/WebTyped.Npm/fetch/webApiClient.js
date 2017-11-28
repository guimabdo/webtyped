"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var param = require('jquery-param');
var WebApiClient = (function () {
    function WebApiClient(baseUrl, api) {
        if (baseUrl === void 0) { baseUrl = WebApiClient.baseUrl; }
        if (api === void 0) { api = WebApiClient.api; }
        this.baseUrl = baseUrl;
        this.api = api;
        this.baseUrl = this.baseUrl || "/";
        this.api = this.api || "";
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
            req.then(function (r) { return resolve(r.json()); }, function (reason) { return reject(reason); });
        });
        return promise;
    };
    //Global setting
    WebApiClient.baseUrl = null;
    WebApiClient.api = null;
    return WebApiClient;
}());
exports.WebApiClient = WebApiClient;
//# sourceMappingURL=webApiClient.js.map