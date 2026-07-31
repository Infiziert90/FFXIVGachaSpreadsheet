import {ItemMappings, type Localized} from "$lib/mappings";
import type {Writable} from "svelte/store";

/**
 * Resolves a language map ({ en: ..., fr: ... }) down to a single string.
 * Falls back to English, then to a caller supplied placeholder, so that data we
 * do not know about yet renders as text instead of throwing.
 *
 * @param {Localized} map - A language keyed map, may be undefined
 * @param {keyof Localized} language - The current language code
 * @param {string} fallback - Text to use when the map is unknown
 * @returns {string} The localized string
 */
export function localized(map: Localized, language: keyof Localized, fallback = "Unknown"): string {
    return map[language] ?? map.En ?? fallback;
}

/**
 * Resolves the localized name of a zone entry (fate, encounter, item...).
 *
 * @param {number} entry - The item id for this entry
 * @param {Writable<string>} language - The current language code
 * @param {string} fallback - Text to use when the entry is unknown
 * @returns {string} The localized name
 */
export function localizedItem(entry: number, language: Writable<string>, fallback = "Unknown"): string {
    if (!ItemMappings.hasOwnProperty(entry)) {
        return fallback;
    }

    // @ts-ignore
    return localized(ItemMappings[entry], language, fallback);
}