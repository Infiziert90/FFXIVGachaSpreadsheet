import { writable } from 'svelte/store';

// Available languages
export const AVAILABLE_LANGUAGES = [
    { code: 'En', name: 'English', letter: 'E' },
    { code: 'Fr', name: 'Français', letter: 'F' },
    { code: 'Ja', name: '日本語', letter: 'J' },
    { code: 'De', name: 'Deutsch', letter: 'D' }
];

// Language store with localStorage persistence
function createLanguageStore() {
    // Get initial value from localStorage or default to 'En'
    const storedLanguage = typeof window !== 'undefined' ? localStorage.getItem('language') : null;
    const initialLanguage = storedLanguage || 'En';

    const { subscribe, set, update } = writable(initialLanguage);

    return {
        subscribe,
        set: (language: string) => {
            if (typeof window !== 'undefined') {
                localStorage.setItem('language', language);
            }
            set(language);
        },
        update
    };
}

export const currentLanguage = createLanguageStore();