<script lang="ts">
    import { Mappings } from "$lib/mappings";
    import type { Reward } from "$lib/interfaces";
    import { getIconPath, getWikiUrl } from "$lib/utils";

    interface Props {
        reward: Reward;
    }

    let { reward }: Props = $props();
    let mapping = $state(Mappings[reward.Id]);
</script>

<div class="card mb-3">
    <div class="row g-0">
        <div class="col-md-2">
            <img class="img-fluid rounded-start" style="height: 100%" loading="lazy" src={getIconPath(mapping.Icon, true)} alt="{mapping.Name} Icon">
        </div>
        <div class="col-md-10">
            <div class="card-body">
                <h5 class="card-title"><a href={getWikiUrl(mapping.Name)} class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">{mapping.Name}</a></h5>
                <p class="card-text">
                    Obtained: {reward.Amount.toLocaleString()} Total: {reward.Total.toLocaleString()} Min-Max: {reward.Min}-{reward.Max}
                </p>
            </div>
        </div>
    </div>
</div>