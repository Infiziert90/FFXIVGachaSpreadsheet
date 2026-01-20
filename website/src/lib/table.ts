import type {Reward} from "$lib/interfaces";
import {Mappings} from "$lib/mappings";
import { getIconPath, getWikiUrl } from "$lib/utils";

export interface ColumnTemplate {
    header: string;

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
    }
};

const ObtainedTemplate: ColumnTemplate = {
    header: 'Obtained',
    field: 'Amount',
    classExtension: ['number', 'text-center']
};

const RewardDesynthTemplate: ColumnTemplate = {
    header: 'Obtained / Desynths',
    field: 'Amount',
    valueRenderer: (row: Reward) => `${row.Amount} / ${row.Total}`,
    classExtension: ['number', 'text-center']
};

const TotalTemplate: ColumnTemplate = {
    header: 'Total',
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