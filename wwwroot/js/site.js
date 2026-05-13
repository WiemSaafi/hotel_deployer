// ── TunisiaStay — Modern Site JavaScript ──────────────────────────────

document.addEventListener('DOMContentLoaded', () => {

  // Auto-dismiss flash messages with smooth animation
  document.querySelectorAll('.alert-success-custom, .alert-danger-custom').forEach(el => {
    setTimeout(() => {
      el.style.transition = 'all 0.6s cubic-bezier(0.4,0,0.2,1)';
      el.style.opacity = '0';
      el.style.transform = 'translateY(-20px)';
      setTimeout(() => el.remove(), 600);
    }, 4000);
  });

  // Scroll reveal with stagger
  const revealObserver = new IntersectionObserver((entries) => {
    entries.forEach((e, i) => {
      if (e.isIntersecting) {
        setTimeout(() => {
          e.target.classList.add('visible');
          e.target.style.animationPlayState = 'running';
        }, i * 60);
      }
    });
  }, { threshold: 0.08, rootMargin: '0px 0px -40px 0px' });

  document.querySelectorAll('.reveal, .fade-in-up').forEach(el => {
    if (el.classList.contains('fade-in-up')) el.style.animationPlayState = 'paused';
    revealObserver.observe(el);
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

  // Navbar scroll effect with smooth transition
  const navbar = document.querySelector('.navbar');
  if (navbar) {
    let lastScroll = 0;
    window.addEventListener('scroll', () => {
      const scrollY = window.scrollY;
      navbar.style.padding = scrollY > 50 ? '0.4rem 0' : '0.9rem 0';
      if (scrollY > 100) {
        navbar.style.boxShadow = '0 4px 30px rgba(0,0,0,0.15)';
      } else {
        navbar.style.boxShadow = 'none';
      }
      lastScroll = scrollY;
    });
  }

  // Smooth hover effects on cards
  document.querySelectorAll('.hotel-card, .room-card, .stat-card').forEach(card => {
    card.addEventListener('mouseenter', () => {
      card.style.transition = 'all 0.45s cubic-bezier(0.4,0,0.2,1)';
    });
  });

  // Confirm delete buttons
  document.querySelectorAll('[data-confirm]').forEach(btn => {
    btn.addEventListener('click', e => {
      if (!confirm(btn.dataset.confirm)) e.preventDefault();
    });
  });

  // Animate stat numbers on scroll
  const animateCounters = () => {
    document.querySelectorAll('.stat-value').forEach(el => {
      const target = parseInt(el.textContent.replace(/[^\d]/g, ''));
      if (isNaN(target) || el.dataset.animated) return;
      const obs = new IntersectionObserver(entries => {
        if (entries[0].isIntersecting) {
          el.dataset.animated = 'true';
          let current = 0;
          const step = Math.ceil(target / 40);
          const timer = setInterval(() => {
            current += step;
            if (current >= target) { current = target; clearInterval(timer); }
            el.textContent = current.toLocaleString('fr-FR');
          }, 30);
          obs.disconnect();
        }
      });
      obs.observe(el);
    });
  };
  animateCounters();

  // Add smooth page transition
  document.querySelectorAll('a:not([target="_blank"]):not([href^="#"])').forEach(link => {
    if (link.href && link.href.startsWith(window.location.origin)) {
      link.addEventListener('click', (e) => {
        // Don't interfere with form buttons or special actions
        if (link.closest('form') || link.classList.contains('no-transition')) return;
      });
    }
  });
});
