<!--{{ $data := dict }}-->
<!--{{ $json := resources.Get (.Param "data") }}-->
<!--{{ $iconPaths := resources.Get "data/IconPaths.json"}}-->
<!--{{ with $json | transform.Unmarshal }}-->
<!--{{ $data = . }}-->
<!--{{ end }}-->
<script lang="ts">
    import { page } from '$app/state';
    import {replaceState} from "$app/navigation";
    import type { PageProps } from './$types';
    import type {Coffer} from "$lib/interfaces";
    import {IconPaths} from "$lib/data";
    import {onMount} from "svelte";
    import {makeSortableTable} from "$lib/table";

    // html elements
    let tabContentElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});
    let tableElement: HTMLTableElement = $state(<HTMLTableElement>(document.createElement('table')));
    let totalStatsElement: HTMLLIElement = $state(<HTMLLIElement>(document.createElement('li')));
    let selectedStatsElement: HTMLLIElement = $state(<HTMLLIElement>(document.createElement('li')));
    let titleStatsElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));

    let { data }: PageProps = $props();
    let patches: string[] = $state([])
    let selectedPatch = $state();

    let cofferData: Coffer[] = data.content;

    for (const patch of Object.keys(cofferData[0].Coffers[0].Patches)) {
        patches.push(patch)
    }

    // add defaults if things aren't set correctly
    let territory = $state(cofferData[0].Territory);
    let coffer = $state(cofferData[0].Coffers[0].CofferId);
    getSearchParams();

    onMount(() => {
        openTab(territory, coffer, false)
    })


    // TODO: Readd this
    // {{ range $data }}
    // var element = document.getElementById('{{ .Territory }}-button');
    // element.classList.add("collapsed");
    // {{ end }}
    //
    // let collapseActive = document.getElementById(`${territory}-collapse`);
    // collapseActive.classList.add("show");
    //
    // let collapseButton = document.getElementById(`${territory}-button`);
    // collapseButton.classList.remove("collapsed");
    // collapseButton.setAttribute("aria-expanded", "true")

    function openTab(territory, coffer, addQuery) {
        if (addQuery)
        {
            page.url.searchParams.set('territory', territory);
            page.url.searchParams.set('coffer', coffer);

            replaceState(page.url, page.state);
        }

        // Declare all variables
        let i;

        // Iterate all tab elements and remove the class "active"
        for (const element of Object.values(tabElements)) {
            element.classList.remove('active');
        }

        // Show the current tab, and add an "active" class to the link that opened the tab
        tabContentElement.style.display = "block";
        tabElements[`${territory}${coffer}`].classList.add('active');

        let variantData = cofferData.find((e) => e.Territory === territory);
        if (!variantData) return;

        let loadedCoffer = variantData.Coffers.find((e) => e.CofferId === coffer)
        if (!loadedCoffer) return;

        let requestedPatch = patches[selectedPatch];
        let patchData = loadedCoffer.Patches[requestedPatch];

        makeSortableTable(tableElement, patchData.Items, [
            {
                header: '',
                sortable: false,
                templateRenderer: (row) => {
                    console.log(row)
                    return `<img width="40" height="40" loading="lazy" src="https://v2.xivapi.com/api/asset?path=ui/icon/${IconPaths[row.Id]}_hr1.tex&format=png">`
                },
                classExtension: ['icon']
            },
            {
                header: 'Name',
                field: 'Name',
                templateRenderer: (row) => {
                    const wikiName = row.Name.replace(/\s+/g, '_');
                    return `<a href="https://ffxiv.consolegameswiki.com/wiki/${wikiName}" class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">${row.Name}</a>`
                }
            },
            {header: 'Obtained', field: 'Amount', classExtension: ['number', 'text-center']},
            {header: 'Total', field: 'Total', classExtension: ['number', 'text-center']},
            {
                header: 'Min-Max',
                field: 'Min',
                valueRenderer: (row) => `${row.Min}–${row.Max}`,
                classExtension: ['number', 'text-center']
            },
            {
                header: 'Chance',
                field: 'Percentage',
                defaultSort: 'asc',
                valueRenderer: (row) => `${(row.Percentage * 100).toFixed(2)}%`,
                classExtension: ['percentage', 'text-end']
            },
        ]);

        titleStatsElement.innerHTML = `<strong>${variantData.Name} Stats</strong>`;

        let total = 0;
        variantData.Coffers.forEach(coffer => {total += coffer.Patches[requestedPatch].Total})
        totalStatsElement.innerText = `Total: ${total.toLocaleString()}`
        selectedStatsElement.innerText = `${loadedCoffer.CofferName}: ${patchData.Total.toLocaleString()}`

        patches.length = 0;
        for (const key of Object.keys(loadedCoffer.Patches)) {
            patches.push(key)
        }

        // Scroll to the top of the page
        window.scrollTo(0, 0);
    }

    function patchSelectionChanged(event: Event) {
        if (!event.currentTarget)
            return;

        let element = (event.currentTarget as HTMLSelectElement);

        getSearchParams()
        openTab(territory, coffer, false)
    }

    function getSearchParams() {
        if (page.url.searchParams.has('territory') && page.url.searchParams.has('coffer')) {
            territory = parseInt(page.url.searchParams.get('territory')!);
            coffer = parseInt(page.url.searchParams.get('coffer')!);
        }
    }
</script>

<div>

</div>

<button class="btn btn-primary btn-lg rounded-xl d-lg-none position-fixed bottom-0 end-0 m-3 w-auto z-3" type="button" data-bs-toggle="offcanvas" data-bs-target="#offcanvasFilter" aria-controls="offcanvasFilter">
    <i class="fas fa-filter"></i>
</button>

<div class="col-12 col-lg-3 order-0 order-lg-1 sticky-left-col">
    <div class="offcanvas-lg offcanvas-start" tabindex="-1" id="offcanvasFilter" aria-labelledby="offcanvasFilterLabel">
        <div class="offcanvas-header">
            <h5 class="offcanvas-title" id="offcanvasFilterLabel">Filter your category</h5>
            <button type="button" class="btn-close" data-bs-dismiss="offcanvas" data-bs-target="#offcanvasFilter" aria-label="Close"></button>
        </div>
        <div class="offcanvas-body">
            <div class="accordion w-100" id="accordionExample">
                {#each cofferData as coffer}
                <div class="accordion-item">
                    <h2 class="accordion-header">
                        <button id="{coffer.Territory}-button"
                                class="accordion-button"
                                class:collapsed={coffer.Territory !== territory}
                                type="button"
                                data-bs-toggle="collapse"
                                data-bs-target="#{coffer.Territory}-collapse"
                                aria-expanded={coffer.Territory === territory}
                                aria-controls="{coffer.Territory}-collapse">
                            {coffer.Name}
                        </button>
                    </h2>
                    <div id="{coffer.Territory}-collapse" class="accordion-collapse collapse" class:show={coffer.Territory === territory} data-bs-parent="#accordionExample">
                        <div class="accordion-body">
                            {#each coffer.Coffers as cofferVariant}
                            <div class="tab">
                                <button id="{coffer.Territory}{cofferVariant.CofferId}-tab"
                                        class="tablinks btn accordion-body-btn"
                                        onclick={() => openTab(coffer.Territory, cofferVariant.CofferId, true)}
                                        bind:this={tabElements[`${coffer.Territory}${cofferVariant.CofferId}`]}>
                                    {cofferVariant.CofferName}
                                </button>
                            </div>
                            {/each}
                        </div>
                    </div>
                </div>
                {/each}
            </div>
        </div>
    </div>
</div>
<div class="col-12 col-lg-2 order-0 order-lg-3">
    <div id="stats" class="stats">
        <div class="card">
            <div class="card-header" bind:this={titleStatsElement}>
            </div>
            <ul class="list-group list-group-flush">
                <li class="list-group-item" bind:this={totalStatsElement}></li>
                <li class="list-group-item" bind:this={selectedStatsElement}></li>
                <li class="list-group-item">
                    <label for="patch">Patch</label>
                    <select id="patch" class="form-select" bind:value={selectedPatch} onchange={patchSelectionChanged}>
                        {#each patches as patch, idx}
                            <option value={idx}>
                                {patch}
                            </option>
                        {/each}
                    </select>
                </li>
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        <table id="table1" class="table table-striped align-middle table-sm table-hover table-borderless" bind:this={tableElement}></table>
    </div>
</div>