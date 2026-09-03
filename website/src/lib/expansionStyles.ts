export interface ExpansionStyle {
    icon: string;
    hue: number;
}

/**
 * Converts an icon ID to the XIVAPI asset path format ("XXXXXX/XXXXXX")
 */
export function iconIdToPath(iconId: number): string {
    const paddedId = iconId.toString().padStart(6, '0');
    const folder = paddedId.substring(0, 3) + '000';
    return `${folder}/${paddedId}`;
}

const expansionStyles: Record<string, ExpansionStyle> = {
    // ARR
    'A Realm Reborn': { icon: iconIdToPath(61875), hue: 236 },
    'ARR': { icon: iconIdToPath(61875), hue: 236 },
    '2.x': { icon: iconIdToPath(61875), hue: 236 },

    // HW
    'Heavensward': { icon: iconIdToPath(61876), hue: 225 },
    'HW': { icon: iconIdToPath(61876), hue: 225 },
    '3.x': { icon: iconIdToPath(61876), hue: 225 },

    // SB
    'Stormblood': { icon: iconIdToPath(61877), hue: 348 },
    'SB': { icon: iconIdToPath(61877), hue: 348 },
    '4.x': { icon: iconIdToPath(61877), hue: 348 },

    // ShB
    'Shadowbringers': { icon: iconIdToPath(61878), hue: 260 },
    'ShB': { icon: iconIdToPath(61878), hue: 260 },
    '5.x': { icon: iconIdToPath(61878), hue: 260 },

    // EW
    'Endwalker': { icon: iconIdToPath(61879), hue: 51 },
    'EW': { icon: iconIdToPath(61879), hue: 51 },
    'Endw': { icon: iconIdToPath(61879), hue: 51 },
    '6.x': { icon: iconIdToPath(61879), hue: 51 },

    // DT
    'Dawntrail': { icon: iconIdToPath(61880), hue: 39 },
    'DT': { icon: iconIdToPath(61880), hue: 39 },
    '7.x': { icon: iconIdToPath(61880), hue: 39 },
};

export function getExpansionStyle(expansionName: string): ExpansionStyle | null {
    return expansionStyles[expansionName] || null;
}
