import { writable } from 'svelte/store';

export interface PageSidebarState {
    showButton: boolean;
    toggle: (() => void) | null;
}

export const pageSidebarStore = writable<PageSidebarState>({
    showButton: false,
    toggle: null
});
