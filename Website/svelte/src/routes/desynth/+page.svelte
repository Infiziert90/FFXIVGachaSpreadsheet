<script lang="ts">
    import { page } from '$app/state';
    import { replaceState } from "$app/navigation";
    import type { DesynthBase, DesynthHistory, Reward } from "$lib/interfaces";
    import { Mappings } from "$lib/data";
    import { onMount } from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import type { ColumnTemplate } from "$lib/table";
    import { Accordion, AccordionItem } from "@sveltestrap/sveltestrap";

    interface Props {
        content: DesynthBase;
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));
    let sourceSearchResultElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));
    let rewardSearchResultElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));

    let { data }: Props = $props();

    const desynthBase: DesynthBase = data.content;

    // Table data
    let tableItems: Reward[] = $state([]);
    let tableColumns: ColumnTemplate[] = $state([]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Inputs
    let sourceSearch = $state('');
    let rewardSearch = $state('');

    // add defaults if things aren't set correctly
    let sourceParam = $state(0);
    let rewardParam = $state(0);
    getSearchParams();

    onMount(() => {
        if (sourceParam > 0) {
            sourceSearch = Mappings[sourceParam].Name;
            sourceInput(new InputEvent('input'))
            onButtonClick(sourceParam, desynthBase.Sources, 'Records', false)
        }

        if (rewardParam > 0) {
            rewardSearch = Mappings[rewardParam].Name;
            rewardInput(new InputEvent('input'))
            onButtonClick(rewardParam, desynthBase.Rewards, 'Received', false)
        }
    })

    function onButtonClick(id: number, usedData: Record<number, DesynthHistory>, statsType: string, addQuery: boolean) {
        if (addQuery) {
            page.url.searchParams.delete(statsType !== 'Desynths' ? 'source' : 'reward')
            page.url.searchParams.set(statsType === 'Desynths' ? 'source' : 'reward', id.toString());
            replaceState(page.url, page.state);
        }

        let loadedData = usedData[id];

        // Update table data
        tableItems = loadedData.Rewards;
        tableColumns = [
            {
                header: '',
                sortable: false,
                templateRenderer: (row) => {
                    return `<img width="40" height="40" loading="lazy" src="https://v2.xivapi.com/api/asset?path=ui/icon/${Mappings[row.Id].Icon}_hr1.tex&format=png">`
                },
                classExtension: ['icon']
            },
            {header: 'Name', field: 'Id', mappingSort: true, valueRenderer: (row) => Mappings[row.Id].Name}, // TODO Fix sorting for field not existing
            {header: 'Obtained', field: 'Amount', classExtension: ['number', 'text-center']},
            {
                header: 'Min-Max',
                field: 'Min',
                valueRenderer: (row) => `${row.Min}–${row.Max}`,
                classExtension: ['number', 'text-center']
            },
            {
                header: 'Chance',
                field: 'Pct',
                defaultSort: 'asc',
                valueRenderer: (row) => `${(row.Percentage * 100).toFixed(2)}%`,
                classExtension: ['percentage', 'text-end']
            },
        ];

        // Update stats
        titleStats = `${Mappings[id].Name} Stats`;
        totalStats = `${statsType}: ${loadedData.Records.toLocaleString()}`;
        selectedStats = ` `;

        let buttons = document.getElementsByClassName("resultButton");
        Array.from(buttons).forEach(function (element) {
            element.classList.remove("btn-success");
        });

        let button = document.getElementById(`${id}`);
        if (button) {
            button.classList.add("btn-success")
        }
    }

    function inputChecker(searchValue: string, usedData: Record<number, DesynthHistory>, statsType: string, outputElement: HTMLDivElement) {
        const first10 = [];
        Object.keys(usedData).find(e => (Mappings[e].Name.toLowerCase().includes(searchValue.toLowerCase()) && first10.push(e), first10.length >= 10));

        outputElement.innerHTML = '';
        if (first10.length > 0) {
            for (let id of first10) {
                let itemInfo = Mappings[id];

                let iconImg = document.createElement('img');
                iconImg.classList.add("icon-small");
                iconImg.style = `width: 40px; height: 40px; float: left;`;
                iconImg.src = `https://v2.xivapi.com/api/asset?path=ui/icon/${itemInfo.Icon}_hr1.tex&format=png`;

                let nameSpan = document.createElement('span');
                nameSpan.style = 'position: relative; top: 0.2rem;';
                nameSpan.innerText = itemInfo.Name;

                let button = document.createElement('button');
                button.id = id;
                button.classList.add("btn", "btn-primary", "btn-sm", "resultButton");
                button.type = 'button';
                button.onclick = function(){onButtonClick(id, usedData, statsType, true)}

                button.append(iconImg, nameSpan);
                outputElement.appendChild(button);
            }
        }
        else {
            outputElement.innerText = "Nothing found";
        }
    }

    function getSearchParams() {
        if (page.url.searchParams.has('source')) {
            sourceParam = parseInt(page.url.searchParams.get('source')!);
        }

        if (page.url.searchParams.has('reward')) {
            rewardParam = parseInt(page.url.searchParams.get('reward')!);
        }
    }


    function sourceInput(event: InputEvent) {
        inputChecker(sourceSearch, desynthBase.Sources, 'Desynths', sourceSearchResultElement);
    }

    function rewardInput(event: InputEvent) {
        inputChecker(rewardSearch, desynthBase.Rewards, 'Received', rewardSearchResultElement);
    }
</script>

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
            <Accordion theme="dark" class="w-100" stayOpen>
                <AccordionItem active header="Source Search">
                    <div class="accordion-body">
                        <div class="input-group mb-3">
                            <input id="sourceSearchInput"
                                   type="text"
                                   class="form-control"
                                   placeholder="Search a source item ..."
                                   aria-label="Sources"
                                   bind:value={sourceSearch}
                                   oninput={sourceInput} >
                        </div>
                        <div id="sourceSearchResult" class="d-grid gap-1" bind:this={sourceSearchResultElement}></div>
                    </div>
                </AccordionItem>
                <AccordionItem active header="Reward Search">
                    <div class="accordion-body">
                        <div class="input-group mb-3">
                            <input id="rewardSearchInput"
                                   type="text"
                                   class="form-control"
                                   placeholder="Search a reward item ..."
                                   aria-label="Rewards"
                                   bind:value={rewardSearch}
                                   oninput={rewardInput} >
                        </div>
                        <div id="rewardSearchResult" class="d-grid gap-1" bind:this={rewardSearchResultElement}></div>
                    </div>
                </AccordionItem>
            </Accordion>
        </div>
    </div>
</div>
<div class="col-12 col-lg-2 order-0 order-lg-3">
    <div id="stats" class="stats">
        <div class="card">
            <div class="card-header">
                <strong>{titleStats}</strong>
            </div>
            <ul class="list-group list-group-flush">
                <li class="list-group-item">{totalStats}</li>
                <li class="list-group-item">{selectedStats}</li>
                <li class="list-group-item">
                    <label for="patch">Patch</label>
                </li>
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if tableItems.length > 0 && tableColumns.length > 0}
            <DropsTable items={tableItems} columns={tableColumns} />
        {:else}
            <p>No data found</p>
        {/if}
    </div>
</div>