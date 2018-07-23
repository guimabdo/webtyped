export interface WebTypedFunction<TParameters, TResult> {
	//Did not get it => https://github.com/Microsoft/TypeScript/wiki/FAQ#why-doesnt-type-inference-work-on-this-interface-interface-foot---
	//Adding members for making inference work on inherited types
	_p?: TParameters;
	_r?: TResult;
}