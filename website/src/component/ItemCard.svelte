<script lang="ts">
    import { getIconPath, getWikiUrl } from "$lib/utils";
    import type {Reward} from "$lib/structs/reward";
    import {ItemMappings} from "$lib/mappings";
    import {currentLanguage} from "$lib/stores/language";
    import {localizedItem} from "$lib/localization";

    interface Props {
        reward: Reward;
    }

    let { reward }: Props = $props();
</script>

<div class="card mb-3">
    <div class="row g-0 align-items-center">
        <div class="col-auto">
            <div class="item-card-icon px-3 d-flex align-items-center justify-content-center rounded-start h-100">
                <img class="item-icon" style="width: 56px; height: 56px;" loading="lazy" src={getIconPath(ItemMappings[reward.Id].Icon, true)} alt="{localizedItem(reward.Id, $currentLanguage)} Icon">
            </div>
        </div>
        <div class="col">
            <div class="card-body">
                <h5 class="card-title"><a href={getWikiUrl(ItemMappings[reward.Id].En)} class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">{localizedItem(reward.Id, $currentLanguage)}</a></h5>
                <p class="card-text">
                    Obtained: {reward.Amount.toLocaleString()} Total: {reward.Total.toLocaleString()} Min-Max: {reward.Min}-{reward.Max}
                </p>
            </div>
        </div>
    </div>
</div>