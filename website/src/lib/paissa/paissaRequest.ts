import {logAndThrow, responseHandler} from "$lib/utils";
import type {WorldDetail} from "$lib/paissa/paissaStruct";

const PaissaURL: string = 'https://paissadb.zhu.codes/worlds/';
const HEADERS: RequestInit = {
    method: 'GET',
    headers: new Headers({
        "Accept"       : "application/json",
        "Content-Type" : "application/json",
        "User-Agent"   : "FFXIV Gacha (gacha.infi.ovh)"
    }),
    mode: 'cors'
};

export async function RequestWorld(world: number): Promise<WorldDetail> {
    return await fetch(`${PaissaURL}${world}`, HEADERS)
        .then(responseHandler)
        .then((data: WorldDetail) => {
            return data;
        })
        .catch((err) => {
            logAndThrow('Error loading paissa world data.', err);
        });
}