<script lang="ts">
	import '../app.css';
    import { Container, Styles } from '@sveltestrap/sveltestrap';
	import favicon from '$lib/assets/favicon.svg';
    import {title, description} from "$lib/title.svelte"
    import Footer from "../component/Footer.svelte";
    import { page } from '$app/state';
  import SiteNav from '../component/SiteNav.svelte';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link rel="stylesheet" href="/css/fontawesome/all.css">
    <Styles />
    <link rel="stylesheet" href="/css/main.css">
    <title>{$title}</title>

    <meta property="og:url" content="{page.url.href}">
    <meta property="og:site_name" content="{$title}">
    <meta property="og:title" content="{$title}">
    <meta property="og:type" content="website">
    <meta property="og:description" content="{$description}" />
</svelte:head>

<div class="d-flex flex-column justify-content-between min-vh-100">
    <div>
        <SiteNav />

        <Container>
            <div class="row gy-3 pt-3">
                {@render children()}
            </div>  
        </Container>
    </div>

    <Footer/>
</div>
