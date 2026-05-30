import { writable } from 'svelte/store';

type LayoutWidth = 'fixed' | 'fluid';

function createLayoutWidthStore() {
    const key = 'layoutWidth';
    const initial: LayoutWidth = (typeof window !== 'undefined' ? (localStorage.getItem(key) as LayoutWidth | null) : null) ?? 'fixed';

    const { subscribe, set, update } = writable<LayoutWidth>(initial);

    return {
        subscribe,
        set: (value: LayoutWidth) => {
            if (typeof window !== 'undefined') {
                try {
                    localStorage.setItem(key, value);
                } catch (e) {
                    console.error('Failed to save layoutWidth to localStorage', e);
                }
            }
            set(value);
        },
        toggle: () => {
            if (typeof window === 'undefined') {
                return;
            }
            const current = (localStorage.getItem(key) as LayoutWidth) ?? 'fixed';
            const next: LayoutWidth = current === 'fluid' ? 'fixed' : 'fluid';
            try {
                localStorage.setItem(key, next);
            } catch (e) {
                console.error('Failed to save layoutWidth to localStorage', e);
            }
            set(next);
        },
        update
    };
}

export const layoutWidth = createLayoutWidthStore();
