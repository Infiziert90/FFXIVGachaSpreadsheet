<script lang="ts">
    import { Mappings } from "$lib/mappings";
    import { getIconPath, getWikiUrl } from "$lib/utils";
    import type {ReductionSource} from "$lib/structs/reduction";

    interface Props {
        reward: ReductionSource;
    }

    let { reward }: Props = $props();
</script>

<div class="card mb-3">
    <div class="row g-0 align-items-center">
        <div class="col-auto">
            <div class="item-card-icon px-3 d-flex align-items-center justify-content-center rounded-start h-100">
                <img class="item-icon" loading="lazy" src={getIconPath(Mappings[reward.Id].Icon, true)} alt="{Mappings[reward.Id].Name} Icon">
            </div>
        </div>
        <div class="col">
            <div class="card-body">
                <h5 class="card-title"><a href={getWikiUrl(Mappings[reward.Id].Name)} class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">{Mappings[reward.Id].Name}</a></h5>
                <p class="card-text">
                    Lowest Sand: {reward.LowestSand === 10000 ? 'No Data' : reward.LowestSand}
                    {#if reward.LowestBonus !== 10000}
                        <br>
                        Lowest Bonus: {reward.LowestBonus}
                    {/if}
                </p>
            </div>
        </div>
    </div>
</div>