export interface ModelMetadata {
	/**
	*Summary should be copied to output
	*Multi line
	**/
	attributes?: Array<any/*object*/>;
	isEnum: boolean;
	isFlagsEnum: boolean;
	isReadOnly: boolean;
	isRequired: boolean;
	order: number;
	placeholder?: string;
	nullDisplayText?: string;
	showForDisplay: boolean;
	showForEdit: boolean;
	simpleDisplayProperty?: string;
	templateHint?: string;
	isNullableValueType: boolean;
	isCollectionType: boolean;
	isEnumerableType: boolean;
	isReferenceOrNullableType: boolean;
	enumNamesAndValues?: any/*System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>*//*<string, string>*/;
	name?: string;
	propertyName?: string;
	properties?: Array<ModelMetadata>;
	enumGroupedDisplayNamesAndValues?: Array<{ key: any/*EnumGroupAndName*/, value: string }>;
	displayName?: string;
	description?: string;
	dataTypeName?: string;
	convertEmptyStringToNull: boolean;
	displayFormatString?: string;
}

export class ModelMetadata$ {
	static readonly $ = 'ModelMetadata';
	static readonly $attributes = 'attributes';
	static readonly $isEnum = 'isEnum';
	static readonly $isFlagsEnum = 'isFlagsEnum';
	static readonly $isReadOnly = 'isReadOnly';
	static readonly $isRequired = 'isRequired';
	static readonly $order = 'order';
	static readonly $placeholder = 'placeholder';
	static readonly $nullDisplayText = 'nullDisplayText';
	static readonly $showForDisplay = 'showForDisplay';
	static readonly $showForEdit = 'showForEdit';
	static readonly $simpleDisplayProperty = 'simpleDisplayProperty';
	static readonly $templateHint = 'templateHint';
	static readonly $isNullableValueType = 'isNullableValueType';
	static readonly $isCollectionType = 'isCollectionType';
	static readonly $isEnumerableType = 'isEnumerableType';
	static readonly $isReferenceOrNullableType = 'isReferenceOrNullableType';
	static readonly $enumNamesAndValues = 'enumNamesAndValues';
	static readonly $name = 'name';
	static readonly $propertyName = 'propertyName';
	static readonly $properties = 'properties';
	static readonly $enumGroupedDisplayNamesAndValues = 'enumGroupedDisplayNamesAndValues';
	static readonly $displayName = 'displayName';
	static readonly $description = 'description';
	static readonly $dataTypeName = 'dataTypeName';
	static readonly $convertEmptyStringToNull = 'convertEmptyStringToNull';
	static readonly $displayFormatString = 'displayFormatString';
}
