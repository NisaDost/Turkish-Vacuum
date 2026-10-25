(function () {
    'use strict';

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Mobil menü
    const hamburger = document.getElementById('hamburgerMenu');
    const navMenu = document.getElementById('navMenu');

    if (hamburger && navMenu) {
        const setMenuOpen = function (open) {
            hamburger.classList.toggle('active', open);
            navMenu.classList.toggle('active', open);
            hamburger.setAttribute('aria-expanded', String(open));
            hamburger.setAttribute('aria-label', open ? hamburger.dataset.labelClose : hamburger.dataset.labelOpen);
        };

        hamburger.addEventListener('click', function () {
            setMenuOpen(!navMenu.classList.contains('active'));
        });

        // Bir bağlantıya tıklanınca, menü dışına tıklanınca veya Esc ile kapat
        navMenu.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () { setMenuOpen(false); });
        });
        document.addEventListener('click', function (event) {
            if (!event.target.closest('.navbar')) setMenuOpen(false);
        });
        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') setMenuOpen(false);
        });
        window.matchMedia('(min-width: 992px)').addEventListener('change', function () { setMenuOpen(false); });
    }

    // Ana sayfa hero arka plan görselleri
    const slides = document.querySelectorAll('.hero-slide');
    if (slides.length > 1 && !reduceMotion) {
        let current = 0;
        setInterval(function () {
            slides[current].classList.remove('active');
            current = (current + 1) % slides.length;
            slides[current].classList.add('active');
        }, 7000);
    }

    // Kaydırdıkça beliren içerikler
    const revealItems = document.querySelectorAll('[data-reveal]');
    if ('IntersectionObserver' in window && !reduceMotion) {
        const revealObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    revealObserver.unobserve(entry.target);
                }
            });
        }, { rootMargin: '0px 0px -8% 0px', threshold: 0.05 });
        revealItems.forEach(function (item) { revealObserver.observe(item); });
    } else {
        revealItems.forEach(function (item) { item.classList.add('is-visible'); });
    }

    // Instagram gömmeleri ağır olduğu için betik, bölüme yaklaşıldığında yüklenir
    const embedsSection = document.querySelector('.embeds-section');
    if (embedsSection) {
        const loadInstagram = function () {
            if (document.getElementById('instagram-embed')) return;
            const script = document.createElement('script');
            script.id = 'instagram-embed';
            script.async = true;
            script.src = 'https://www.instagram.com/embed.js';
            document.body.appendChild(script);
        };

        if ('IntersectionObserver' in window) {
            const embedObserver = new IntersectionObserver(function (entries) {
                if (entries.some(function (entry) { return entry.isIntersecting; })) {
                    loadInstagram();
                    embedObserver.disconnect();
                }
            }, { rootMargin: '600px 0px' });
            embedObserver.observe(embedsSection);
        } else {
            loadInstagram();
        }
    }
})();
