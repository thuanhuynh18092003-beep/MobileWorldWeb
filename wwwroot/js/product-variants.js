(() => {
    const root = document.querySelector('[data-variant-selector]');
    const source = document.getElementById('product-variants-data');
    if (!root || !source) return;
    const variants = JSON.parse(source.textContent);
    if (!variants.length) return;
    const money = value => new Intl.NumberFormat('vi-VN').format(value) + '₫';
    const colorButtons = [...document.querySelectorAll('[data-color]')];
    const storageButtons = [...document.querySelectorAll('[data-storage]')];
    let selected = variants.find(x => x.id === Number(root.dataset.selectedVariantId)) || variants.find(x => x.isDefault && x.stock > 0) || variants.find(x => x.stock > 0) || variants[0];
    const render = () => {
        const photo = document.getElementById('variant-photo');
        const placeholder = document.getElementById('variant-placeholder');
        if (selected.imageUrl) {
            photo.src = selected.imageUrl;
            photo.alt = `${document.querySelector('.details-copy h1').textContent} ${selected.colorName} ${selected.storage}`;
            photo.hidden = false;
            placeholder.hidden = true;
        } else {
            photo.removeAttribute('src');
            photo.hidden = true;
            placeholder.hidden = false;
        }
        document.getElementById('variant-visual').style.setProperty('--phone-color', selected.colorHex);
        document.getElementById('variant-price').textContent = money(selected.price);
        const oldPrice = document.getElementById('variant-old-price');
        oldPrice.hidden = selected.oldPrice == null;
        oldPrice.textContent = selected.oldPrice == null ? '' : money(selected.oldPrice);
        document.getElementById('selected-color-name').textContent = selected.colorName;
        document.getElementById('variant-spec-storage').textContent = selected.storage;
        document.getElementById('selected-variant-id').value = selected.id;
        const quantity = document.getElementById('variant-quantity');
        quantity.max = selected.stock;
        quantity.value = Math.min(Math.max(Number(quantity.value) || 1, 1), Math.max(selected.stock, 1));
        const button = document.getElementById('variant-add-button');
        button.disabled = selected.stock < 1;
        button.textContent = selected.stock < 1 ? 'Tạm hết hàng' : 'Thêm vào giỏ hàng ↗';
        document.getElementById('variant-stock').textContent = selected.stock > 0
            ? `✓ Còn ${selected.stock} sản phẩm · Sẵn sàng giao hàng` : 'Phiên bản này hiện tạm hết hàng';
        colorButtons.forEach(button => button.setAttribute('aria-pressed', button.dataset.color === selected.colorName));
        storageButtons.forEach(button => button.setAttribute('aria-pressed', button.dataset.storage === selected.storage));
        const viewer = document.querySelector('.details-3d-button');
        if (viewer) viewer.dataset.productColor = selected.colorHex;
    };
    // Color and storage choices are regular links to the selected variant. Native
    // navigation keeps image, price and stock in sync even when scripts are blocked.
    render();
})();
