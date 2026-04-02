import {writable} from "svelte/store";

// Rate limit store with localStorage persistence
function createRateLimitStore() {
    // Get initial value from localStorage or default to '0'
    const storedRateLimit = typeof window !== 'undefined' ? localStorage.getItem('rateLimit') : null;
    const initialRateLimit = parseInt(storedRateLimit ?? '0');

    const { subscribe, set, update } = writable(initialRateLimit);

    return {
        subscribe,
        set: (rateLimit: number) => {
            if (typeof window !== 'undefined') {
                localStorage.setItem('rateLimit', rateLimit.toString());
            }
            set(rateLimit);
        },
        update
    };
}

export const currentRateLimit = createRateLimitStore();