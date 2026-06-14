<script lang="ts">
    import { page } from '$app/state';
    import {
        Collapse,
        Navbar,
        NavbarToggler,
        NavbarBrand,
        Nav,
        NavItem,
        NavLink,
        Dropdown,
        DropdownToggle,
        DropdownMenu,
        DropdownItem,
        Button,
        colorMode,
        Icon
    } from '@sveltestrap/sveltestrap';
    import { pageSidebarStore } from '$lib/stores/pageSidebar';
    import { layoutWidth } from '$lib/stores/layoutWidth';
    import { colorScheme } from '$lib/stores/colorScheme';
    import logoSvg from '$lib/assets/logo.svg?raw';

    interface MenuItem {
        label: string;
        href: string;
    }

    interface MenuCategory {
        label: string;
        id: string;
        items: MenuItem[];
    }

    const menuCategories: MenuCategory[] = [
        {
            label: 'Containers',
            id: 'containers',
            items: [
                { label: 'Coffers', href: '/coffer/' },
                { label: 'Lockboxes', href: '/lockbox/' },
                { label: 'Card Packs', href: '/card/' },
                { label: 'Logograms and Fragments', href: '/logofrag/' }
            ]
        },
        {
            label: 'Instance Drops',
            id: 'instances',
            items: [
                { label: 'Deep Dungeons', href: '/deep/' },
                { label: 'Loot', href: '/loot/' }
            ]
        },
        {
            label: 'Others',
            id: 'others',
            items: [
                { label: 'Eureka Bunnies', href: '/bunny/' },
                { label: 'Occult', href: '/occult/' },
                { label: 'Desynthesis', href: '/desynth/' },
                { label: 'Ventures', href: '/venture/' }
            ]
        },
        {
            label: 'Submarines',
            id: 'submarine',
            items: [
                { label: 'Submarines', href: '/submarine/' }
            ]
        },
    ];

    // Normalize path for comparison (remove trailing slashes)
    function normalizePath(path: string): string {
        return path.replace(/\/$/, '') || '/';
    }

    // Check if a path matches the current page
    function isActivePath(href: string): boolean {
        const currentPath = normalizePath(page.url.pathname);
        const itemPath = normalizePath(href);
        return currentPath === itemPath;
    }
        
    // Depending on the screen size, the menu is open or closed
    // This is a hack to make the menu open on large screens by default, and close on small screens
    let isOpen = $state(false);
    if (typeof window !== 'undefined' && window.innerWidth > 992) {
        isOpen = true;
    }
    const toggle = () => (isOpen = !isOpen);

    // Initialize colorMode from localStorage
    if (typeof window !== 'undefined') {
        try {
            const storedTheme = localStorage.getItem('theme');
            if (storedTheme && (storedTheme === 'light' || storedTheme === 'dark' || storedTheme === 'auto')) {
                colorMode.set(storedTheme as 'light' | 'dark' | 'auto');
            }
        } catch (error) {
            console.error('Error reading theme from local storage:', error);
        }
    }

    // Sync colorMode changes to localStorage
    $effect(() => {
        if (typeof window === 'undefined') return;
        const theme = $colorMode;
        try {
            localStorage.setItem('theme', theme);
        } catch (error) {
            console.error('Error saving theme to local storage:', error);
        }
    });
</script>

<Navbar expand="lg" class="border-bottom border-body-tertiary" sticky="top">
    {#if $pageSidebarStore.showButton && $pageSidebarStore.toggle}
        <Button
            class="navbar-toggler d-lg-none"
            onclick={() => $pageSidebarStore.toggle?.()}
            aria-label="Open filter menu"
        >
            <div class="d-flex align-items-center justify-content-center" style="width: 1.5em;height: 1.5em;">
                <Icon name="funnel-fill" />
            </div>
        </Button>
    {/if}
    <NavbarBrand href="/" class="p-0">
        <span class="site-logo">{@html logoSvg}</span>
        <span class="visually-hidden">XIVStats</span>
    </NavbarBrand>
    <NavbarToggler onclick={toggle} />

<Collapse {isOpen} navbar expand="lg">
    <!-- Main Menu -->
    <Nav class="me-auto" navbar>
        {#each menuCategories as category}
            <NavItem>
                <Dropdown nav>
                    <DropdownToggle nav class="nav-link" caret>
                        {category.label}
                    </DropdownToggle>
                    <DropdownMenu>
                        {#each category.items as item}
                            <DropdownItem href={item.href} active={isActivePath(item.href)}>
                                {item.label}
                            </DropdownItem>
                        {/each}
                    </DropdownMenu>
                </Dropdown>
            </NavItem>
        {/each}
    </Nav>

    <!-- Social + Settings Menu -->
    <Nav class="ms-auto" navbar>
        <NavItem>
            <NavLink href="/about">About</NavLink>
        </NavItem>

        <NavItem>
            <NavLink href="https://ko-fi.com/infiii" target="_blank">
                <Icon name="suit-heart-fill" />
                <span class="visually-hidden">KoFi</span>
            </NavLink>
        </NavItem>

        <NavItem>
            <NavLink href="https://github.com/Infiziert90/FFXIVGachaSpreadsheet" target="_blank">
                <Icon name="github" />
                <span class="visually-hidden">GitHub</span>
            </NavLink>
        </NavItem>

        <NavItem>
            <Dropdown nav>
                <DropdownToggle nav caret aria-label="Settings">
                    <Icon name="gear" />
                </DropdownToggle>
                <DropdownMenu end>
                    <div class="dropdown-header">Layout width</div>
                    <DropdownItem onclick={() => layoutWidth.set('fixed')} active={$layoutWidth === 'fixed'}>
                        <Icon name="arrows-angle-contract" class="me-2" /> Fixed
                    </DropdownItem>
                    <DropdownItem onclick={() => layoutWidth.set('fluid')} active={$layoutWidth === 'fluid'}>
                        <Icon name="arrows-angle-expand" class="me-2" /> Fluid
                    </DropdownItem>

                    <div class="dropdown-divider"></div>

                    <div class="dropdown-header">Palette</div>
                    <DropdownItem onclick={() => colorScheme.set('tinted')} active={$colorScheme === 'tinted'}>
                        <Icon name="water" class="me-2" /> Tinted
                    </DropdownItem>
                    <DropdownItem onclick={() => colorScheme.set('neutral')} active={$colorScheme === 'neutral'}>
                        <Icon name="circle-half" class="me-2" /> Neutral
                    </DropdownItem>

                    <div class="dropdown-divider"></div>

                    <div class="dropdown-header">Mode</div>
                    <DropdownItem onclick={() => colorMode.set('light')} active={$colorMode === 'light'}>
                        <Icon name="sun-fill" class="me-2" /> Light
                    </DropdownItem>
                    <DropdownItem onclick={() => colorMode.set('dark')} active={$colorMode === 'dark'}>
                        <Icon name="moon-stars-fill" class="me-2" /> Dark
                    </DropdownItem>
                    <DropdownItem onclick={() => colorMode.set('auto')} active={$colorMode === 'auto'}>
                        <Icon name="display" class="me-2" /> Auto
                    </DropdownItem>
                </DropdownMenu>
            </Dropdown>
        </NavItem>
    </Nav>
</Collapse>
</Navbar>

<style>
    .site-logo {
        display: flex;
        align-items: center;
    }
    .site-logo :global(svg) {
        height: 30px;
        width: auto;
        color: inherit;
    }
</style>
