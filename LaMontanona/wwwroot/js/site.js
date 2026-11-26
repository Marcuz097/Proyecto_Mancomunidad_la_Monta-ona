/* site.js · Mancomunidad La Montañona
   Funciona con la navegación mejorada de Blazor: los eventos van delegados al
   document y lo que depende de la página se vuelve a iniciar en cada carga. */

(() => {
    const $ = (s, r = document) => r.querySelector(s);
    const $$ = (s, r = document) => [...r.querySelectorAll(s)];
    const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
    const root = document.documentElement;
    const guardar = (k, v) => { try { localStorage.setItem(k, v); } catch { } };

    /* ---------- eventos globales (una sola vez) ---------- */
    document.addEventListener('click', e => {
        // tamaño de letra
        const fs = e.target.closest('[data-fs]');
        if (fs) {
            let n = parseInt(root.dataset.fs || '0', 10) + (fs.dataset.fs === 'up' ? 1 : -1);
            n = Math.max(0, Math.min(2, n));
            n ? root.dataset.fs = n : root.removeAttribute('data-fs');
            guardar('fs', n);
        }
        // modo claro / oscuro
        if (e.target.closest('[data-tema]')) {
            const oscuro = root.dataset.theme === 'dark';
            oscuro ? root.removeAttribute('data-theme') : root.dataset.theme = 'dark';
            guardar('tema', oscuro ? 'light' : 'dark');
            $$('[data-tema]').forEach(b => b.textContent = oscuro ? '☀ Claro' : '☾ Oscuro');
        }
        // menú móvil
        if (e.target.closest('#burger')) {
            const m = $('#menu'); const ab = m.classList.toggle('open');
            $('#burger').setAttribute('aria-expanded', ab);
        } else if (e.target.closest('#menu a')) { $('#menu')?.classList.remove('open'); }
        // ampliar imagen
        const z = e.target.closest('[data-zoom]');
        if (z) {
            const v = $('#visor'); if (!v) return;
            const img = z.matches('img') ? z : $('img', z);
            $('#visorFoto').src = z.dataset.zoom || img?.src || '';
            $('#visorTitulo').textContent = z.dataset.titulo || img?.alt || '';
            v.showModal();
        }
        if (e.target.closest('#visorCerrar')) $('#visor')?.close();
        if (e.target.id === 'visor') e.target.close();
        // pestañas del plan de trabajo
        const tb = e.target.closest('[data-tab]');
        if (tb) {
            $$('[data-tab]').forEach(b => { const on = b === tb; b.classList.toggle('on', on); b.setAttribute('aria-selected', on); });
            $$('[data-panel]').forEach(p => p.hidden = p.dataset.panel !== tb.dataset.tab);
        }
    });
    window.addEventListener('scroll', () => $('#cab')?.classList.toggle('scrolled', scrollY > 8), { passive: true });

    /* ---------- por página (se repite tras cada navegación) ---------- */
    function init() {
        root.classList.add('js');
        const txt = root.dataset.theme === 'dark' ? '☾ Oscuro' : '☀ Claro';
        $$('[data-tema]').forEach(b => { if (b.textContent !== txt) b.textContent = txt; });

        // aparición al hacer scroll
        const rv = $$('.rv:not(.vis)');
        if (reduce || !('IntersectionObserver' in window)) rv.forEach(x => x.classList.add('vis'));
        else {
            const io = new IntersectionObserver(es => es.forEach(en => { if (en.isIntersecting) { en.target.classList.add('vis'); io.unobserve(en.target); } }), { threshold: .12 });
            rv.forEach(x => io.observe(x));
        }

        // contadores
        $$('[data-count]:not([data-prep])').forEach(el => {
            el.dataset.prep = '1';
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

        [initPlayer, initDenuncia, initSeguimiento].forEach(f => { try { f(); } catch (err) { console.error(err); } });
    }

    function initPlayer() {
        const horas = $('#horas'); if (!horas || horas.dataset.init) return; horas.dataset.init = '1';
        const ob = $('#observatorio');
        const p = parseInt(ob?.dataset.prob ?? '', 10);
        const prob = Number.isNaN(p) ? 70 : p;
        const base = [10, 15, 30, 55, 70, 60, 35, 20];
        // "Hoy" usa lo que publica el administrador; "Mañana" es de ejemplo
        const F = [
            { r: base.map(v => Math.round(v * prob / 70)), n: ob?.dataset.nivel || 'Precaución', t: ob?.dataset.msg || 'Tormentas en la tarde. Evite quemas y senderos cercanos a quebradas.' },
            { r: [5, 10, 15, 25, 35, 30, 15, 10], n: 'Vigilancia', t: 'Calor al mediodía. Mantenga atención ante conatos de incendio.' }
        ];
        const al = $('#al'), tabs = $$('.o-dia');
        const dia = n => {
            tabs.forEach((t, k) => { t.classList.toggle('on', k === n); t.setAttribute('aria-selected', k === n); });
            horas.innerHTML = F[n].r.map((v, k) => `<div class="o-h" title="${k * 3}:00, lluvia ${v} %"><i style="height:${Math.max(v, 4)}%"></i><span>${k * 3}h</span></div>`).join('');
            al.textContent = '';
            const b = document.createElement('b'); b.textContent = F[n].n + ': ';
            al.append(b, F[n].t);
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

    // Denuncia REAL: se guarda en la base y devuelve un código de seguimiento
    function initDenuncia() {
        // Los eventos se conectan una sola vez al documento.
        // Así funcionan aunque Blazor renderice el formulario después.
        if (document.documentElement.dataset.denunciaEvents === '1') return;
        document.documentElement.dataset.denunciaEvents = '1';

        // Vista previa de la foto y modo anónimo
        document.addEventListener('change', e => {
            if (e.target.matches('#foto')) {
                const archivo = e.target.files?.[0];
                const vista = document.querySelector('#vprev');
                if (archivo && vista) { vista.src = URL.createObjectURL(archivo); vista.hidden = false; }
            }
            if (e.target.matches('#anon')) {
                const datos = document.querySelector('#datosDen');
                if (datos) datos.hidden = e.target.checked;
            }
        });

        // Botón "Usar mi ubicación"
        document.addEventListener('click', e => {
            const boton = e.target.closest('#gps');
            if (!boton) return;
            e.preventDefault();

            const mensaje = document.querySelector('#gm');
            const ubicacion = document.querySelector('#ubi');
            const latitud = document.querySelector('#lat');
            const longitud = document.querySelector('#lng');
            if (!mensaje || !ubicacion || !latitud || !longitud) return;

            mensaje.textContent = 'Solicitando ubicación…';
            if (!window.isSecureContext) {
                mensaje.textContent = 'La ubicación requiere HTTPS o localhost. Abre el sitio desde http://localhost.';
                return;
            }
            if (!navigator.geolocation) {
                mensaje.textContent = 'Este navegador no permite obtener la ubicación.';
                return;
            }

            boton.disabled = true;
            navigator.geolocation.getCurrentPosition(
                posicion => {
                    const lat = posicion.coords.latitude, lng = posicion.coords.longitude;
                    latitud.value = lat.toFixed(6);
                    longitud.value = lng.toFixed(6);
                    if (!ubicacion.value.trim()) ubicacion.value = `${lat.toFixed(5)}, ${lng.toFixed(5)}`;
                    mensaje.textContent = 'Ubicación añadida correctamente.';
                    boton.disabled = false;
                },
                error => {
                    const mensajes = {
                        1: 'Permiso de ubicación denegado. Revisa los permisos del navegador.',
                        2: 'No se pudo determinar tu ubicación.',
                        3: 'La búsqueda de ubicación tardó demasiado.'
                    };
                    mensaje.textContent = mensajes[error.code] || 'No se pudo obtener la ubicación.';
                    boton.disabled = false;
                },
                { enableHighAccuracy: true, timeout: 20000, maximumAge: 0 }
            );
        });

        // Si la persona cambia la ubicación a mano, se borran las coordenadas del GPS
        document.addEventListener('input', e => {
            if (e.target.matches('#ubi')) {
                const lat = document.querySelector('#lat'), lng = document.querySelector('#lng');
                if (lat) lat.value = '';
                if (lng) lng.value = '';
            }
        });

        // Reconoce coordenadas escritas a mano: "13.98, -89.01"
        function leerCoordenadas(texto) {
            const m = texto.match(/^\s*(-?\d+(?:[.,]\d+)?)\s*[,;]\s*(-?\d+(?:[.,]\d+)?)\s*$/);
            if (!m) return null;
            const lat = Number(m[1].replace(',', '.')), lng = Number(m[2].replace(',', '.'));
            if (!Number.isFinite(lat) || !Number.isFinite(lng)) return null;
            if (lat < -90 || lat > 90 || lng < -180 || lng > 180) return null;
            return { lat, lng };
        }

        // Envío de la denuncia (un solo manejador)
        document.addEventListener('submit', async e => {
            const form = e.target;
            if (!form.matches('#fi')) return;
            e.preventDefault();

            const error = document.querySelector('#errd');
            const boton = document.querySelector('#btnDen');
            const exito = document.querySelector('#okd');
            const ubicacion = document.querySelector('#ubi');
            const lat = document.querySelector('#lat');
            const lng = document.querySelector('#lng');
            if (!error || !boton || !exito || !ubicacion || !lat || !lng) return;

            error.hidden = true;
            error.textContent = '';

            if (!ubicacion.value.trim()) {
                error.textContent = 'Escribe la ubicación de la denuncia.';
                error.hidden = false;
                return;
            }

            const coordenadas = leerCoordenadas(ubicacion.value);
            if (!lat.value && coordenadas) {
                lat.value = String(coordenadas.lat);
                lng.value = String(coordenadas.lng);
            }

            const textoBoton = boton.textContent;
            boton.disabled = true;
            boton.textContent = 'Enviando denuncia…';

            try {
                const respuesta = await fetch('/api/incidentes/', {
                    method: 'POST',
                    body: new FormData(form),
                    headers: { 'Accept': 'application/json' }
                });

                const texto = await respuesta.text();
                let resultado = {};
                try { resultado = texto ? JSON.parse(texto) : {}; }
                catch { throw new Error(`El servidor respondió con un formato inesperado. HTTP ${respuesta.status}.`); }

                if (!respuesta.ok) {
                    throw new Error(resultado.error || resultado.detail || `Error del servidor: ${respuesta.status}`);
                }
                if (!resultado.codigo) {
                    throw new Error('El servidor respondió, pero no devolvió el código de seguimiento.');
                }

                document.querySelector('#codd').textContent = resultado.codigo;
                form.hidden = true;
                exito.hidden = false;
                exito.scrollIntoView({ behavior: 'smooth', block: 'center' });
            } catch (problema) {
                console.error('Error al enviar denuncia:', problema);
                error.textContent = problema.message || 'No se pudo enviar la denuncia.';
                error.hidden = false;
            } finally {
                boton.disabled = false;
                boton.textContent = textoBoton;
            }
        });
    }

    // Consulta pública: "¿cómo va mi denuncia?"
    function initSeguimiento() {
        const f = $('#fseg'); if (!f || f.dataset.init) return; f.dataset.init = '1';
        f.addEventListener('submit', async e => {
            e.preventDefault();
            const c = $('#cseg').value.trim(), out = $('#rseg');
            if (!c) { out.textContent = 'Escribe tu código.'; return; }
            out.textContent = 'Consultando…';
            try {
                const r = await fetch('/api/incidentes/seguimiento/' + encodeURIComponent(c));
                if (r.status === 404) { out.textContent = 'No encontramos ese código.'; return; }
                if (!r.ok) throw new Error();
                const j = await r.json();
                out.textContent = `${j.tipo} · reportada el ${new Date(j.fecha).toLocaleDateString('es-SV')} · Estado: ${j.estado}`;
            } catch { out.textContent = 'No pudimos consultar. Inténtalo de nuevo.'; }
        });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init); else init();
    if (window.Blazor?.addEventListener) Blazor.addEventListener('enhancedload', init);

    // Con <Routes @rendermode="InteractiveServer" /> Blazor cambia de página sin lanzar "enhancedload".
    // Por eso también vigilamos el DOM: cuando cambia la página, se vuelve a iniciar (con una pequeña pausa).
    let pausa;
    new MutationObserver(() => { clearTimeout(pausa); pausa = setTimeout(init, 120); })
        .observe(document.body, { childList: true, subtree: true });
})();