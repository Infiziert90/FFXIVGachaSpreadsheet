<script>
import {getLastUpdate} from "$lib/utils.ts";
import {browser} from "$app/environment";
</script>

<footer class="bg-body-tertiary text-body-tertiary text-center py-3 mt-5">
    <p class="mb-0"><strong>Made by Infi</strong></p>
    <p class="mb-0">
        <strong>Last Update</strong>:
        {#await getLastUpdate(browser) then value}
            {value}
        {/await}
    </p>
</footer>