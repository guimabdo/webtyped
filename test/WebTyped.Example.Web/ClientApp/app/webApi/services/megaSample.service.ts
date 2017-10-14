import { Injectable, Inject, forwardRef } from '@angular/core';
import { Http } from '@angular/http';
import { WebApiClient, WebApiEventEmmiterService, WebApiObservable } from '@guimabdo/webtyped-angular';
@Injectable()
export class MegaSampleService extends WebApiClient {
	constructor(@Inject('API_BASE_URL') baseUrl: string, http: Http, @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {
		super(baseUrl, "api/MegaSample", http, eventEmmiter);
	}
	GetThisStringFromQuery = (str: string) : WebApiObservable<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromQuery,
				parameters: { str }
			},
			``,
			{ str }
		);
	};
	GetThisStringFromQueryExplicit = (str: string) : WebApiObservable<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromQueryExplicit,
				parameters: { str }
			},
			`explicit`,
			{ str }
		);
	};
	GetThisStringFromRoute = (str: string) : WebApiObservable<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromRoute,
				parameters: { str }
			},
			`${str}`,
			undefined
		);
	};
	PostAndReturnThisStringFromQuery = (str: string) : WebApiObservable<string> => {
		return this.invokePost<string>({
				func: this.PostAndReturnThisStringFromQuery,
				parameters: { str }
			},
			``,
			{ str }
		);
	};
}

