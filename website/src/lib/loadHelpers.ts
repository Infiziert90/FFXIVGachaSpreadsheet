import {logAndThrow, responseCompressedHandler, responseHandler} from "$lib/utils";
import type {DesynthBase, DesynthesisBase, SubLoot, Venture} from "$lib/interfaces";
import type {Reduction} from "$lib/structs/reduction";
import type {DesynthBase2} from "$lib/structs/desynthesis";
import type {Coffer} from "$lib/structs/coffer";
import type {ChestDrop} from "$lib/structs/chestDrop";
import type {BnpcPairing} from "$lib/structs/bnpc";

type Fetch = typeof fetch;

const initHeaders = {
    headers: {
        Accept: "application/json, application/gzip",
        "Accept-Encoding": "gzip",
    }
};

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

export async function loadDesynth2(path: string, fetch: Fetch): Promise<{content: DesynthBase2}> {
    try {
        const res = await fetch(path)
            .then(responseHandler)
            .then((data: DesynthBase2) => {
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

export async function loadReduction(path: string, fetch: Fetch): Promise<{content: Reduction}> {
    try {
        const res: Reduction = await fetch(path)
            .then(responseHandler)
            .then((data: Reduction) => {
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
        const res: ChestDrop[] = await fetch(path, initHeaders)
            .then(responseCompressedHandler)
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
        const res: BnpcPairing = await fetch(path, initHeaders)
            .then(responseCompressedHandler)
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