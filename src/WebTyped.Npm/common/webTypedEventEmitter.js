"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var WebTypedEventEmitter = (function () {
    function WebTypedEventEmitter() {
        var _this = this;
        this.methods = [];
        this.callbacks = [];
        this.on = function (f, callback) {
            var index = _this.methods.indexOf(f);
            if (index < 0) {
                index = _this.methods.length;
                _this.methods.push(f);
                _this.callbacks.push([]);
            }
            _this.callbacks[index].push(callback);
            return _this;
        };
        this.off = function (f, callback) {
            var index = _this.methods.indexOf(f);
            if (index >= 0) {
                var callbackIndex = _this.callbacks[index].indexOf(callback);
                if (callbackIndex >= 0) {
                    _this.callbacks[index].splice(callbackIndex, 1);
                }
            }
            return _this;
        };
        this.emit = function (info) {
            var index = _this.methods.indexOf(info.func);
            if (index >= 0) {
                _this.callbacks[index].forEach(function (c) { c(info); });
            }
        };
    }
    WebTypedEventEmitter.single = new WebTypedEventEmitter();
    WebTypedEventEmitter.on = function (f, callback) {
        return WebTypedEventEmitter.single.on(f, callback);
    };
    WebTypedEventEmitter.off = function (f, callback) {
        return WebTypedEventEmitter.single.off(f, callback);
    };
    return WebTypedEventEmitter;
}());
exports.WebTypedEventEmitter = WebTypedEventEmitter;
