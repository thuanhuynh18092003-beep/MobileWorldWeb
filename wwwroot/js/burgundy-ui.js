(() => {
    const root = document.documentElement;
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)');

    function syncGlow() {
        const enabled = !root.classList.contains('glow-off');
        document.querySelectorAll('[data-glow-toggle]').forEach(button => {
            button.setAttribute('aria-pressed', String(enabled));
            button.setAttribute('aria-label', `${enabled ? 'Tắt' : 'Bật'} hiệu ứng sáng Burgundy`);
            button.title = `${enabled ? 'Tắt' : 'Bật'} hiệu ứng sáng Burgundy`;
        });
    }
    syncGlow();
    document.querySelectorAll('[data-glow-toggle]').forEach(button => {
        button.addEventListener('click', () => {
            root.classList.toggle('glow-off');
            try { localStorage.setItem('burgundyGlow', root.classList.contains('glow-off') ? 'off' : 'on'); } catch { /* Private mode can disable storage. */ }
            syncGlow();
        });
    });

    const orbit = document.querySelector('[data-technology-orbit]');
    const features = {
        chip: ['Hiệu năng', 'Chọn cấu hình phù hợp cho công việc, giải trí và sáng tạo mỗi ngày.'],
        camera: ['Nhiếp ảnh', 'So sánh camera để lưu giữ rõ nét những khoảnh khắc bạn yêu thích.'],
        display: ['Màn hình', 'Tìm kích thước và chất lượng hiển thị phù hợp với cách bạn sử dụng.']
    };
    orbit?.querySelectorAll('[data-technology]').forEach(button => {
        button.addEventListener('click', () => {
            const feature = button.dataset.technology;
            if (!features[feature]) return;
            orbit.dataset.active = feature;
            orbit.querySelectorAll('[data-technology]').forEach(item => item.setAttribute('aria-pressed', String(item === button)));
            document.querySelector('#technology-feature-title').textContent = features[feature][0];
            document.querySelector('#technology-feature-description').textContent = features[feature][1];
        });
    });

    if (!reduceMotion.matches && window.matchMedia('(hover: hover) and (pointer: fine)').matches) {
        document.querySelectorAll('.product-card').forEach(card => {
            card.addEventListener('pointermove', event => {
                const rect = card.getBoundingClientRect();
                const x = (event.clientX - rect.left) / rect.width - .5;
                const y = (event.clientY - rect.top) / rect.height - .5;
                card.style.setProperty('--tilt-x', `${(-y * 5).toFixed(2)}deg`);
                card.style.setProperty('--tilt-y', `${(x * 5).toFixed(2)}deg`);
            }, { passive: true });
            card.addEventListener('pointerleave', () => {
                card.style.removeProperty('--tilt-x');
                card.style.removeProperty('--tilt-y');
            });
        });
    }

    const loader = document.querySelector('.cosmic-loader');
    const finishLoading = () => {
        root.classList.remove('js-loading');
        loader?.setAttribute('aria-hidden', 'true');
    };
    if (!loader || reduceMotion.matches) finishLoading();
    else {
        if (document.readyState === 'complete') window.setTimeout(finishLoading, 450);
        else window.addEventListener('load', () => window.setTimeout(finishLoading, 450), { once: true });
        window.setTimeout(finishLoading, 2200);
    }
})();
