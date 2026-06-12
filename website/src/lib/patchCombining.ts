import type {Chest, CofferContent, CofferVariant, Reward, VentureContent} from "$lib/interfaces";
import type {Option} from "svelte-multiselect";

/**
 * Combines all coffer data into a single entry for the selected patch range
 * @param data
 * @param selectedPatches
 */
export function combineCoffer(data: Record<string, CofferContent>, selectedPatches: Option[]): CofferContent {
    const combinedData: CofferContent = {Total: 0, Items: []};

    const processedItems = new Set<number>();
    for (const patch of selectedPatches) {
        let currentPatch = data[patch.toString()];

        combinedData.Total += currentPatch.Total;

        for (const item of currentPatch.Items) {
            if (!processedItems.has(item.Id)) {
                processedItems.add(item.Id);

                combinedData.Items.push(processItem(item, data, selectedPatches));
            }
        }
    }

    return combinedData;
}

/**
 * Gets the total records for all variants for the selected patch range
 * @param variants
 * @param selectedPatches
 */
export function combineVariantTotal(variants: CofferVariant[], selectedPatches: Option[]): number {
    let total = 0;
    for (const patch of selectedPatches) {
        variants.forEach(c => {
            if (patch.toString() in c.Patches) {
                total += c.Patches[patch.toString()].Total;
            }
        });
    }

    return total;
}

/**
 * Combines all duty records into a single entry for the selected patch range
 * @param data
 * @param selectedPatches
 */
export function combineLoot(data: Record<string, Chest[]>, selectedPatches: Option[]): Record<number, Chest> {
    const combinedData: Record<number, Chest> = {};

    const processedItems = new Set<number>();
    for (const patch of selectedPatches) {
        let chests = data[patch.toString()];
        for (const chest of chests) {
            if (!(chest.Id in combinedData))
                combinedData[chest.Id] = {Records: 0, Id: chest.Id, MapId: chest.MapId, Name: chest.Name, PlaceNameSub: chest.PlaceNameSub, Position: chest.Position, TerritoryId: chest.TerritoryId, Rewards: []}

            let selectedChest = combinedData[chest.Id];
            selectedChest.Records += chest.Records;
            for (const item of chest.Rewards) {
                let id = (chest.Id << 20) + item.Id;
                if (!processedItems.has(id)) {
                    processedItems.add(id);

                    selectedChest.Rewards.push(processLoot(chest.Id, item, data, selectedPatches));
                }
            }
        }
    }

    return combinedData;
}

/**
 * Gets the total number of duty records for the selected patch range
 * @param counts
 * @param selectedPatches
 */
export function combineDutyTotal(counts: Record<string, number>, selectedPatches: Option[]): number {
    let total = 0;
    for (const patch of selectedPatches) {
        if (patch.toString() in counts) {
            total += counts[patch.toString()];
        }
    }

    return total;
}

/**
 * Combines all venture data into a single entry for the selected patch range
 * @param data
 * @param selectedPatches
 */
export function combineVenture(data: Record<string, VentureContent>, selectedPatches: Option[]): VentureContent {
    const combinedData: VentureContent = {Total: 0, Primaries: [], Secondaries: []};

    const processedPrimaryItems = new Set<number>();
    const processedSecondaryItems = new Set<number>();
    for (const patch of selectedPatches) {
        let currentPatch = data[patch.toString()];

        combinedData.Total += currentPatch.Total;

        for (const item of currentPatch.Primaries) {
            if (!processedPrimaryItems.has(item.Id)) {
                processedPrimaryItems.add(item.Id);

                combinedData.Primaries.push(processVentureTask(item, data, selectedPatches, true));
            }
        }

        for (const item of currentPatch.Secondaries) {
            if (!processedSecondaryItems.has(item.Id)) {
                processedSecondaryItems.add(item.Id);

                combinedData.Secondaries.push(processVentureTask(item, data, selectedPatches, false));
            }
        }
    }

    return combinedData;
}

/**
 * Gets the total task record for the selected patch range
 * @param counts
 * @param selectedPatches
 */
export function combineTaskTotal(counts: Record<string, VentureContent>, selectedPatches: Option[]): number {
    let total = 0;
    for (const patch of selectedPatches) {
        if (patch.toString() in counts) {
            total += counts[patch.toString()].Total;
        }
    }

    return total;
}

function processItem(item: Reward, data: Record<string, CofferContent>, selectedPatches: Option[]) {
    let sumOfAllRecords = 0;
    let processedReward: Reward = {Id: item.Id, Amount: 0, Total: 0, Max: 0, Min: 0, Pct: 0};
    for (const patch of selectedPatches) {
        let processingPatch = data[patch.toString()];

        let reward = processingPatch.Items.find(e => e.Id === item.Id);
        if (reward === undefined) continue;

        sumOfAllRecords += processingPatch.Total;

        processedReward.Amount += reward.Amount;
        processedReward.Total += reward.Total;
        if (processedReward.Min === 0) {
            processedReward.Min = reward.Min;
        } else {
            processedReward.Min = Math.min(processedReward.Min, reward.Min);
        }
        processedReward.Max = Math.max(processedReward.Max, reward.Max);
    }

    processedReward.Pct = processedReward.Amount / sumOfAllRecords;
    return processedReward;
}

function processLoot(chestId: number, item: Reward, data: Record<string, Chest[]>, selectedPatches: Option[]) {
    let sumOfAllRecords = 0;
    let processedReward: Reward = {Id: item.Id, Amount: 0, Total: 0, Max: 0, Min: 0, Pct: 0};
    for (const patch of selectedPatches) {
        let chest = data[patch.toString()].find(c => c.Id === chestId);
        if (chest === undefined) continue;

        let reward = chest.Rewards.find(e => e.Id === item.Id);
        if (reward === undefined) continue;

        sumOfAllRecords += chest.Records;

        processedReward.Amount += reward.Amount;
        processedReward.Total += reward.Total;
        if (processedReward.Min === 0) {
            processedReward.Min = reward.Min;
        } else {
            processedReward.Min = Math.min(processedReward.Min, reward.Min);
        }
        processedReward.Max = Math.max(processedReward.Max, reward.Max);
    }

    processedReward.Pct = processedReward.Amount / sumOfAllRecords;
    return processedReward;
}

function processVentureTask(item: Reward, data: Record<string, VentureContent>, selectedPatches: Option[], isPrimary: boolean) {
    let sumOfAllRecords = 0;
    let processedReward: Reward = {Id: item.Id, Amount: 0, Total: 0, Max: 0, Min: 0, Pct: 0};
    for (const patch of selectedPatches) {
        let processingPatch = data[patch.toString()];

        let currentData = isPrimary ? processingPatch.Primaries : processingPatch.Secondaries;
        let reward = currentData.find(e => e.Id === item.Id);
        if (reward === undefined) continue;

        sumOfAllRecords += processingPatch.Total;

        processedReward.Amount += reward.Amount;
        processedReward.Total += reward.Total;
        if (processedReward.Min === 0) {
            processedReward.Min = reward.Min;
        } else {
            processedReward.Min = Math.min(processedReward.Min, reward.Min);
        }
        processedReward.Max = Math.max(processedReward.Max, reward.Max);
    }

    processedReward.Pct = processedReward.Amount / sumOfAllRecords;
    return processedReward;
}