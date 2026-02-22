import {logAndThrow, responseHandler} from "$lib/utils";
import type {BnpcPairing, ChestDrop, Coffer, DesynthBase, DesynthesisBase, SubLoot, Venture} from "$lib/interfaces";
import {type ItemInfo, Mappings} from "$lib/mappings";

type Fetch = typeof fetch;

export async function loadMapping(fetch: Fetch) {
    try {
        if (Object.keys(Mappings).length > 0) return;

        const res: Record<number, ItemInfo> = await fetch('/data/Mappings.json')
            .then(responseHandler)
            .then((data: Record<number, ItemInfo>) =>{
                return data;
            });

        if (!res) {
            throw new Error(`Mapping resource is invalid.`);
        }

        for (const [key, value] of Object.entries(res)) {
            Mappings[parseInt(key)] = value;
        }
    } catch (err) {
        logAndThrow('Error loading mapping data.', err)
    }
}

export async function loadCoffer(path: string, fetch: Fetch): Promise<{content: Coffer[]}> {
    try {
        const res: Coffer[] = await fetch(path)
            .then(responseHandler)
            .then((data: Coffer[]) => {
                return data;
            });

        if (!res) {
            throw new Error(`${path} resource is invalid.`);
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}

export async function loadDesynth(path: string, fetch: Fetch): Promise<{content: DesynthBase}> {
    try {
        const res = await fetch(path)
            .then(responseHandler)
            .then((data: DesynthBase) => {
                return data;
            });

        if (!res) {
            throw new Error(`${path} resource is invalid.`);
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}


export async function loadDesynthesisBase(path: string, fetch: Fetch): Promise<{content: DesynthesisBase}> {
    try {
        const res = await fetch(path)
            .then(responseHandler)
            .then((data: DesynthesisBase) => {
                return data;
            });

        if (!res) {
            throw new Error(`${path} resource is invalid.`);
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}

export async function loadVentures(path: string, fetch: Fetch): Promise<{content: Venture[]}> {
    try {
        const res: Venture[] = await fetch(path)
            .then(responseHandler)
            .then((data: Venture[]) => {
                return data;
            });

        if (!res) {
            throw new Error(`${path} resource is invalid.`);
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}

export async function loadChestDrops(path: string, fetch: Fetch): Promise<{content: ChestDrop[]}> {
    try {
        const res: ChestDrop[] = await fetch(path)
            .then(responseHandler)
            .then((data: ChestDrop[]) => {
                return data;
            });

        if (!res) {
            throw new Error(`${path} resource is invalid.`);
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}

export async function loadSubmarines(path: string, fetch: Fetch): Promise<{content: SubLoot}> {
    try {
        const res: SubLoot = await fetch(path)
            .then(responseHandler)
            .then((data: SubLoot) => {
                return data;
            });

        if (!res) {
            throw new Error(`${path} resource is invalid.`);
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}

export async function loadBnpc(path: string, fetch: Fetch): Promise<{content: BnpcPairing}> {
    try {
        const res: BnpcPairing = await fetch(path)
            .then(responseHandler)
            .then((data: BnpcPairing) => {
                return data;
            });

        if (!res) {
            throw new Error(`${path} resource is invalid.`);
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}