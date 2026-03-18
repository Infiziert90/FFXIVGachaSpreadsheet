import {writable} from "svelte/store";

// World store with localStorage persistence
function createWorldStore() {
    // Get initial value from localStorage or default to '33'
    const storedWorld = typeof window !== 'undefined' ? localStorage.getItem('world') : null;
    const initialWorld = parseInt(storedWorld ?? '33');

    const { subscribe, set, update } = writable(initialWorld);

    return {
        subscribe,
        set: (world: number) => {
            if (typeof window !== 'undefined') {
                localStorage.setItem('world', world.toString());
            }
            set(world);
        },
        update
    };
}

export const currentWorld = createWorldStore();