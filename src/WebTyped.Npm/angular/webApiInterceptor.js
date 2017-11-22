"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
var http_1 = require("@angular/common/http");
var core_1 = require("@angular/core");
var Observable_1 = require("rxjs/Observable");
var WebApiInterceptor = (function () {
    function WebApiInterceptor() {
    }
    WebApiInterceptor.prototype.intercept = function (req, next) {
        var body = req.body;
        //Stringify strings
        if ((typeof body) === "string") {
            body = JSON.stringify(body);
        }
        //Force always application/json (otherwise contenttype will be text/plain for strings)
        var headers = req.headers;
        headers = headers.set("Content-Type", "application/json");
        var clonedRequest = req.clone({
            responseType: 'text',
            body: body,
            headers: headers
        });
        //if (clonedRequest.body
        return next.handle(clonedRequest)
            .map(function (event) {
            if (event instanceof http_1.HttpResponse) {
                var body;
                try {
                    //Fix for https://github.com/angular/angular/issues/18396
                    //(Accepting json strings)
                    body = JSON.parse(event.body);
                }
                catch (err) {
                    //For actions that returns a string
                    //asp.net will send the string without quotes
                    //and parse will fail. (Unless you use attr [Produces("application/json")])
                    body = event.body;
                }
                return event.clone({
                    body: body,
                });
            }
            return event;
        })
            .catch(function (error) {
            var parsedError = Object.assign({}, error, { error: JSON.parse(error.error) });
            return Observable_1.Observable.throw(new http_1.HttpErrorResponse(parsedError));
        });
    };
    WebApiInterceptor = __decorate([
        core_1.Injectable()
    ], WebApiInterceptor);
    return WebApiInterceptor;
}());
exports.WebApiInterceptor = WebApiInterceptor;
//# sourceMappingURL=webApiInterceptor.js.map