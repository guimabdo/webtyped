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
var $ = require("jquery");
var FakeXhr = (function (_super) {
    __extends(FakeXhr, _super);
    function FakeXhr() {
        var _this = _super.call(this, function (res, rej) { return res(null); }) || this;
        _this.state = function () { return "pending"; };
        _this.statusCode = function () { return 0; };
        _this.always = function () { return _this; };
        _this.fail = function () { return _this; };
        _this.done = function () { return _this; };
        _this.progress = function () { return _this; };
        _this.promise = function () { return _this; };
        return _this;
    }
    return FakeXhr;
}(Promise));
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
        if (typeof ($.ajax) === 'undefined') {
            var anyFake = new FakeXhr();
            return anyFake;
        }
        ;
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
            url += $.param(search);
        }
        return $.ajax({
            url: url,
            dataType: 'json',
            contentType: 'application/json',
            data: body ? JSON.stringify(body) : undefined,
            method: httpMethod,
        });
    };
    //Global setting
    WebApiClient.baseUrl = null;
    WebApiClient.api = null;
    return WebApiClient;
}());
exports.WebApiClient = WebApiClient;
//# sourceMappingURL=webApiClient.js.map