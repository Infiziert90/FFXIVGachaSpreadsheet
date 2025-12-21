import adapter from '@sveltejs/adapter-cloudflare';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

/** @type {import('@sveltejs/kit').Config} */
const config = {
    // Consult https://svelte.dev/docs/kit/integrations
    // for more information about preprocessors
    preprocess: vitePreprocess(),
    kit: {
        adapter: adapter({
            // See below for an explanation of these options
            fallback: 'plaintext',
            routes: {
                include: ['/*'],
                exclude: ['<all>']
            }
        })
    }
};

export default config;