<script lang="ts">
    import 'bootstrap/dist/css/bootstrap.min.css';
    import 'bootstrap-icons/font/bootstrap-icons.css';
    import 'leaflet/dist/leaflet.css';
	import '../app.css';
    import {Container} from '@sveltestrap/sveltestrap';
	import faviconPng from '$lib/assets/favicon.png';
	import faviconSvg from '$lib/assets/favicon.svg';
    import Footer from "../component/Footer.svelte";
    import { layoutWidth } from '$lib/stores/layoutWidth';
    import { page } from '$app/state';
    import SiteNav from '../component/SiteNav.svelte';
	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={faviconSvg} type="image/svg+xml" />
	<link rel="icon" href={faviconPng} type="image/png" />

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <!--    Static   -->
    <meta property="og:url" content="{page.url.href}">
    <meta property="og:type" content="website">

    <!-- IBM Plex fonts -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:ital,wght@0,300;0,400;0,500;0,700;1,400&family=IBM+Plex+Sans+JP:wght@300;400;500;700&display=swap" rel="stylesheet">
</svelte:head>

<div class="d-flex flex-column justify-content-between min-vh-100">
    <div>
        <SiteNav />

        <Container fluid={$layoutWidth === 'fluid'}>
            <div class="row gy-3 pt-3">
                {@render children()}
            </div>  
        </Container>
    </div>

    <Footer/>
</div>
