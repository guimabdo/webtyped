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

export class ODataModelOf1$ {
	static readonly $ = 'ODataModelOf1';
	static readonly $ids = 'ids';
	static readonly $search = 'search';
	static readonly $skip = 'skip';
	static readonly $take = 'take';
	static readonly $orderBy = 'orderBy';
	static readonly $select = 'select';
	static readonly $expand = 'expand';
	static readonly $count = 'count';
	static readonly $filter = 'filter';
}
