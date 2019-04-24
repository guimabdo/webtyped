export interface ODataModelOf1<TKey> {
	ids?: Array<TKey>;
	search?: string;
	skip?: number;
	take?: number;
	orderBy?: string;
	select?: string;
	expand?: string;
	count?: boolean;
	filter?: string;
}

export namespace ODataModelOf1$ {
	export const $ = 'ODataModelOf1';
	export const $ids = 'ids';
	export const $search = 'search';
	export const $skip = 'skip';
	export const $take = 'take';
	export const $orderBy = 'orderBy';
	export const $select = 'select';
	export const $expand = 'expand';
	export const $count = 'count';
	export const $filter = 'filter';
}
