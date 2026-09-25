document.querySelectorAll('.quantity-control').forEach(control => {
  const input = control.querySelector('input');
  control.querySelector('[data-qty-minus]')?.addEventListener('click', () => { input.value = Math.max(Number(input.min) || 1, Number(input.value) - 1); });
  control.querySelector('[data-qty-plus]')?.addEventListener('click', () => { input.value = Math.min(Number(input.max) || Infinity, Number(input.value) + 1); });
});
