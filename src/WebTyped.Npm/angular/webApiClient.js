"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var _1 = require("./");
var WebApiClient = (function () {
    function WebApiClient(api, http, eventEmmiter) {
        this.api = api;
        this.http = http;
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
        return new _1.WebApiObservable(info, this.eventEmmiter, this.http, httpMethod, 'api', //baseBath
        this.api, action, body, search);
    };
    return WebApiClient;
}());
exports.WebApiClient = WebApiClient;
