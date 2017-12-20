"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var rxjs_1 = require("rxjs");
var WebApiEventEmitterService = (function () {
    function WebApiEventEmitterService() {
        var _this = this;
        this._eventBus = new rxjs_1.Subject();
        this.on = function (f) {
            return _this._eventBus
                .filter(function (e) {
                return e.func == f;
            });
        };
        this.emit = function (info) {
            _this._eventBus.next(info);
        };
    }
    WebApiEventEmitterService = __decorate([
        core_1.Injectable()
    ], WebApiEventEmitterService);
    return WebApiEventEmitterService;
}());
exports.WebApiEventEmitterService = WebApiEventEmitterService;
//# sourceMappingURL=webApiEventEmitter.service.js.map