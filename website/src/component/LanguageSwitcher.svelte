<script>
    import { currentLanguage, AVAILABLE_LANGUAGES } from '$lib/stores/language.ts';

    const HINT_DISMISSED_KEY = 'languageSwitcherHintDismissed';
    let showHint = $state(false);

    if (typeof window !== 'undefined') {
        try {
            showHint = localStorage.getItem(HINT_DISMISSED_KEY) !== 'true';
        } catch (error) {
            console.error('Error reading language switcher hint from local storage:', error);
        }
    }

    function dismissHint() {
        showHint = false;
        try {
            localStorage.setItem(HINT_DISMISSED_KEY, 'true');
        } catch (error) {
            console.error('Error saving language switcher hint to local storage:', error);
        }
    }

    function handleLanguageChange(event) {
        const selectedLanguage = event.target.value;
        currentLanguage.set(selectedLanguage);
        dismissHint();
    }
</script>

<div class="position-relative d-inline-flex">
    <fieldset class="language-switcher-fieldset d-flex flex-wrap align-items-center border rounded-2 px-1">
        <legend class="visually-hidden">Select Language</legend>
        {#each AVAILABLE_LANGUAGES as language}
            <label
                    class="language-option d-flex align-items-center px-2 py-1 text-body-secondary"
                    class:text-body-emphasis={$currentLanguage === language.code}
                    class:text-decoration-underline={$currentLanguage === language.code}
            >
                <input
                        type="radio"
                        class="visually-hidden"
                        name="language"
                        value={language.code}
                        checked={$currentLanguage === language.code}
                        onchange={handleLanguageChange}
                />
                {language.letter}
            </label>
        {/each}
    </fieldset>

    {#if showHint}
        <div class="popover bs-popover-bottom language-hint shadow" role="tooltip">
            <span class="popover-arrow"></span>
            <div class="popover-body d-flex align-items-start gap-2 small">
                <span>You can display item names in your language here</span>
                <button type="button" class="btn-close flex-shrink-0 mt-1" aria-label="Close" onclick={dismissHint}></button>
            </div>
        </div>
    {/if}
</div>

<style>
    /* Bootstrap has no hover/focus-within variant for the `border` utility,
       so the border highlight still needs an explicit rule. */
    .language-switcher-fieldset:hover,
    .language-switcher-fieldset:focus-within {
        border-color: var(--bs-emphasis-color);
    }

    /* Labels aren't inherently clickable, and the color/underline swap on
       hover, focus and selection has no Bootstrap utility equivalent. */
    .language-option {
        cursor: pointer;
        transition: color 150ms cubic-bezier(0.4, 0, 0.2, 1);
    }

    .language-option:hover,
    .language-option:focus-within {
        color: var(--bs-emphasis-color);
        text-decoration: underline;
    }

    /* Bootstrap's popover positioning relies on Popper.js, which isn't in use
       here, so the placement and arrow offset still need to be set manually. */
    .language-hint {
        position: absolute;
        top: 100%;
        right: 0;
        margin-top: 0.5rem;
        max-width: 220px;
        width: max-content;
    }

    .language-hint .popover-arrow {
        /* Bootstrap's popover-arrow relies on Popper.js to set `position:
           absolute` inline; without it the arrow renders in normal flow. */
        position: absolute;
        right: 1rem;
        left: auto;
    }

    /* Below the navbar's `lg` collapse breakpoint the switcher sits near the
       left edge of the stacked mobile menu instead of the far right of the
       navbar, so the hint has to anchor from the other side to stay on-screen. */
    @media (max-width: 991.98px) {
        .language-hint {
            right: auto;
            left: 0;
        }

        .language-hint .popover-arrow {
            right: auto;
            left: 1rem;
        }
    }
</style>
