<script lang="ts">
    let {cardTitle, cardLink, cardIcon} = $props();
</script>

<div class="col">
    <div class="border rounded-3 p-3 position-relative h-100">
        <div class="row">
            <div class="col-md-4 text-center text-md-start">
                <img src="https://v2.xivapi.com/api/asset?path=ui/icon/{cardIcon}_hr1.tex&format=png" class="img-fluid rounded-start" alt="Icon for {cardTitle}" height="64" width="64">
            </div>
            <div class="col-md-8 d-flex align-items-center justify-content-center justify-content-md-start">
                <a class="fs-5 stretched-link text-decoration-none link-body-emphasis" href="{cardLink}">{cardTitle}</a>
            </div>
        </div>
    </div>
</div>