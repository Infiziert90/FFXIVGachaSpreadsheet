<script lang="ts">
    import { Icon, Button, Offcanvas } from '@sveltestrap/sveltestrap';
    import { browser } from '$app/environment';

    interface Props {
        title?: string;
        sidebarId?: string;
        colClass?: string;
        children?: import('svelte').Snippet;
    }

    let { 
        title = 'Filter your category',
        sidebarId = 'offcanvasFilter',
        colClass = 'col-12 col-lg-3 order-0 order-lg-1 sticky-left-col',
        children
    }: Props = $props();

    // Generate accessible label ID for the offcanvas header
    const labelId = $derived(`${sidebarId}Label`);
    
    // Track window width for responsive behavior (992px = Bootstrap's lg breakpoint)
    let windowWidth = $state(browser && typeof window !== 'undefined' ? window.innerWidth : 0);
    
    // Determine if we're on desktop (lg breakpoint and above)
    const isDesktop = $derived(windowWidth >= 992);
    
    // Offcanvas open state: starts open on desktop, closed on mobile
    let isOpen = $state(browser && typeof window !== 'undefined' ? window.innerWidth >= 992 : false);

    /**
     * Toggle the offcanvas visibility.
     * Only works on mobile - desktop always stays open.
     */
    function toggle(): void {
        if (!browser) return;
        // Only allow toggling on mobile screens (< 992px)
        if (windowWidth < 992) {
            isOpen = !isOpen;
        }
    }

    /**
     * Monitor window resize and maintain desktop behavior.
     * On desktop (>= 992px), keep the sidebar always open.
     * On mobile, allow it to be toggled.
     */
    $effect(() => {
        if (!browser) return;
        
        const handleResize = () => {
            windowWidth = window.innerWidth;
            // Force open on desktop, allow toggle on mobile
            if (windowWidth >= 992) {
                isOpen = true;
            }
        };
        
        // Check initial size and set up resize listener
        handleResize();
        window.addEventListener('resize', handleResize);
        
        // Cleanup listener on component destroy
        return () => window.removeEventListener('resize', handleResize);
    });

    /**
     * Set the ID on the offcanvas header's h5 element for accessibility.
     * This ensures aria-labelledby works correctly.
     * Runs when isOpen changes to ensure the element exists.
     */
    $effect(() => {
        if (!browser || !isOpen) return;
        
        // Small delay to ensure DOM is updated
        const timeoutId = setTimeout(() => {
            const offcanvasElement = document.getElementById(sidebarId);
            if (offcanvasElement) {
                const headerH5 = offcanvasElement.querySelector('.offcanvas-title');
                if (headerH5) {
                    headerH5.id = labelId;
                }
            }
        }, 0);
        
        return () => clearTimeout(timeoutId);
    });
</script>

<!-- Mobile toggle button - only visible on screens smaller than lg (992px) -->
<Button 
    color="primary"
    size="lg"
    class="rounded-xl d-lg-none position-fixed bottom-0 end-0 m-3 w-auto z-3" 
    onclick={toggle}
    aria-label="Open filter menu"
>
    <Icon name="funnel-fill" />
</Button>

<!-- Sidebar container with responsive column classes -->
<div class={colClass}>
    <!-- 
        Offcanvas component configuration:
        - scroll: Allows scrolling within the offcanvas body
        - isOpen: Controls visibility (always true on desktop)
        - toggle: Disabled on desktop (undefined) to prevent closing, enabled on mobile
        - backdrop: Disabled on desktop (no overlay), enabled on mobile
        - placement="start": Slides in from the left
        - lg={true}: Adds 'offcanvas-lg' class for desktop display (always visible on lg+)
        - container="inline": Renders in place instead of portal (stays in column layout)
    -->
    <Offcanvas 
        scroll
        isOpen={isOpen} 
        toggle={isDesktop ? undefined : toggle}
        backdrop={!isDesktop}
        placement="start"
        lg={true}
        container="inline"
        id={sidebarId}
        aria-labelledby={labelId}
        header={title}
    >
        {@render children()}
    </Offcanvas>
</div>
