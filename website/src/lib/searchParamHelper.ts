interface CofferSearchParams {
    territoryId: number,
    cofferId: number
}

/**
 * Reads territory and coffer from URL parameters
 * @param searchParams - The URL search parameters to read from
 * @returns Coffer search parameters if successful, undefined otherwise.
 */
export function tryGetCofferSearchParams(searchParams: URLSearchParams): CofferSearchParams | undefined {
    const territoryId = tryGetParam('territory', searchParams);
    const cofferId = tryGetParam('coffer', searchParams);

    if (territoryId === undefined || cofferId === undefined) return undefined;
    return { territoryId, cofferId };
}

interface VentureSearchParams {
    categoryId: number,
    taskTypeId: number
}

/**
 * Tries to read category and task type from URL parameters
 * @param searchParams - The URL search parameters to read from
 * @returns Venture search parameters if successful, undefined otherwise.
 */
export function tryGetVentureSearchParams(searchParams: URLSearchParams): VentureSearchParams | undefined {
    const categoryId = tryGetParam('category', searchParams);
    const taskTypeId = tryGetParam('task_type', searchParams);

    if (categoryId === undefined || taskTypeId === undefined) return undefined;
    return { categoryId, taskTypeId };
}

interface SubmarineSearchParams {
    mapId: number,
}

/**
 * Tries to read map from the URL parameters.
 * @param searchParams - The URL search parameters to read from
 * @returns Submarine search parameters if successful, undefined otherwise.
 */
export function tryGetSubmarineSearchParams(searchParams: URLSearchParams): SubmarineSearchParams | undefined {
    const mapId = tryGetParam('map', searchParams);

    if (mapId === undefined) return undefined;
    return { mapId };
}

interface DesynthSearchParams {
    sourceId: number,
    rewardId: number,
}

/**
 * Tries to read source or reward from URL parameters
 * @param searchParams - The URL search parameters to read from
 * @returns Desynth search parameters if successful, undefined otherwise.
 */
export function tryGetDesynthSearchParams(searchParams: URLSearchParams): DesynthSearchParams | undefined {
    const sourceId = tryGetParam('source', searchParams);
    const rewardId = tryGetParam('reward', searchParams);

    if (sourceId === undefined && rewardId === undefined) return undefined;

    if (sourceId !== undefined) return {sourceId, rewardId: 0}
    if (rewardId !== undefined) return {sourceId: 0, rewardId}
}

interface DutyLootSearchParams {
    categoryId: number,
    expansionId: number,
    headerId: number,
    dutyId: number
}

/**
 * Tries to read category, expansion, header and duty from URL parameters
 * @param searchParams - The URL search parameters to read from
 * @returns DutyLoot search parameters if successful, undefined otherwise.
 */
export function tryGetDutyLootSearchParams(searchParams: URLSearchParams): DutyLootSearchParams | undefined {
    const categoryId = tryGetParam('category', searchParams);
    const expansionId = tryGetParam('expansion', searchParams);
    const headerId = tryGetParam('header', searchParams);
    const dutyId = tryGetParam('duty', searchParams);

    if (categoryId === undefined || expansionId === undefined || headerId === undefined || dutyId === undefined) return undefined;
    return { categoryId, expansionId, headerId, dutyId };
}

/**
 * Tries to get the specified parameter from the URL search parameters.
 * @param param - The parameter to read from the URL
 * @param searchParams - The URL search parameters to read from
 * @returns The parsed number if successful, undefined otherwise.
 */
function tryGetParam(param: string, searchParams: URLSearchParams): number | undefined {
    const value = searchParams.get(param);
    if(value === null) return undefined;

    const parsed = parseInt(value);
    if(isNaN(parsed)) return undefined

    return parsed
}