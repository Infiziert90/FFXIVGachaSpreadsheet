import { writable } from 'svelte/store';

function createTitle() {
    const {subscribe, set, update} = writable('');

    return {
        subscribe,
        set: (value: string) => {set(`FFXIV Gacha • ${value}`)},
        clear: () => {set('FFXIV Gacha');}
    }
}

function createDescription() {
    const {subscribe, set, update} = writable('');

    return {
        subscribe,
        set: (value: string) => {set(value)},
        clear: () => {set('No description provided.');}
    }
}


export const title = createTitle();
export const description = createDescription();