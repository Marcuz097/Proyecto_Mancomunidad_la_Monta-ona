(() => {
    function initHome() {
        const $ = s => document.querySelector(s), $$ = s => [...document.querySelectorAll(s)];
        const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
        const rio = $('#rio');
        if (!rio || rio.dataset.init === '1') return; // solo en la portada y una sola vez
        rio.dataset.init = '1';

        // 1. Reproductor Somos Río Lempa: rota tus imágenes y sincroniza con los botones
        const vids = [
            { t: 'Somos Río Lempa', c: 'Protegemos las fuentes de la cuenca del Lempa.' },
            { t: 'Un Gigante Natural', c: 'Reserva ecológica a 1,700 metros de altura en Chalatenango.' },
            { t: 'Fuente de Vida', c: 'Zona de recarga acuífera que alimenta ríos y manantiales.' },
            { t: 'Agua para El Salvador', c: 'Aquí está el agua que da vida a El Salvador.' }
        ];
        let i = 0, playing = !reduce, p = 0; const DUR = 6000;
        const slides = $$('.hero-slider img'), cap = $('#cap'), vt = $('#vt'), prog = $('#prog'), pp = $('#pp'), btns = $$('.list button');

        function show(n) {
            if (!slides.length) return;
            i = (n + slides.length) % slides.length; p = 0;
            slides.forEach((img, k) => img.classList.toggle('active', k === i));
            if (cap) cap.textContent = vids[i]?.c || '';
            if (vt) vt.textContent = vids[i]?.t || 'Somos Río Lempa';
            btns.forEach((b, k) => { b.classList.toggle('on', k === i); b.setAttribute('aria-pressed', k === i); });
        }

        function setPP() {
            if (!pp) return;
            pp.textContent = playing ? '❚❚' : '▶';
            pp.setAttribute('aria-label', playing ? 'Pausar' : 'Reproducir');
            const badge = $('.badge');
            if (badge) badge.style.opacity = playing ? 1 : .4;
        }

        btns.forEach((b, k) => b.addEventListener('click', () => show(k)));
        if (pp) pp.addEventListener('click', () => { playing = !playing; setPP(); });

        let last = performance.now();
        (function tick(now) {
            if (!document.body.contains(rio)) return; // detiene el ciclo si sales de Home
            const d = now - last; last = now;
            if (playing) {
                p += d;
                if (p >= DUR) show(i + 1);
                if (prog) prog.style.width = (p / DUR * 100) + '%';
            }
            requestAnimationFrame(tick);
        })(last);
        show(0); setPP();

        // 2. Observatorio: Hoy / Mañana con barras de lluvia por franja de 3 h
        const F = [
            { r: [10, 15, 30, 55, 70, 60, 35, 20], a: '<b>Precaución:</b> tormentas en la tarde. Evite quemas y senderos cercanos a quebradas.' },
            { r: [5, 10, 15, 25, 35, 30, 15, 10], a: '<b>Vigilancia:</b> calor al mediodía. Mantenga atención ante conatos de incendio.' }
        ];
        const horas = $('#horas'), al = $('#al'), tabs = $$('.dia');
        function dia(n) {
            tabs.forEach((t, k) => { t.classList.toggle('on', k === n); t.setAttribute('aria-selected', k === n); });
            if (horas) horas.innerHTML = F[n].r.map((v, k) => `<div class="h" title="${k * 3}:00, lluvia ${v} %"><i style="height:${v}%"></i><span>${k * 3}h</span></div>`).join('');
            if (al) al.innerHTML = F[n].a;
        }
        tabs.forEach((t, k) => t.addEventListener('click', () => dia(k)));
        dia(0);

        // 3. Programas: tocar para ver el detalle
        const MAS = [
            'Patrullajes, brigadas y monitoreo de especies. Los reportes de guardabosques alimentan este programa.',
            'Sistemas agropastoriles y restauración de suelos. Las familias participantes se registran con su DUI.',
            'Mesas del agua con juntas y comunidades de la cuenca.',
            'Planificación, transparencia y formación de equipos técnicos.',
            'Rutas, guías locales y emprendimientos comunitarios.'
        ];
        $$('.eje').forEach((e, k) => {
            if (!e.querySelector('.mas')) {
                const m = document.createElement('p');
                m.className = 'mas';
                m.textContent = MAS[k] || '';
                e.append(m);
            }
            e.tabIndex = 0;
            e.setAttribute('role', 'button');
            e.setAttribute('aria-expanded', 'false');
            const t = () => e.setAttribute('aria-expanded', e.classList.toggle('open'));
            e.addEventListener('click', t);
            e.addEventListener('keydown', ev => { if (ev.key === 'Enter' || ev.key === ' ') { ev.preventDefault(); t(); } });
        });

        // 4. Formulario de incidente en ventana, con vista previa de foto y ubicación del móvil
        const dlg = $('#dlg'), form = $('#fi'), ok = $('#ok');
        if (dlg && form && ok) {
            $$('[data-open]').forEach(a => a.addEventListener('click', e => { e.preventDefault(); ok.hidden = true; form.hidden = false; dlg.showModal(); }));
            $('#cerrar')?.addEventListener('click', () => dlg.close());
            dlg.addEventListener('click', e => { if (e.target === dlg) dlg.close(); });
            $('#foto')?.addEventListener('change', e => { const f = e.target.files[0]; if (f) { $('#pv').src = URL.createObjectURL(f); $('#pv').hidden = false; } });
            $('#gps')?.addEventListener('click', () => {
                const m = $('#gm'); m.textContent = 'Buscando tu ubicación…';
                if (!navigator.geolocation) { m.textContent = 'Tu navegador no permite ubicación. Escríbela a mano.'; return; }
                navigator.geolocation.getCurrentPosition(
                    g => { $('#ubi').value = g.coords.latitude.toFixed(5) + ', ' + g.coords.longitude.toFixed(5); m.textContent = 'Ubicación añadida.'; },
                    () => { m.textContent = 'No pudimos obtenerla. Escríbela a mano.'; }
                );
            });
            form.addEventListener('submit', e => { e.preventDefault(); form.hidden = true; ok.hidden = false; });
        }
    }

    // Ejecuta al cargar y observa cambios de navegación en Blazor
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initHome);
    } else {
        initHome();
    }
    new MutationObserver(initHome).observe(document.body, { childList: true, subtree: true });
})();