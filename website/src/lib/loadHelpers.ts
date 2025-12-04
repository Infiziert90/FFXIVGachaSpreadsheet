import {errorHandling, logAndThrow, responseHandler} from "$lib/utils";
import type {ChestDrop, Coffer, Venture} from "$lib/interfaces";
import {error} from "@sveltejs/kit";
import {type ItemInfo, Mappings} from "$lib/mappings";

export async function loadMapping(fetch: any) {
    try {
        if (Object.keys(Mappings).length > 0) return;

        await fetch('data/Mappings.json')
            .then(responseHandler)
            .then((data: Record<number, ItemInfo>) =>{
                for (const [key, value] of Object.entries(data)) {
                    Mappings[parseInt(key)] = value;
                }
            })
            .catch(errorHandling);
    } catch (err) {
        logAndThrow('Error loading mapping data.', err)
    }
}

export async function loadCoffer(path: string, fetch: any): Promise<{content: Coffer[]}> {
    try {
        const res = await fetch(path)
            .then(responseHandler)
            .then((data: Coffer[]) => {
                return data;
            });

        if (!res) {
            throw error(500, {message: `Failed to load ${path} data set.`});
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}

export async function loadDesynth(path: string, fetch: any): Promise<{content: Coffer[]}> {
    try {
        const res = await fetch(path)
            .then(responseHandler)
            .then((data: Coffer[]) => {
                return data;
            });

        if (!res) {
            throw error(500, {message: `Failed to load ${path} data set.`});
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}

export async function loadVentures(path: string, fetch: any): Promise<{content: Venture[]}> {
    try {
        const res = await fetch(path)
            .then(responseHandler)
            .then((data: Venture[]) => {
                return data;
            });

        if (!res) {
            throw error(500, {message: `Failed to load ${path} data set.`});
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}

export async function loadChestDrops(path: string, fetch: any): Promise<{content: ChestDrop[]}> {
    try {
        const res = await fetch(path)
            .then(responseHandler)
            .then((data: ChestDrop[]) => {
                return data;
            });

        if (!res) {
            throw error(500, {message: `Failed to load ${path} data set.`});
        }

        return {content: res};
    } catch (err) {
        logAndThrow(`Failed to load ${path} data set.`, err)
    }
}