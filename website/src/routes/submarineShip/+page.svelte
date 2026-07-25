<script lang="ts">
    import PageSidebar from "../../component/PageSidebar.svelte";
    import {onMount, tick} from "svelte";
    import {Mappings} from "$lib/mappings";
    import {page} from "$app/state";
    import {replaceState} from "$app/navigation";
    import {LastRank} from "$lib/sheets/sheetHelper";
    import type {SubRankRow} from "$lib/sheets/structure/submarines/subRank";
    import {TargetValues} from "$lib/submarines/target";
    import {
        type BestRoute,
        CalculateDuration, EmptyBestRoute,
        FindCalculatedRoute,
        SectorsToPath,
        ToExplorationArray
    } from "$lib/submarines/voyage";
    import {SubmarineBuild} from "$lib/submarines/build";
    import {getDuration, getIconPath, getWikiUrl} from "$lib/utils";
    import MultiSelect, {type Option} from "svelte-multiselect";
    import {SimpleSubExplorationSheet, SimpleSubMapSheet} from "$lib/sheets/simplifiedSheets";
    import {type SubMapRow, ToMapName} from "$lib/sheets/structure/submarines/subMap";
    import {ToLetterName} from "$lib/sheets/structure/submarines/subExploration";
    import {Input} from "@sveltestrap/sveltestrap";
    import {type Breakpoint, CalculateBreakpoint, EmptyBreakpoint} from "$lib/submarines/sector";

    // const
    const PartsCount: number = 10;

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: { [key: string]: HTMLButtonElement } = $state({});

    let allBuilds: SubmarineBuild[] = [];
    let rank: SubRankRow;
    let selectedRank = $state(145);
    let target: TargetValues = $state(new TargetValues());
    let lockedTarget: TargetValues = $state(new TargetValues());

    let ignoreBreakpoints: boolean = $state(false);
    let sectors: number[] = [13, 15, 10, 18, 26];
    let map: SubMapRow = $state(SimpleSubMapSheet[0])

    let selectedSectors: number[] = $state([]);
    let availableSectors: number[] = $state([]);
    let bestSectorPath: BestRoute = $state(EmptyBestRoute());
    let sectorBreakpoints: Breakpoint = $state(EmptyBreakpoint());
    let filteredPath: FilteredBuild[] = $state([]);

    let mapOptions: string[] = $state([]);
    let optionsToId: Record<number, number> = $state({});
    let selectedOption: Option = $state('' as Option);
    let selectOptionId = $state(0);

    initialize();

    for (const mapKey of Object.keys(SimpleSubMapSheet)) {
        let id = parseInt(mapKey);
        if (id === 0)
            continue;

        let idx = mapOptions.length;
        mapOptions.push(ToMapName(SimpleSubMapSheet[id]));

        optionsToId[idx] = id;
    }
    selectedOption = mapOptions[0];
    selectOptionId = optionsToId[0];
    changeMapSelection(1);

    // Set default meta data
    let title = $state('Submarine Ship Finder');
    let description = $state('Find the perfect build for the selected sectors.');

    // // Override defaults with URL parameters if they exist
    // let submarineSearchParams = tryGetSubmarineSearchSearchParams(page.url.searchParams);
    // if (submarineSearchParams !== undefined) {
    //     tableItemId = submarineSearchParams.itemId;
    //
    //     // svelte-ignore state_referenced_locally
    //     if (tableItemId in Mappings) {
    //         title = `Submarine Item Search - ${Mappings[tableItemId].Name}`;
    //         description = `All known sectors with drops for ${Mappings[tableItemId].Name}.`
    //     }
    // }

    // When page loads, open the tab for the current map
    onMount(async () => {
        // if (tableItemId > 0) {
        //     await onButtonClick(tableItemId, false);
        // }
    })

    async function onButtonClick(itemId: number, addQuery: boolean = false) {
        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('item', itemId.toString());
            replaceState(page.url, page.state);
        }

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Submarine Item Search - ${Mappings[itemId].Name}`;
    }

    function initialize() {
        allBuilds = [];

        rank = LastRank;
        selectedRank = rank.RowId;

        for (let hull = 0; hull < PartsCount; hull++)
        {
            for (let stern = 0; stern < PartsCount; stern++)
            {
                for (let bow = 0; bow < PartsCount; bow++)
                {
                    for (let bridge = 0; bridge < PartsCount; bridge++)
                    {
                        allBuilds.push(new SubmarineBuild(selectedRank, (hull * 4) + 3, (stern * 4) + 4, (bow * 4) + 1, (bridge * 4) + 2));
                    }
                }
            }
        }

        lockedTarget = TargetValues.FromBuilds(allBuilds);
        target = TargetValues.FromTarget(lockedTarget);
    }

    async function refreshList() {
        let newList: SubmarineBuild[] = [];
        for (let hull = 0; hull < PartsCount; hull++)
        {
            for (let stern = 0; stern < PartsCount; stern++)
            {
                for (let bow = 0; bow < PartsCount; bow++)
                {
                    for (let bridge = 0; bridge < PartsCount; bridge++)
                    {
                        newList.push(new SubmarineBuild(selectedRank, (hull * 4) + 3, (stern * 4) + 4, (bow * 4) + 1, (bridge * 4) + 2));
                    }
                }
            }
        }

        allBuilds = newList;
        lockedTarget = TargetValues.FromBuilds(allBuilds);
    }

    export interface FilteredBuild {
        Build: SubmarineBuild;
        Time: number;
    }

    function filterBuilds(): FilteredBuild[] {
        console.log("Rebuild triggered")
        sectorBreakpoints = CalculateBreakpoint([]);

        let distance: number = 0;
        let hasRoute: boolean = sectors.length > 0;
        if (hasRoute) {
            sectors = bestSectorPath.Path;
            distance = bestSectorPath.Distance;

            sectorBreakpoints = CalculateBreakpoint(bestSectorPath.Path);
        }

        let builds = allBuilds
            .filter(b => selectedRank >= b.HighestRankPart && b.Range >= distance && b.BuildCost <= rank.Capacity)
            .filter(b => hasRoute && !ignoreBreakpoints
                ? target.SectorFilter(b, sectorBreakpoints)
                : target.Filter(b))
            .map(t => {
                return {
                    Build: t,
                    Time: 43200
                }
            });

        if (hasRoute) {
            builds = builds.map(b => {
                return {
                    Build: b.Build,
                    Time: CalculateDuration(ToExplorationArray(sectors), b.Build.Speed)
                }
            });
        }

        return builds;
    }

    function sortBuilds(filteredBuilds: FilteredBuild[]): FilteredBuild[] {
        return filteredBuilds.sort((a, b) => a.Time - b.Time);
    }

    /**
     * public partial class BuilderWindow
     * {
     *     public bool FuturePrediction;
     *
     *     public bool ShipTab()
     *     {
     *         using var tabItem = ImRaii.TabItem($"{Language.BuilderTabShip}##Ship");
     *         if (!tabItem.Success)
     *             return false;
     *
     *         if (!FuturePrediction && SelectedRank > Sheets.LastRank)
     *             SelectedRank = (int)Sheets.LastRank;
     *
     *         if (ImGui.SliderInt("##shipSliderRank", ref SelectedRank, 1, (int)Sheets.LastRank + (!FuturePrediction ? 0 : 50), $"{Language.TermsRank} %d"))
     *         {
     *             Rank = Build.SubRank.From((uint)SelectedRank);
     *             RefreshList();
     *         }
     *
     *         ImGui.SameLine();
     *         ImGui.Checkbox(Language.BuilderShipCheckboxIgnoreBreakpoints, ref IgnoreBreakpoints);
     *
     *         ImGui.SameLine();
     *         ImGui.Checkbox("Predict Future", ref FuturePrediction);
     *
     *         Helper.TextColored(ImGuiColors.DalamudViolet, Language.BuilderShipHeaderRoute);
     *         SelectedRoute();
     *
     *         ImGuiHelpers.ScaledDummy(5.0f);
     *
     *         var hasRoute = CurrentBuild.Sectors.Count > 0;
     *         if (!hasRoute || IgnoreBreakpoints)
     *         {
     *             if (ImGui.CollapsingHeader(Language.TermsStats))
     *             {
     *                 var textWidth = ImGui.CalcTextSize("Surveillance:").X + (15.0f * ImGuiHelpers.GlobalScale);
     *                 var sliderWidth = ImGui.GetWindowWidth() / 3;
     *
     *                 ImGui.TextUnformatted($"{Language.TermsSurveillance}:");
     *                 ImGui.SameLine(textWidth);
     *                 using (ImRaii.ItemWidth(sliderWidth))
     *                 {
     *                     if (ImGui.SliderInt("##shipSliderMinSurveillance", ref Target.MinSurveillance, LockedTarget.MinSurveillance, LockedTarget.MaxSurveillance, "Min %d"))
     *                         Target.MaxSurveillance = Math.Max(Target.MinSurveillance, Target.MaxSurveillance);
     *
     *                     ImGui.SameLine();
     *
     *                     if (ImGui.SliderInt("##shipSliderMaxSurveillance", ref Target.MaxSurveillance, LockedTarget.MinSurveillance, LockedTarget.MaxSurveillance, "Max %d"))
     *                         Target.MinSurveillance = Math.Min(Target.MinSurveillance, Target.MaxSurveillance);
     *                 }
     *
     *                 ImGui.TextUnformatted($"{Language.TermsRetrieval}:");
     *                 ImGui.SameLine(textWidth);
     *                 using (ImRaii.ItemWidth(sliderWidth))
     *                 {
     *                     if (ImGui.SliderInt("##shipSliderMinRetrieval", ref Target.MinRetrieval, LockedTarget.MinRetrieval, LockedTarget.MaxRetrieval, "Min %d"))
     *                         Target.MaxRetrieval = Math.Max(Target.MinRetrieval, Target.MaxRetrieval);
     *
     *                     ImGui.SameLine();
     *
     *                     if (ImGui.SliderInt("##shipSliderMaxRetrieval", ref Target.MaxRetrieval, LockedTarget.MinRetrieval, LockedTarget.MaxRetrieval, "Max %d"))
     *                         Target.MinRetrieval = Math.Min(Target.MinRetrieval, Target.MaxRetrieval);
     *                 }
     *
     *                 ImGui.TextUnformatted($"{Language.TermsFavor}:");
     *                 ImGui.SameLine(textWidth);
     *                 using (ImRaii.ItemWidth(sliderWidth))
     *                 {
     *                     if (ImGui.SliderInt("##shipSliderMinFavor", ref Target.MinFavor, LockedTarget.MinFavor, LockedTarget.MaxFavor, "Min %d"))
     *                         Target.MaxFavor = Math.Max(Target.MinFavor, Target.MaxFavor);
     *
     *                     ImGui.SameLine();
     *
     *                     if (ImGui.SliderInt("##shipSliderMaxFavor", ref Target.MaxFavor, LockedTarget.MinFavor, LockedTarget.MaxFavor, "Max %d"))
     *                         Target.MinFavor = Math.Min(Target.MinFavor, Target.MaxFavor);
     *                 }
     *
     *                 ImGui.TextUnformatted($"{Language.TermsSpeed}:");
     *                 ImGui.SameLine(textWidth);
     *                 using (ImRaii.ItemWidth(sliderWidth))
     *                 {
     *                     if (ImGui.SliderInt("##shipSliderMinSpeed", ref Target.MinSpeed, LockedTarget.MinSpeed, LockedTarget.MaxSpeed, "Min %d"))
     *                         Target.MaxSpeed = Math.Max(Target.MinSpeed, Target.MaxSpeed);
     *
     *                     ImGui.SameLine();
     *
     *                     if (ImGui.SliderInt("##shipSliderMaxSpeed", ref Target.MaxSpeed, LockedTarget.MinSpeed, LockedTarget.MaxSpeed, "Max %d"))
     *                         Target.MinSpeed = Math.Min(Target.MinSpeed, Target.MaxSpeed);
     *                 }
     *             }
     *
     *             ImGuiHelpers.ScaledDummy(10.0f);
     *         }
     *         else
     *         {
     *             var secondRow = ImGui.GetWindowWidth() / 5.1f;
     *
     *             var breakpoints = Sectors.CalculateBreakpoint(CurrentBuild.Sectors);
     *
     *             Helper.TextColored(ImGuiColors.DalamudViolet, $"{Language.TermsBreakpoints}:");
     *             Helper.TextColored(ImGuiColors.HealerGreen, Language.TermsSurveillance);
     *             ImGui.SameLine(secondRow);
     *             ImGui.TextUnformatted($"T2: {breakpoints.T2} | T3: {breakpoints.T3}");
     *
     *             Helper.TextColored(ImGuiColors.HealerGreen, Language.TermsRetrieval);
     *             ImGui.SameLine(secondRow);
     *             ImGui.TextUnformatted($"{Language.TermsNormal}: {breakpoints.Normal} | {Language.TermsOptimal}: {breakpoints.Optimal}");
     *
     *             Helper.TextColored(ImGuiColors.HealerGreen, Language.TermsFavor);
     *             ImGui.SameLine(secondRow);
     *             ImGui.TextUnformatted($"{Language.TermsFavor}: {breakpoints.Favor}");
     *
     *             Helper.TextColored(ImGuiColors.DalamudViolet, $"{Language.TermsOptions}:");
     *
     *             if (ImGui.Checkbox(Language.BuilderShipCheckboxT1, ref Target.UseT1))
     *                 Target.UseT2 = false;
     *             ImGui.SameLine();
     *             if (ImGui.Checkbox(Language.BuilderShipCheckboxT2, ref Target.UseT2))
     *                 Target.UseT1 = false;
     *
     *             if (ImGui.Checkbox(Language.BuilderShipCheckboxPoor, ref Target.UsePoor))
     *                 Target.UseNormal = false;
     *             ImGui.SameLine();
     *             if (ImGui.Checkbox(Language.BuilderShipCheckboxNormal, ref Target.UseNormal))
     *                 Target.UsePoor = false;
     *
     *             ImGui.Checkbox(Language.BuilderShipCheckboxFavor, ref Target.IgnoreFavor);
     *             ImGui.SameLine();
     *             ImGui.Checkbox(Language.BuilderShipCheckboxModded, ref Target.NoModded);
     *
     *             ImGuiHelpers.ScaledDummy(10.0f);
     *         }
     *
     *         if (!FilterBuilds().Any())
     *         {
     *             ImGuiHelpers.ScaledDummy(20.0f);
     *
     *             var text = Language.BuilderShipCalculationNothingFound;
     *
     *             ImGui.SetCursorPosX((ImGui.GetWindowSize().X - ImGui.CalcTextSize(text).X) * 0.5f);
     *             Helper.TextColored(ImGuiColors.DalamudOrange, text);
     *             return true;
     *         }
     *
     *         using var table = ImRaii.Table("##shipTable", hasRoute ? 13 : 12, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable);
     *         if (!table.Success)
     *             return true;
     *
     *         ImGui.TableSetupColumn(Language.TermsCost);
     *         ImGui.TableSetupColumn(Language.TermsRepair);
     *         ImGui.TableSetupColumn(Language.TermsHull, ImGuiTableColumnFlags.NoSort);
     *         ImGui.TableSetupColumn(Language.TermsStern, ImGuiTableColumnFlags.NoSort);
     *         ImGui.TableSetupColumn(Language.TermsBow, ImGuiTableColumnFlags.NoSort);
     *         ImGui.TableSetupColumn(Language.TermsBridge, ImGuiTableColumnFlags.NoSort);
     *         ImGui.TableSetupColumn(Language.TermsSurveillance, ImGuiTableColumnFlags.PreferSortDescending);
     *         ImGui.TableSetupColumn(Language.TermsRetrieval, ImGuiTableColumnFlags.PreferSortDescending);
     *         ImGui.TableSetupColumn(Language.TermsFavor, ImGuiTableColumnFlags.PreferSortDescending);
     *         ImGui.TableSetupColumn(Language.TermsSpeed, ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortDescending);
     *         ImGui.TableSetupColumn(Language.TermsRange, ImGuiTableColumnFlags.PreferSortDescending);
     *         if (hasRoute)
     *             ImGui.TableSetupColumn(Language.TermsDuration, ImGuiTableColumnFlags.NoSort);
     *         ImGui.TableSetupColumn("##Import", ImGuiTableColumnFlags.NoSort);
     *
     *         ImGui.TableHeadersRow();
     *         var tableContent = SortBuilds(ImGui.TableGetSortSpecs().Specs).ToArray();
     *
     *         using var clipper = new ListClipper(tableContent.Length, itemHeight: ImGui.GetTextLineHeight() * 1.1f);
     *         foreach (var i in clipper.Rows)
     *         {
     *             var (build, time) = tableContent[i];
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.BuildCost}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.RepairCosts}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.HullIdentifier}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.SternIdentifier}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.BowIdentifier}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.BridgeIdentifier}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.Surveillance}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.Retrieval}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.Favor}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.Speed}");
     *
     *             ImGui.TableNextColumn();
     *             ImGui.TextUnformatted($"{build.Range}");
     *
     *             if (hasRoute)
     *             {
     *                 ImGui.TableNextColumn();
     *                 ImGui.TextUnformatted(ToTime(time));
     *             }
     *
     *             ImGui.TableNextColumn();
     *             if (ImGuiComponents.IconButton(i, FontAwesomeIcon.ArrowRightFromBracket))
     *             {
     *                 CurrentBuild.UpdateBuild(build, SelectedRank);
     *                 CurrentBuild.OriginalSub = 0;
     *             }
     *
     *             if (ImGui.IsItemHovered())
     *                 Helper.Tooltip(Language.BuilderShipTableSelect);
     *
     *             ImGui.TableNextRow();
     *         }
     *
     *         return true;
     *     }
     */

    async function refreshFiltering() {
        filteredPath = sortBuilds(filterBuilds());
    }

    function changeMapSelection(mapId: number) {
        map = SimpleSubMapSheet[mapId];

        availableSectors = Object.values(SimpleSubExplorationSheet).filter(s => s.Map === mapId && !s.StartingPoint).map(s => s.RowId);
        selectedSectors = [];
    }

    async function optionChanged(payload: {type: 'add' | 'remove' | 'removeAll' | 'selectAll' | 'reorder', option: Option}) {
        if (payload.type === 'selectAll' || payload.type === 'selectAll' || payload.type === 'reorder' || payload.type === 'removeAll' || payload.type === 'remove')
            return;

        let optionIndex = mapOptions.indexOf(payload.option.toString());
        if (optionIndex === -1) {
            console.error(`Option ${payload.option} not found in options array`);
            return;
        }

        changeMapSelection(optionsToId[optionIndex]);
    }

    async function selectItem(sector: number) {
        if (selectedSectors.length >= 5)
            return;

        if (!selectedSectors.includes(sector)) {
            selectedSectors.push(sector);

            bestSectorPath = FindCalculatedRoute(selectedSectors);
            selectedSectors = [...bestSectorPath.Path];
        }

        if (availableSectors.includes(sector)) {
            let idx = availableSectors.indexOf(sector);
            if (idx !== -1) {
                availableSectors.splice(idx, 1);
            }

            availableSectors.sort((a, b) => a - b);
        }

        await refreshFiltering();
    }

    async function deselectItem(sector: number) {
        if (selectedSectors.includes(sector)) {
            let idx = selectedSectors.indexOf(sector);
            if (idx !== -1) {
                selectedSectors.splice(idx, 1);
            }

            bestSectorPath = FindCalculatedRoute(selectedSectors);
            selectedSectors = [...bestSectorPath.Path];
        }

        if (!availableSectors.includes(sector)) {
            availableSectors.push(sector);
            availableSectors.sort((a, b) => a - b);
        }

        await refreshFiltering();
    }

    async function checkboxOptionsChanged() {
        await refreshFiltering();
    }

    async function targetOptionsChanged(target: TargetValues, targetOption: boolean) {
        targetOption = !targetOption;

        await refreshFiltering();
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<PageSidebar>
    <div class="d-flex flex-column gap-2 max-w-100 overflow-x-hidden">
        <MultiSelect
                bind:value={selectedOption}
                options={mapOptions}
                ulSelectedClass="multiSelect-selection"
                ulOptionsStyle="padding-left:0.5rem;"
                placeholder="Select a map"
                onchange={optionChanged}
                maxSelect={1}
                minSelect={1}
                required={true}
                portal={{ active: true }}
        />

        <select class="w-100 form-select" size="5">
            {#each selectedSectors as item}
                <option
                        class="border-bottom"
                        value={item}
                        onclick={async () => await deselectItem(item)}
                >
                    {ToLetterName(SimpleSubExplorationSheet[item])}
                </option>
            {/each}
        </select>

        <select class="w-100 form-select" size="15" disabled={selectedSectors.length >= 5}>
            {#each availableSectors as item}
                <option
                        class="border-bottom"
                        value={item}
                        onclick={async () => await selectItem(item)}
                >
                    {ToLetterName(SimpleSubExplorationSheet[item])}
                </option>
            {/each}
        </select>

        <h5 class="mt-3">Options:</h5>
        <Input class="mb-0" type="checkbox" bind:checked={ignoreBreakpoints} label="Ignore Breakpoints" on:change={async () => await checkboxOptionsChanged()}></Input>
        {#if !ignoreBreakpoints}
            <Input class="mb-0" type="checkbox" bind:checked={target.UseT1} label="Only Tier 1" on:change={async () => await targetOptionsChanged(target, target.UseT1)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.UseT2} label="Only Tier 2" on:change={async () => await targetOptionsChanged(target, target.UseT2)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.UsePoor} label="Only Poor" on:change={async () => await targetOptionsChanged(target, target.UsePoor)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.UseNormal} label="Only Normal" on:change={async () => await targetOptionsChanged(target, target.UseNormal)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.IgnoreFavor} label="Ignore Favor" on:change={async () => await targetOptionsChanged(target, target.IgnoreFavor)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.NoModded} label="No Modified Parts" on:change={async () => await targetOptionsChanged(target, target.NoModded)}></Input>
        {/if}
    </div>
</PageSidebar>
<div class="col-12 col-lg-9 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        <div class="container mb-5 p-2 rounded border tier-anchor" style="background-color: var(--bs-tertiary-bg);">
            {#if bestSectorPath.Path.length > 0}
                <div class="card mb-5 bg-secondary">
                    <div class="row g-0 align-items-center">
                        <div class="col-2">
                            <div class="item-card-icon px-3 d-flex align-items-center justify-content-center rounded-start h-100">
                                <h4 class="m-0">Route</h4>
                            </div>
                        </div>
                        <div class="col">
                            <div class="card-body">
                                <h5 class="card-title">{SectorsToPath(" -> ", bestSectorPath.Path)}</h5>
                                <p class="card-text m-0">
                                    Tier 2: {sectorBreakpoints.T2} - Tier 3: {sectorBreakpoints.T3}
                                    <br>
                                    Normal: {sectorBreakpoints.Normal} - Optimal: {sectorBreakpoints.Optimal}
                                    <br>
                                    Favor: {sectorBreakpoints.Favor}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
                {#each filteredPath as build}
                    <div class="card mb-3">
                        <div class="row g-0 align-items-center">
                            <div class="col-2">
                                <div class="item-card-icon px-3 d-flex align-items-center justify-content-center rounded-start h-100">
                                    <h4 class="m-0">{build.Build.FullIdentifier()}</h4>
                                </div>
                            </div>
                            <div class="col">
                                <div class="card-body">
                                    <h5 class="card-title">{getDuration(build.Time)}</h5>
                                    <p class="card-text m-0">
                                        Surv: {build.Build.Surveillance} - Ret: {build.Build.Retrieval} - Favor: {build.Build.Favor}
                                        <br>
                                        Speed: {build.Build.Speed} - Range: {build.Build.Range}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>
                {/each}
            {:else}
                <p>No sectors selected.</p>
            {/if}
        </div>
    </div>
</div>