(() => {
    const canvas = document.getElementById('revenue-chart');
    const source = document.getElementById('revenue-data');
    if (!canvas || !source) return;
    const rows = JSON.parse(source.textContent);
    const format = value => value >= 1000000 ? `${(value / 1000000).toFixed(value >= 10000000 ? 0 : 1)}tr` : value >= 1000 ? `${Math.round(value / 1000)}k` : `${value}`;
    function draw() {
        const width = Math.max(canvas.parentElement.clientWidth, rows.length * 24, 580);
        const height = 330;
        const ratio = window.devicePixelRatio || 1;
        canvas.style.width = `${width}px`;
        canvas.width = Math.round(width * ratio);
        canvas.height = Math.round(height * ratio);
        const ctx = canvas.getContext('2d');
        ctx.scale(ratio, ratio);
        const left = 58, right = 50, top = 30, bottom = 45;
        const areaW = width - left - right, areaH = height - top - bottom;
        const maxRevenue = Math.max(1, ...rows.map(x => x.revenue)) * 1.15;
        const maxOrders = Math.max(1, ...rows.map(x => Math.max(x.newOrders, x.completedOrders))) * 1.15;
        ctx.font = '11px Manrope, sans-serif';
        ctx.lineWidth = 1;
        for (let i = 0; i <= 4; i++) {
            const y = top + areaH * i / 4;
            ctx.strokeStyle = 'rgba(224,224,230,.13)';
            ctx.beginPath(); ctx.moveTo(left, y); ctx.lineTo(width - right, y); ctx.stroke();
            ctx.fillStyle = '#b9afc2';
            ctx.textAlign = 'right'; ctx.fillText(format(maxRevenue * (1 - i / 4)), left - 8, y + 4);
            ctx.textAlign = 'left'; ctx.fillText(String(Math.round(maxOrders * (1 - i / 4))), width - right + 8, y + 4);
        }
        const step = areaW / rows.length;
        const barW = Math.max(5, Math.min(17, step * .57));
        rows.forEach((row, i) => {
            const x = left + step * (i + .5);
            const barH = row.revenue / maxRevenue * areaH;
            const grad = ctx.createLinearGradient(0, top + areaH - barH, 0, top + areaH);
            grad.addColorStop(0, '#cf4265'); grad.addColorStop(1, '#800020');
            ctx.fillStyle = grad; ctx.fillRect(x - barW / 2, top + areaH - barH, barW, barH);
            if (i % Math.ceil(rows.length / 10) === 0 || i === rows.length - 1) {
                ctx.fillStyle = '#b9afc2'; ctx.textAlign = 'center'; ctx.fillText(row.date, x, height - 17);
            }
        });
        function line(key, color) {
            ctx.beginPath(); ctx.lineWidth = 2.5; ctx.strokeStyle = color;
            rows.forEach((row, i) => {
                const x = left + step * (i + .5), y = top + areaH * (1 - row[key] / maxOrders);
                i ? ctx.lineTo(x, y) : ctx.moveTo(x, y);
            });
            ctx.stroke();
            rows.forEach((row, i) => {
                const x = left + step * (i + .5), y = top + areaH * (1 - row[key] / maxOrders);
                ctx.beginPath(); ctx.fillStyle = color; ctx.arc(x, y, 2.7, 0, Math.PI * 2); ctx.fill();
            });
        }
        line('newOrders', '#b56df0');
        line('completedOrders', '#65cbea');
    }
    new ResizeObserver(draw).observe(canvas.parentElement);
    draw();
})();
