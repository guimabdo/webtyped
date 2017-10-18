import { Observable } from 'rxjs';
import { WebApiCallInfo } from '@guimabdo/webtyped-common';
export declare class WebApiEventEmmiterService {
    private _eventBus;
    constructor();
    on: (f: Function) => Observable<WebApiCallInfo>;
    emit: (info: WebApiCallInfo) => void;
}
