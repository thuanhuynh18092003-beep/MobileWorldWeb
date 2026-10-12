(() => {
    for (const [pickerId, codeId] of [['variant-color-picker', 'ColorHex'], ['product-color-picker', 'Color']]) {
        const picker = document.getElementById(pickerId);
        const code = document.getElementById(codeId);
        if (!picker || !code) continue;
        picker.addEventListener('input', () => { code.value = picker.value.toUpperCase(); });
        code.addEventListener('input', () => { if (/^#[0-9a-f]{6}$/i.test(code.value)) picker.value = code.value; });
    }
    for (const pair of document.querySelectorAll('[data-color-pair]')) {
        const picker = pair.querySelector('[data-color-picker]');
        const code = pair.querySelector('[data-color-code]');
        if (!picker || !code) continue;
        picker.addEventListener('input', () => { code.value = picker.value.toUpperCase(); });
        code.addEventListener('input', () => { if (/^#[0-9a-f]{6}$/i.test(code.value)) picker.value = code.value; });
    }
})();
