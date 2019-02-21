export interface ModelMetadata {
	attributes: Array<any/*object*/>;
	isEnum: boolean;
	isFlagsEnum: boolean;
	isReadOnly: boolean;
	isRequired: boolean;
	order: number;
	placeholder: string;
	nullDisplayText: string;
	showForDisplay: boolean;
	showForEdit: boolean;
	simpleDisplayProperty: string;
	templateHint: string;
	isNullableValueType: boolean;
	isCollectionType: boolean;
	isEnumerableType: boolean;
	isReferenceOrNullableType: boolean;
	enumNamesAndValues: any/*System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>*//*<string, string>*/;
	name: string;
	propertyName: string;
	properties: Array<ModelMetadata>;
	enumGroupedDisplayNamesAndValues: Array<{ key: any/*EnumGroupAndName*/, value: string }>;
	displayName: string;
	description: string;
	dataTypeName: string;
	convertEmptyStringToNull: boolean;
	displayFormatString: string;
}

export namespace ModelMetadata {
	export const $ = 'ModelMetadata';
	export const $attributes = 'attributes';
	export const $isEnum = 'isEnum';
	export const $isFlagsEnum = 'isFlagsEnum';
	export const $isReadOnly = 'isReadOnly';
	export const $isRequired = 'isRequired';
	export const $order = 'order';
	export const $placeholder = 'placeholder';
	export const $nullDisplayText = 'nullDisplayText';
	export const $showForDisplay = 'showForDisplay';
	export const $showForEdit = 'showForEdit';
	export const $simpleDisplayProperty = 'simpleDisplayProperty';
	export const $templateHint = 'templateHint';
	export const $isNullableValueType = 'isNullableValueType';
	export const $isCollectionType = 'isCollectionType';
	export const $isEnumerableType = 'isEnumerableType';
	export const $isReferenceOrNullableType = 'isReferenceOrNullableType';
	export const $enumNamesAndValues = 'enumNamesAndValues';
	export const $name = 'name';
	export const $propertyName = 'propertyName';
	export const $properties = 'properties';
	export const $enumGroupedDisplayNamesAndValues = 'enumGroupedDisplayNamesAndValues';
	export const $displayName = 'displayName';
	export const $description = 'description';
	export const $dataTypeName = 'dataTypeName';
	export const $convertEmptyStringToNull = 'convertEmptyStringToNull';
	export const $displayFormatString = 'displayFormatString';
}
