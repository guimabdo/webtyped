/*import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
@Injectable()
export class WebTypedInterceptor implements HttpInterceptor {
    constructor() { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		var body = req.body;
		var bodyType = (typeof body);
        //Stringify strings
		if (bodyType === "string") {
            body = JSON.stringify(body);
        }
        //Force always application/json (otherwise contenttype will be text/plain for strings)
		var headers = req.headers;
        var currentContentType = headers.get("Content-Type");
        if (!currentContentType && !(typeof FormData != 'undefined' && body instanceof FormData)) {
			headers = headers.set("Content-Type", "application/json");
		}

        var clonedRequest = req.clone({
            responseType: 'text',
            body: body,
            headers: headers
		});
		var handle: Observable<HttpEvent<any>> = next.handle(clonedRequest);
		return handle.pipe(map((event: HttpEvent<any>) => {
			//Manage response so void and "string" responses wont produces parse exception
			if (event instanceof HttpResponse) {
				var body: any;
				try {
					//(Accepting json strings)
					body = JSON.parse(event.body);
				} catch (err) {
					//For actions that returns a string or empty(for void)
					//asp.net will send the string without quotes
					//and parse will fail. (Unless you use attr [Produces("application/json")])
					body = event.body;
				}

				return event.clone({
					body: body,
				});
			}
			return event;
		}));
    }
}*/