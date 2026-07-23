<script lang="ts">
    import { Mappings } from "$lib/mappings";
    import { getIconPath, getWikiUrl } from "$lib/utils";

    interface Props {
        itemId: number;
    }

    let { itemId }: Props = $props();
</script>

<div class="card mb-3">
    <div class="row g-0 align-items-center">
        {#if itemId in Mappings}
            <div class="col-auto">
                <div class="item-card-icon px-3 d-flex align-items-center justify-content-center rounded-start h-100">
                    <img class="item-icon" style="width: 56px; height: 56px;" loading="lazy" src={getIconPath(Mappings[itemId].Icon, true)} alt="{Mappings[itemId].Name} Icon">
                </div>
            </div>
            <div class="col">
                <div class="card-body">
                    <h5 class="card-title"><a href={getWikiUrl(Mappings[itemId].Name)} class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">{Mappings[itemId].Name}</a></h5>
                </div>
            </div>
        {/if}
    </div>
</div>