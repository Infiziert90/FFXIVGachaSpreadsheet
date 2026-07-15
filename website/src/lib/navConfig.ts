import { getFormattedIconId, getIconPath } from '$lib/utils';

export interface NavItem {
    label: string;
    href: string;
    icon: number;
}

export interface NavCategory {
    label: string;
    id: string;
    items: NavItem[];
}

export const navCategories: NavCategory[] = [
    {
        label: 'Content',
        id: 'content',
        items: [
            { label: 'Random Coffers',          href: '/coffer/',    icon: 61812 },
            { label: 'Lockboxes',               href: '/lockbox/',   icon: 61808 },
            { label: 'Card Packs',              href: '/card/',      icon: 61820 },
            { label: 'Ventures',                href: '/venture/',   icon: 61818 },
            { label: 'Eureka Bunnies',          href: '/bunny/',     icon: 61833 },
            { label: 'Deep Dungeons',           href: '/deep/',      icon: 61824 },
            { label: 'Desynthesis',             href: '/desynth/',   icon: 120   },
            { label: 'Reduction',               href: '/reduction/', icon: 121   },
            { label: 'Occult',                  href: '/occult/',    icon: 61851 },
            { label: 'Loot',                    href: '/loot/',      icon: 61801 },
            { label: 'Logograms and Fragments', href: '/logofrag/',  icon: 61837 },
        ]
    },
    {
        label: 'Submarines',
        id: 'submarine',
        items: [
            { label: 'Submarine Loot', href: '/submarine/', icon: 63191 }
        ]
    },
    {
        label: 'Maps',
        id: 'maps',
        items: [
            { label: 'Monster Locations', href: '/bnpc/',        icon: 61837 },
            { label: 'Housing Info',      href: '/housing/',     icon: 60756 },
            { label: 'Open Plots',        href: '/housingOpen/', icon: 60758 }
        ]
    }
];

export function getNavIconSrc(icon: number): string {
    return getIconPath(getFormattedIconId(icon), true);
}
