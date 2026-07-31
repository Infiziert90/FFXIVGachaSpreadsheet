<script lang="ts">
    import { getIconPath, getWikiUrl } from "$lib/utils";
    import {ItemMappings} from "$lib/mappings";
    import {localizedItem} from "$lib/localization";
    import {currentLanguage} from "$lib/stores/language";

    interface Props {
        itemId: number;
    }

    let { itemId }: Props = $props();
</script>

<div class="card mb-3">
    <div class="row g-0 align-items-center">
        {#if ItemMappings.hasOwnProperty(itemId)}
            <div class="col-auto">
                <div class="item-card-icon px-3 d-flex align-items-center justify-content-center rounded-start h-100">
                    <img class="item-icon" style="width: 56px; height: 56px;" loading="lazy" src={getIconPath(ItemMappings[itemId].Icon, true)} alt="{localizedItem(itemId, $currentLanguage)} Icon">
                </div>
            </div>
            <div class="col">
                <div class="card-body">
                    <h5 class="card-title"><a href={getWikiUrl(ItemMappings[itemId].En)} class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">{localizedItem(itemId, $currentLanguage)}</a></h5>
                </div>
            </div>
        {/if}
    </div>
</div>