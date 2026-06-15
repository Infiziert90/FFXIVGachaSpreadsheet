import { writable } from 'svelte/store';

export type ColorScheme = 'tinted' | 'neutral';

function createColorSchemeStore() {
    const key = 'colorScheme';
    const stored = typeof window !== 'undefined' ? localStorage.getItem(key) : null;
    const initial: ColorScheme = (stored === 'tinted' || stored === 'neutral') ? stored : 'tinted';

    const { subscribe, set } = writable<ColorScheme>(initial);

    return {
        subscribe,
        set: (value: ColorScheme) => {
            if (typeof window !== 'undefined') {
                try {
                    localStorage.setItem(key, value);
                    document.documentElement.setAttribute('data-color-scheme', value);
                } catch (e) {
                    console.error('Failed to save colorScheme to localStorage', e);
                }
            }
            set(value);
        }
    };
}

export const colorScheme = createColorSchemeStore();
