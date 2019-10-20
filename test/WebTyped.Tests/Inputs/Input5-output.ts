import * as extMdl0 from './cblxSignupResultModel';
import * as extMdl1 from './cblxSignupModel';
import { WebTypedCallInfo, WebTypedFunction, WebTypedInvoker } from '@guimabdo/webtyped-common';
import { Observable } from 'rxjs';
export class CblxIdentityService {
	static readonly controllerName = 'CblxIdentityController';
	private api = 'cblx-identity';
	constructor(private invoker: WebTypedInvoker) {}
	signup: CblxIdentityService.SignupFunction = (model: extMdl1.CblxSignupModel) : Observable<extMdl0.CblxSignupResultModel> => {
		return this.invoker.invoke({
				returnTypeName: 'extMdl0.CblxSignupResultModel',
				kind: 'Signup',
				func: this.signup,
				parameters: { model, _wtKind: 'Signup' }
			},
			this.api,
			`sign-up`,
			`post`,
			model,
			undefined,
			{"name":"Observable","module":"rxjs"}
		);
	};
	resetPassword: CblxIdentityService.ResetPasswordFunction = (email: string) : Observable<void> => {
		return this.invoker.invoke({
				returnTypeName: 'void',
				kind: 'ResetPassword',
				func: this.resetPassword,
				parameters: { email, _wtKind: 'ResetPassword' }
			},
			this.api,
			`password/reset`,
			`post`,
			email,
			undefined,
			{"name":"Observable","module":"rxjs"}
		);
	};
}
export namespace CblxIdentityService {
	export type SignupParameters = {model: extMdl1.CblxSignupModel, _wtKind: 'Signup' };
	export interface SignupCallInfo extends WebTypedCallInfo<SignupParameters, extMdl0.CblxSignupResultModel> { kind: 'Signup'; }
	export type SignupFunctionBase = (model: extMdl1.CblxSignupModel) => Observable<extMdl0.CblxSignupResultModel>;
	export interface SignupFunction extends WebTypedFunction<SignupParameters, extMdl0.CblxSignupResultModel>, SignupFunctionBase {}
	export type ResetPasswordParameters = {email: string, _wtKind: 'ResetPassword' };
	export interface ResetPasswordCallInfo extends WebTypedCallInfo<ResetPasswordParameters, void> { kind: 'ResetPassword'; }
	export type ResetPasswordFunctionBase = (email: string) => Observable<void>;
	export interface ResetPasswordFunction extends WebTypedFunction<ResetPasswordParameters, void>, ResetPasswordFunctionBase {}
}
