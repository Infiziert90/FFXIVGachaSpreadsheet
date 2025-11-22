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

        colorMode,
        Icon,

        Button
    } from '@sveltestrap/sveltestrap';

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
        }
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
    let isOpen = false;
    if (window && window.innerWidth > 992) {
        isOpen = true;
    } else {
        isOpen = false;
    }
    const toggle = () => (isOpen = !isOpen);
</script>

<Navbar color="dark" dark theme="dark" expand="md" class="border-bottom border-body-tertiary">
<NavbarBrand href="/">FFXIV Gacha</NavbarBrand>
<NavbarToggler on:click={toggle} />

<Collapse {isOpen} navbar expand="md">
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

    <!-- Social Menu -->
    <Nav class="ms-auto" navbar>
        <NavItem>
            <NavLink href="/about">About</NavLink>
        </NavItem>
        <NavItem>
            <NavLink href="https://github.com/Infiziert90/FFXIVGachaSpreadsheet" target="_blank">
                <Icon name="github" class="text-white"/>
                <span class="visually-hidden">GitHub</span>
            </NavLink>
        </NavItem>

        <NavItem>
            <Dropdown nav>
                <DropdownToggle nav caret>
                    <Icon name={$colorMode === 'light' ? 'sun-fill' : $colorMode === 'dark' ? 'moon-stars-fill' : 'circle-half'} />
                </DropdownToggle>
                <DropdownMenu end>
                    <DropdownItem onclick={() => colorMode.set('light')} active={$colorMode === 'light'}>Light <Icon name="sun-fill" /></DropdownItem>
                    <DropdownItem onclick={() => colorMode.set('dark')} active={$colorMode === 'dark'}>Dark <Icon name="moon-stars-fill" /></DropdownItem>
                    <DropdownItem onclick={() => colorMode.set('auto')} active={$colorMode === 'auto'}>Auto <Icon name="circle-half" /></DropdownItem>
                </DropdownMenu>
            </Dropdown>
        </NavItem>
    </Nav>
</Collapse>
</Navbar>