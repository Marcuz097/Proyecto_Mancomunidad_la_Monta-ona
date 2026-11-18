(() => {
    const $ = s => document.querySelector(s), $$ = s => [...document.querySelectorAll(s)];
    const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (!document.querySelector('#rio')) return; // solo en la portada

    // 1. Reproductor Somos Río Lempa: lista que rota sola, con pausa y selección manual
    const vids = [
        { t: 'Reforestación en la cuenca', c: 'Voluntarios plantan especies nativas en la ladera.', f: 'none' },
        { t: 'Sistemas agropastoriles', c: 'Ganadería y cultivos que cuidan el suelo y suman ingresos.', f: 'hue-rotate(35deg) saturate(1.2)' },
        { t: 'Agua para las comunidades', c: 'Protegemos las fuentes de la cuenca del Lempa.', f: 'hue-rotate(-25deg) brightness(1.1)' }];
    let i = 0, playing = !reduce, p = 0; const DUR = 8000;
    const svg = $('.screen svg'), cap = $('#cap'), vt = $('#vt'), prog = $('#prog'), pp = $('#pp'), btns = $$('.list button');
    function show(n) {
        i = (n + vids.length) % vids.length; p = 0; svg.style.filter = vids[i].f; cap.textContent = vids[i].c; vt.textContent = vids[i].t;
        btns.forEach((b, k) => { b.classList.toggle('on', k === i); b.setAttribute('aria-pressed', k === i) })
    }
    function setPP() { pp.textContent = playing ? '❚❚' : '▶'; pp.setAttribute('aria-label', playing ? 'Pausar' : 'Reproducir'); $('.badge').style.opacity = playing ? 1 : .4 }
    btns.forEach((b, k) => b.addEventListener('click', () => show(k)));
    pp.addEventListener('click', () => { playing = !playing; setPP() });
    let last = performance.now();
    (function tick(now) { const d = now - last; last = now; if (playing) { p += d; if (p >= DUR) show(i + 1); prog.style.width = (p / DUR * 100) + '%' } requestAnimationFrame(tick) })(last);
    show(0); setPP();

    // 2. Observatorio: Hoy / Mañana con barras de lluvia por franja de 3 h
    const F = [{ r: [10, 15, 30, 55, 70, 60, 35, 20], a: '<b>Precaución:</b> tormentas en la tarde. Evite quemas y senderos cercanos a quebradas.' },
    { r: [5, 10, 15, 25, 35, 30, 15, 10], a: '<b>Vigilancia:</b> calor al mediodía. Mantenga atención ante conatos de incendio.' }];
    const horas = $('#horas'), al = $('#al'), tabs = $$('.dia');
    function dia(n) {
        tabs.forEach((t, k) => { t.classList.toggle('on', k === n); t.setAttribute('aria-selected', k === n) });
        horas.innerHTML = F[n].r.map((v, k) => `<div class="h" title="${k * 3}:00, lluvia ${v} %"><i style="height:${v}%"></i><span>${k * 3}h</span></div>`).join(''); al.innerHTML = F[n].a
    }
    tabs.forEach((t, k) => t.addEventListener('click', () => dia(k))); dia(0);

    // 3. Programas: tocar para ver el detalle
    const MAS = ['Patrullajes, brigadas y monitoreo de especies. Los reportes de guardabosques alimentan este programa.',
        'Sistemas agropastoriles y restauración de suelos. Las familias participantes se registran con su DUI.',
        'Mesas del agua con juntas y comunidades de la cuenca.', 'Planificación, transparencia y formación de equipos técnicos.', 'Rutas, guías locales y emprendimientos comunitarios.'];
    $$('.eje').forEach((e, k) => {
        const m = document.createElement('p'); m.className = 'mas'; m.textContent = MAS[k]; e.append(m);
        e.tabIndex = 0; e.setAttribute('role', 'button'); e.setAttribute('aria-expanded', 'false');
        const t = () => e.setAttribute('aria-expanded', e.classList.toggle('open'));
        e.addEventListener('click', t); e.addEventListener('keydown', ev => { if (ev.key === 'Enter' || ev.key === ' ') { ev.preventDefault(); t() } })
    });

    // 4. Formulario de incidente en ventana, con vista previa de foto y ubicación del móvil
    const dlg = $('#dlg'), form = $('#fi'), ok = $('#ok');
    $$('[data-open]').forEach(a => a.addEventListener('click', e => { e.preventDefault(); ok.hidden = true; form.hidden = false; dlg.showModal() }));
    $('#cerrar').addEventListener('click', () => dlg.close());
    dlg.addEventListener('click', e => { if (e.target === dlg) dlg.close() });
    $('#foto').addEventListener('change', e => { const f = e.target.files[0]; if (f) { $('#pv').src = URL.createObjectURL(f); $('#pv').hidden = false } });
    $('#gps').addEventListener('click', () => {
        const m = $('#gm'); m.textContent = 'Buscando tu ubicación…';
        if (!navigator.geolocation) { m.textContent = 'Tu navegador no permite ubicación. Escríbela a mano.'; return }
        navigator.geolocation.getCurrentPosition(g => { $('#ubi').value = g.coords.latitude.toFixed(5) + ', ' + g.coords.longitude.toFixed(5); m.textContent = 'Ubicación añadida.' },
            () => { m.textContent = 'No pudimos obtenerla. Escríbela a mano.' })
    });
    form.addEventListener('submit', e => { e.preventDefault(); form.hidden = true; ok.hidden = false });
})();
