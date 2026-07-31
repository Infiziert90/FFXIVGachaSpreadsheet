<script>
    import { currentLanguage, AVAILABLE_LANGUAGES } from '$lib/stores/language.ts';
    
    function handleLanguageChange(event) {
        const selectedLanguage = event.target.value;
        currentLanguage.set(selectedLanguage);
    }
</script>

<div class="language-switcher">
    <fieldset>
        <legend class="sr-only">Select Language</legend>
        {#each AVAILABLE_LANGUAGES as language}
            <label>
                <input
                        type="radio"
                        name="language"
                        value={language.code}
                        checked={$currentLanguage === language.code}
                        onchange={handleLanguageChange}
                        class="sr-only"
                />
                <span class="lang-text">
                    {language.letter}
                </span>
            </label>
        {/each}
    </fieldset>
</div>

<style>
    fieldset {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        border: 1px solid #334155;
        border-radius: 0.375rem;
        padding: 0 0.25rem;
        margin: 0;
        transition: border-color 200ms cubic-bezier(0.4, 0, 0.2, 1);
    }

    fieldset:hover,
    fieldset:focus-within {
        border-color: #ffffff;
    }

    label {
        display: flex;
        align-items: center;
        cursor: pointer;
    }


    .lang-text {
        padding: 0.25rem 0.5rem;
        color: #d1d5db;
        transition: color 150ms cubic-bezier(0.4, 0, 0.2, 1),
        text-decoration-color 150ms cubic-bezier(0.4, 0, 0.2, 1);
    }

    .lang-text:hover, label:focus-within .lang-text {
        text-decoration: underline;
        color: #ffffff;
    }

    input:checked + .lang-text {
        text-decoration: underline;
        color: #ffffff;
    }

    .sr-only {
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        white-space: nowrap;
        border-width: 0;
    }
</style>
