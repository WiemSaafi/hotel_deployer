// ── TunisiaStay — Site JavaScript ──────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {

  // Auto-dismiss flash messages after 4s
  document.querySelectorAll('.alert-success-custom, .alert-danger-custom').forEach(el => {
    setTimeout(() => {
      el.style.transition = 'opacity 0.5s ease';
      el.style.opacity = '0';
      setTimeout(() => el.remove(), 500);
    }, 4000);
  });

  // Animate fade-in-up elements on scroll
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(e => {
      if (e.isIntersecting) e.target.style.animationPlayState = 'running';
    });
  }, { threshold: 0.1 });

  document.querySelectorAll('.fade-in-up').forEach(el => {
    el.style.animationPlayState = 'paused';
    observer.observe(el);
  });

  // Date pickers: enforce depart > arrivee
  const arrInput = document.querySelector('[name="DateArrivee"]');
  const depInput = document.querySelector('[name="DateDepart"]');
  if (arrInput && depInput) {
    arrInput.addEventListener('change', () => {
      if (depInput.value && depInput.value <= arrInput.value) {
        const next = new Date(arrInput.value);
        next.setDate(next.getDate() + 1);
        depInput.value = next.toISOString().split('T')[0];
      }
      depInput.min = arrInput.value;
    });
  }

  // Navbar scroll shrink effect
  const navbar = document.querySelector('.navbar');
  if (navbar) {
    window.addEventListener('scroll', () => {
      navbar.style.padding = window.scrollY > 50 ? '0.5rem 0' : '0.9rem 0';
    });
  }

  // Confirm delete buttons
  document.querySelectorAll('[data-confirm]').forEach(btn => {
    btn.addEventListener('click', e => {
      if (!confirm(btn.dataset.confirm)) e.preventDefault();
    });
  });
});
