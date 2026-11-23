(() => {
    const $ = (s, r = document) => r.querySelector(s);
    const $$ = (s, r = document) => [...r.querySelectorAll(s)];
    const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
    const root = document.documentElement;
    const guardar = (k, v) => { try { localStorage.setItem(k, v); } catch { } };

    document.addEventListener('click', e => {
        const fs = e.target.closest('[data-fs]');
        if (fs) {
            let n = parseInt(root.dataset.fs || '0', 10) + (fs.dataset.fs === 'up' ? 1 : -1);
            n = Math.max(0, Math.min(2, n));
            n ? root.dataset.fs = n : root.removeAttribute('data-fs');
            guardar('fs', n);
        }
        if (e.target.closest('[data-tema]')) {
            const oscuro = root.dataset.theme === 'dark';
            oscuro ? root.removeAttribute('data-theme') : root.dataset.theme = 'dark';
            guardar('tema', oscuro ? 'light' : 'dark');
            $$('[data-tema]').forEach(b => b.textContent = oscuro ? '☀ Claro' : '☾ Oscuro');
        }
        if (e.target.closest('#burger')) {
            const m = $('#menu'); const ab = m.classList.toggle('open');
            $('#burger').setAttribute('aria-expanded', ab);
        } else if (e.target.closest('#menu a')) { $('#menu')?.classList.remove('open'); }
        const z = e.target.closest('[data-zoom]');
        if (z) {
            const v = $('#visor'); if (!v) return;
            const img = z.matches('img') ? z : $('img', z);
            $('#visorFoto').src = z.dataset.zoom || img?.src || '';
            $('#visorTitulo').textContent = z.dataset.titulo || img?.alt || '';
            v.showModal();
        }
        if (e.target.id === 'visor') e.target.close();
        const tb = e.target.closest('[data-tab]');
        if (tb) {
            $$('[data-tab]').forEach(b => { const on = b === tb; b.classList.toggle('on', on); b.setAttribute('aria-selected', on); });
            $$('[data-panel]').forEach(p => p.hidden = p.dataset.panel !== tb.dataset.tab);
        }
    });
    window.addEventListener('scroll', () => $('#cab')?.classList.toggle('scrolled', scrollY > 8), { passive: true });

    function init() {
        root.classList.add('js');
        const txt = root.dataset.theme === 'dark' ? '☾ Oscuro' : '☀ Claro';
        $$('[data-tema]').forEach(b => { if (b.textContent !== txt) b.textContent = txt; });

        const rv = $$('.rv:not(.vis)');
        if (reduce || !('IntersectionObserver' in window)) rv.forEach(x => x.classList.add('vis'));
        else {
            const io = new IntersectionObserver(es => es.forEach(en => { if (en.isIntersecting) { en.target.classList.add('vis'); io.unobserve(en.target); } }), { threshold: .12 });
            rv.forEach(x => io.observe(x));
        }

        $$('[data-count]:not([data-done])').forEach(el => {
            const fin = +el.dataset.count, suf = el.dataset.suffix || '';
            el.textContent = reduce ? fin + suf : '0' + suf;
            const io = new IntersectionObserver(([en]) => {
                if (!en.isIntersecting) return; io.disconnect(); el.dataset.done = '1';
                if (reduce) return;
                const t0 = performance.now(), D = 1400;
                (function f(t) { const k = Math.min((t - t0) / D, 1); el.textContent = Math.round(fin * (1 - Math.pow(1 - k, 3))) + suf; if (k < 1) requestAnimationFrame(f); })(t0);
            }, { threshold: .6 });
            io.observe(el);
        });

        initObservatorio(); initPlayer(); initDenuncia();
    }

    function initObservatorio() {
        const horas = $('#horas'); if (!horas || horas.dataset.init) return; horas.dataset.init = '1';
        const F = [
            { r: [10, 15, 30, 55, 70, 60, 35, 20], a: '<b>Precaución:</b> tormentas en la tarde. Evite quemas y senderos cercanos a quebradas.' },
            { r: [5, 10, 15, 25, 35, 30, 15, 10], a: '<b>Vigilancia:</b> calor al mediodía. Mantenga atención ante conatos de incendio.' }
        ];
        const al = $('#al'), tabs = $$('.o-dia');
        const dia = n => {
            tabs.forEach((t, k) => { t.classList.toggle('on', k === n); t.setAttribute('aria-selected', k === n); });
            horas.innerHTML = F[n].r.map((v, k) => `<div class="o-h" title="${k * 3}:00, lluvia ${v} %"><i style="height:${Math.max(v, 4)}%"></i><span>${k * 3}h</span></div>`).join('');
            al.innerHTML = F[n].a;
        };
        tabs.forEach((t, k) => t.addEventListener('click', () => dia(k)));
        dia(0);
    }

    function initPlayer() {
        const st = $('#stage'); if (!st || st.dataset.init) return; st.dataset.init = '1';
        const slides = $$('.slide', st), btns = $$('.plist button'), cap = $('#cap'), vt = $('#vt'), prog = $('#prog'), pp = $('#pp');
        let i = 0, p = 0, playing = !reduce; const DUR = 6000;
        const show = n => {
            i = (n + slides.length) % slides.length; p = 0;
            slides.forEach((s, k) => s.classList.toggle('active', k === i));
            btns.forEach((b, k) => { b.classList.toggle('on', k === i); b.setAttribute('aria-pressed', k === i); });
            cap.textContent = slides[i].dataset.c || ''; vt.textContent = slides[i].dataset.t || '';
        };
        const setPP = () => { pp.textContent = playing ? '❚❚' : '▶'; pp.setAttribute('aria-label', playing ? 'Pausar' : 'Reproducir'); };
        btns.forEach((b, k) => b.addEventListener('click', () => show(k)));
        pp.addEventListener('click', () => { playing = !playing; setPP(); });
        let last = performance.now();
        (function tick(now) {
            if (!document.body.contains(st)) return;
            const d = now - last; last = now;
            if (playing) { p += d; if (p >= DUR) show(i + 1); prog.style.width = (p / DUR * 100) + '%'; }
            requestAnimationFrame(tick);
        })(last);
        show(0); setPP();
    }

    function initDenuncia() {
        const form = $('#fi'); if (!form || form.dataset.init) return; form.dataset.init = '1';
        const ok = $('#okd');
        $('#foto')?.addEventListener('change', e => { const f = e.target.files[0]; if (f) { $('#vprev').src = URL.createObjectURL(f); $('#vprev').hidden = false; } });
        $('#gps')?.addEventListener('click', () => {
            const m = $('#gm'); m.textContent = 'Buscando tu ubicación…';
            if (!navigator.geolocation) { m.textContent = 'Tu navegador no permite ubicación. Escríbela a mano.'; return; }
            navigator.geolocation.getCurrentPosition(
                g => { $('#ubi').value = g.coords.latitude.toFixed(5) + ', ' + g.coords.longitude.toFixed(5); m.textContent = 'Ubicación añadida.'; },
                () => { m.textContent = 'No pudimos obtenerla. Escríbela a mano.'; });
        });
        // DEMOSTRACIÓN: aún no se guarda en la base de datos
        form.addEventListener('submit', e => { e.preventDefault(); form.hidden = true; ok.hidden = false; ok.scrollIntoView({ behavior: 'smooth', block: 'center' }); });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init); else init();
    if (window.Blazor?.addEventListener) Blazor.addEventListener('enhancedload', init);
    else {
        let t;
        new MutationObserver(() => { clearTimeout(t); t = setTimeout(init, 150); }).observe(document.body, { childList: true, subtree: true });
    }
})();