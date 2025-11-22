<script lang="ts">
	import '../app.css';
	import 'bootstrap/dist/css/bootstrap.min.css';
	import 'bootstrap/dist/js/bootstrap.bundle.min.js';
	import favicon from '$lib/assets/favicon.svg';
    import {title, description} from "$lib/title.svelte"
    import Footer from "../component/Footer.svelte";
    import { page } from '$app/state';
    import {loadIcons} from "$lib/data";
	
	let { children } = $props();

    let active = $state(false);
    let isBrandItem = $state(true);

    let identifier1 = $state('containers');
    let identifier2 = $state('instances');
    let identifier3 = $state('others');

    loadIcons()
</script>

<svelte:head>
	<link rel="icon" href={favicon} />

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link rel="stylesheet" href="/css/fontawesome/all.css">

    <link rel="stylesheet" href="/css/main.css">
    <script type="text/javascript" src="/js/table.js"></script>
    <script type="text/javascript" src="/js/theme.js"></script>

    <title>{$title}</title>

    <meta property="og:url" content="{page.url.href}">
    <meta property="og:site_name" content="{$title}">
    <meta property="og:title" content="{$title}">
    <meta property="og:type" content="website">
    <meta property="og:description" content="{$description}" />
</svelte:head>

<div class="d-flex flex-column justify-content-between min-vh-100">
    <div>
        <nav class="navbar navbar-expand-lg sticky-top bg-dark border-bottom border-body" data-bs-theme="dark">
            <div class="container-fluid">
                <!-- Menu Toggler -->
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                    <span class="navbar-toggler-icon"></span>
                </button>

                <!-- Main Menu -->
                <div class="collapse navbar-collapse" id="navbarSupportedContent">
                    <ul class="menu-main navbar-nav me-auto mb-2 mb-lg-0">
                        <li class="nav-item">
                            <a class="nav-link" class:active={active} class:navbar-brand={isBrandItem} href="/">FFXIV Gacha</a>
                        </li>

                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle" class:active={active} href="#" id="navbarDropdown{identifier1}" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                                Containers
                            </a>
                            <ul class="dropdown-menu" aria-labelledby="navbarDropdown{identifier1}">
                                <li>
                                    <a class="dropdown-item" class:active={active} href="/coffer/" aria-current={active}>
                                        Coffers
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item" class:active={active} href="/lockbox/" aria-current={active}>
                                        Lockboxes
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item" class:active={active} href="/card/" aria-current={active}>
                                        Card Packs
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item" class:active={active} href="/logofrag/" aria-current={active}>
                                        Logograms and Fragments
                                    </a>
                                </li>
                            </ul>
                        </li>

                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle" class:active={active} href="#" id="navbarDropdown{identifier2}" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                                Instance Drops
                            </a>
                            <ul class="dropdown-menu" aria-labelledby="navbarDropdown{identifier2}">
                                <li>
                                    <a class="dropdown-item" class:active={active} href="/deep/" aria-current={active}>
                                        Deep Dungeons
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item" class:active={active} href="/loot/" aria-current={active}>
                                        Loot
                                    </a>
                                </li>
                            </ul>
                        </li>

                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle" class:active={active} href="#" id="navbarDropdown{identifier3}" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                                Others
                            </a>
                            <ul class="dropdown-menu" aria-labelledby="navbarDropdown{identifier3}">
                                <li>
                                    <a class="dropdown-item" class:active={active} href="/bunny/" aria-current={active}>
                                        Eureka Bunnies
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item" class:active={active} href="/occult/" aria-current={active}>
                                        Occult
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item" class:active={active} href="/desynth/" aria-current={active}>
                                        Desynthesis
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item" class:active={active} href="/venture/" aria-current={active}>
                                        Ventures
                                    </a>
                                </li>
                            </ul>
                        </li>
                    </ul>
                </div>

                <!-- Social Menu -->
                <div class="menu-social">
                    <ul class="navbar-nav flex-row align-items-center">
                        <li class="nav-item">
                            <a class="nav-link" href="/about" target='_self'>
                                About
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="https://github.com/Infiziert90/FFXIVGachaSpreadsheet" target='_blank'>
                                <i class="fab fa-brands fa-github text-white" style="font-size: 1.5em;"></i>
                            </a>
                        </li>
                        <li class="nav-item dropdown">
                            <button class="btn btn-link nav-link px-0 px-lg-2 py-2 dropdown-toggle d-flex align-items-center" id="bd-theme" type="button" aria-expanded="false" data-bs-toggle="dropdown" data-bs-display="static">
                                <i class="fas fa-moon fa-fw"></i>
                            </button>
                            <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="bd-theme-text">
                                <li>
                                    <button type="button" class="dropdown-item d-flex align-items-center" data-bs-theme-value="light" aria-pressed="false">
                                        <i class="fas fa-sun fa-fw me-2"></i> Light
                                    </button>
                                </li>
                                <li>
                                    <button type="button" class="dropdown-item d-flex align-items-center active" data-bs-theme-value="dark" aria-pressed="true">
                                        <i class="fas fa-moon fa-fw me-2"></i> Dark
                                    </button>
                                </li>
                                <li>
                                    <button type="button" class="dropdown-item d-flex align-items-center" data-bs-theme-value="auto" aria-pressed="false">
                                        <i class="fas fa-desktop fa-fw me-2"></i> Auto
                                    </button>
                                </li>
                            </ul>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>

        <div class="container">
            <div class="row gy-3 pt-3">
                {@render children()}
            </div>
        </div>
    </div>

    <Footer/>
</div>
