(() => {
    const name = document.getElementById('Name');
    const slug = document.getElementById('Slug');
    if (name?.dataset.autoSlug === 'true' && slug) {
        let editedSlug = slug.value.length > 0;
        slug.addEventListener('input', () => { editedSlug = slug.value.length > 0; });
        name.addEventListener('input', () => {
            if (editedSlug) return;
            slug.value = name.value.toLowerCase().replace(/đ/g, 'd').normalize('NFD')
                .replace(/[\u0300-\u036f]/g, '').replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
        });
    }
    const parent = document.getElementById('parentCategoryId');
    const child = document.getElementById('SubcategoryId');
    if (!parent || !child) return;
    const update = () => {
        for (const option of child.options) {
            if (!option.dataset.parent) continue;
            option.hidden = option.dataset.parent !== parent.value;
            option.disabled = option.hidden;
        }
        if (child.selectedOptions[0]?.disabled) child.value = '';
    };
    parent.addEventListener('change', () => { child.value = ''; update(); });
    update();
})();
