import {Mappings} from "$lib/mappings";
import { getIconPath, getWikiUrl } from "$lib/utils";
import type {Reward} from "$lib/structs/reward";
import type {ItemDetail} from "$lib/interfaces";
import {findMapFromSector, getFullSectorName} from "$lib/submarines/utils";

export interface ColumnTemplate {
    header: string;

    isIcon?: boolean;
    sortable?: boolean;
    defaultSort?: string;
    mappingSort?: boolean;

    templateRenderer?: Function;
    valueRenderer?: Function;
    field?: keyof Reward;

    classExtension?: string[];
}

const IconTemplate: ColumnTemplate = {
    header: '',
    sortable: false,
    isIcon: true,
    templateRenderer: (row: Reward) => {
        return `<img width="40" height="40" loading="lazy" src={getIconPath(Mappings[row.Id].Icon, true)} alt="${Mappings[row.Id].Name} Icon">`
    },
    classExtension: ['icon']
};

const NameTemplate: ColumnTemplate = {
    header: 'Name',
    field: 'Id',
    mappingSort: true,
    templateRenderer: (row: Reward) => {
        const name = Mappings[row.Id].Name;
        return `<a href={getWikiUrl(name)} class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">${name}</a>`
    },
    classExtension: ['name']
};

const ObtainedTemplate: ColumnTemplate = {
    header: 'Seen',
    field: 'Amount',
    classExtension: ['number', 'text-center']
};

const RewardDesynthTemplate: ColumnTemplate = {
    header: 'Seen / Desynths',
    field: 'Amount',
    valueRenderer: (row: Reward) => `${row.Amount} / ${row.Total}`,
    classExtension: ['number', 'text-center']
};

const TotalTemplate: ColumnTemplate = {
    header: 'Dropped',
    field: 'Total',
    classExtension: ['number', 'text-center']
};

const MinMaxTemplate: ColumnTemplate = {
    header: 'Min-Max',
    field: 'Min',
    valueRenderer: (row: Reward) => `${row.Min}–${row.Max}`,
    classExtension: ['number', 'text-center']
};

const ChanceTemplate: ColumnTemplate = {
    header: 'Chance',
    field: 'Pct',
    defaultSort: 'asc',
    valueRenderer: (row: Reward) => `${(row.Pct * 100).toFixed(2)}%`,
    classExtension: ['percentage', 'text-end']
};

const ChanceNoDefaultSortTemplate: ColumnTemplate = {
    header: 'Chance',
    field: 'Pct',
    valueRenderer: (row: Reward) => `${(row.Pct * 100).toFixed(2)}%`,
    classExtension: ['percentage', 'text-end']
};

export const FullColumnSetup: ColumnTemplate[] = [
    IconTemplate,
    NameTemplate,
    ObtainedTemplate,
    TotalTemplate,
    MinMaxTemplate,
    ChanceTemplate,
];

export const NameObtainedChanceSetup: ColumnTemplate[] = [
    IconTemplate,
    NameTemplate,
    ObtainedTemplate,
    ChanceTemplate,
];

export const NameObtainedMinChanceSetup: ColumnTemplate[] = [
    IconTemplate,
    NameTemplate,
    ObtainedTemplate,
    MinMaxTemplate,
    ChanceTemplate,
];

export const RewardDesynthSpecial: ColumnTemplate[] = [
    IconTemplate,
    NameTemplate,
    RewardDesynthTemplate,
    MinMaxTemplate,
    ChanceTemplate,
];

export const ReduceSpecial: ColumnTemplate[] = [
    IconTemplate,
    NameTemplate,
    MinMaxTemplate,
    ChanceNoDefaultSortTemplate,
]

// Submarine specific
export interface SubColumnTemplate {
    header: string;

    isIcon?: boolean;
    sortable?: boolean;
    defaultSort?: string;
    mappingSort?: boolean;

    templateRenderer?: Function;
    valueRenderer?: Function;
    field?: keyof ItemDetail;

    classExtension?: string[];
}

const SubSectorTemplate: SubColumnTemplate = {
    header: 'Sector',
    field: 'Sector',
    templateRenderer: (row: ItemDetail) => {
        return `<a href="/submarine?map=${findMapFromSector(row.Sector)}#${row.Sector}" class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0">${getFullSectorName(row.Sector)}</a>`
    },
    classExtension: ['name'],
    sortable: false
};

const SubTierTemplate: SubColumnTemplate = {
    header: 'Tier',
    field: 'Tier',
    valueRenderer: (row: ItemDetail) => `T${row.Tier}`,
    classExtension: ['small-number', 'text-center'],
    sortable: false
};

const SubPoorTemplate: SubColumnTemplate = {
    header: 'Poor',
    field: 'Poor',
    valueRenderer: (row: ItemDetail) => row.Poor,
    classExtension: ['small-number', 'text-center'],
    sortable: false
};

const SubNormalTemplate: SubColumnTemplate = {
    header: 'Normal',
    field: 'Normal',
    valueRenderer: (row: ItemDetail) => row.Normal,
    classExtension: ['small-number', 'text-center'],
    sortable: false
};

const SubOptimalTemplate: SubColumnTemplate = {
    header: 'Optimal',
    field: 'Optimal',
    valueRenderer: (row: ItemDetail) => row.Optimal,
    classExtension: ['small-number', 'text-center'],
    sortable: false
};

const SubDropRateTemplate: SubColumnTemplate = {
    header: 'Drop Chance',
    field: 'T3Rate',
    valueRenderer: (row: ItemDetail) => `${row.T3Rate}%`,
    classExtension: ['percentage', 'text-end'],
    sortable: false
};

export const SubColumns: SubColumnTemplate[] = [
    SubSectorTemplate,
    SubTierTemplate,
    SubPoorTemplate,
    SubNormalTemplate,
    SubOptimalTemplate,
    SubDropRateTemplate,
];