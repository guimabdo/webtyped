//*Generated with WebTyped*
import { WebTypedClient } from '@guimabdo/webtyped-fetch';
export class MegaSampleService extends WebTypedClient {
	constructor(baseUrl: string = WebTypedClient.baseUrl) {
		super(baseUrl, "api/megaSample");
	}
	getThisStringFromQuery = (str: string) : Promise<string> => {
		return this.invokeGet<string>({
				func: this.getThisStringFromQuery,
				parameters: { str }
			},
			``,
			{ str }
		);
	};
	getThisStringFromQueryExplicit = (str: string) : Promise<string> => {
		return this.invokeGet<string>({
				func: this.getThisStringFromQueryExplicit,
				parameters: { str }
			},
			`explicit`,
			{ str }
		);
	};
	getThisStringFromRoute = (str: string) : Promise<string> => {
		return this.invokeGet<string>({
				func: this.getThisStringFromRoute,
				parameters: { str }
			},
			`route/${str}`,
			undefined
		);
	};
	getTheseStrings = (str: string, str2: string) : Promise<Array<string>> => {
		return this.invokeGet<Array<string>>({
				func: this.getTheseStrings,
				parameters: { str, str2 }
			},
			`these/${str}`,
			{ str2 }
		);
	};
	postAndReturnThisStringFromQuery = (str: string) : Promise<string> => {
		return this.invokePost<string>({
				func: this.postAndReturnThisStringFromQuery,
				parameters: { str }
			},
			``,
			null,
			{ str }
		);
	};
	postAndReturnThisStringFromQueryExplicit = (str: string) : Promise<string> => {
		return this.invokePost<string>({
				func: this.postAndReturnThisStringFromQueryExplicit,
				parameters: { str }
			},
			`explicit`,
			null,
			{ str }
		);
	};
	postAndReturnThisStringFromRoute = (str: string) : Promise<string> => {
		return this.invokePost<string>({
				func: this.postAndReturnThisStringFromRoute,
				parameters: { str }
			},
			`${str}`,
			null,
			undefined
		);
	};
	postAndReturnTheseStrings = (str: string, str2: string, str3: string) : Promise<Array<string>> => {
		return this.invokePost<Array<string>>({
				func: this.postAndReturnTheseStrings,
				parameters: { str, str2, str3 }
			},
			`these/${str}`,
			str3,
			{ str2 }
		);
	};
	postAndReturnModel = (model: Fetch.MegaSampleService.Model) : Promise<Fetch.MegaSampleService.Model> => {
		return this.invokePost<Fetch.MegaSampleService.Model>({
				func: this.postAndReturnModel,
				parameters: { model }
			},
			`model`,
			model,
			undefined
		);
	};
	postAndReturnModelSameName = (model: Fetch.ModelA) : Promise<Fetch.ModelA> => {
		return this.invokePost<Fetch.ModelA>({
				func: this.postAndReturnModelSameName,
				parameters: { model }
			},
			`modelA1`,
			model,
			undefined
		);
	};
	postAndReturnModelSameName2 = (model: Fetch.OtherModels.ModelA) : Promise<Fetch.OtherModels.ModelA> => {
		return this.invokePost<Fetch.OtherModels.ModelA>({
				func: this.postAndReturnModelSameName2,
				parameters: { model }
			},
			`modelA2`,
			model,
			undefined
		);
	};
	postAndReturnTuple_NotWorkingYet = (tuple: {str: string, number: number}) : Promise<{str: string, number: number}> => {
		return this.invokePost<{str: string, number: number}>({
				func: this.postAndReturnTuple_NotWorkingYet,
				parameters: { tuple }
			},
			`tuple`,
			tuple,
			undefined
		);
	};
	asyncTest = () : Promise<number> => {
		return this.invokeGet<number>({
				func: this.asyncTest,
				parameters: {  }
			},
			`async`,
			undefined
		);
	};
}
