import type {CofferContent, CofferVariant, Reward} from "$lib/interfaces";
import type {Option} from "svelte-multiselect";

export function combineCoffer(data: { [key: string]: CofferContent }, selectedPatches: Option[]): CofferContent {
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