import { Observable } from 'rxjs';
import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
export declare class WebTypedEventEmitterService {
    private _eventBus;
    constructor();
    on: (f: Function) => Observable<WebTypedCallInfo>;
    emit: (info: WebTypedCallInfo) => void;
}
