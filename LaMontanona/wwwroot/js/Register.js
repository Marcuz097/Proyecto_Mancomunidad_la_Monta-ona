(() => {
    const clave = document.getElementById('Input_Password');
    const conf = document.getElementById('Input_ConfirmPassword');
    const medidor = document.getElementById('medidor');
    const reglas = [...document.querySelectorAll('#reglas li')];
    const coincide = document.getElementById('coincide');
    const form = document.getElementById('formRegistro');
    const btn = document.getElementById('btnRegistro');

    // Mostrar / ocultar contraseña (cada ojo controla su campo)
    document.querySelectorAll('[data-ver]').forEach(b => b.addEventListener('click', () => {
        const campo = document.getElementById(b.dataset.ver);
        const mostrar = campo.type === 'password';
        campo.type = mostrar ? 'text' : 'password';
        b.setAttribute('aria-pressed', mostrar);
        b.setAttribute('aria-label', mostrar ? 'Ocultar contraseña' : 'Mostrar contraseña');
    }));

    // Fortaleza de la contraseña
    const prueba = {
        len: v => v.length >= 8,
        up: v => /[A-Z]/.test(v),
        low: v => /[a-z]/.test(v),
        num: v => /\d/.test(v),
        sym: v => /[^A-Za-z0-9]/.test(v)
    };
    const colores = ['#c9791a', '#c9791a', '#d9b43a', '#8fd05a', '#9ad121'];

    function evaluar() {
        const v = clave.value; let ok = 0;
        reglas.forEach(li => { const c = prueba[li.dataset.r](v); li.classList.toggle('ok', c); if (c) ok++; });
        medidor.style.width = (v ? Math.max(ok, 1) / 5 * 100 : 0) + '%';
        medidor.style.background = colores[Math.max(ok - 1, 0)];
        revisarCoincide();
    }

    function revisarCoincide() {
        if (!conf.value) { coincide.hidden = true; return; }
        const igual = conf.value === clave.value;
        coincide.hidden = false;
        coincide.textContent = igual ? '✓ Las contraseñas coinciden' : 'Las contraseñas aún no coinciden';
        coincide.classList.toggle('ok', igual);
    }

    clave?.addEventListener('input', evaluar);
    conf?.addEventListener('input', revisarCoincide);
    evaluar();

    // Evita doble clic y muestra estado de carga
    form?.addEventListener('submit', () => {
        btn.disabled = true;
        btn.querySelector('span').textContent = 'Creando tu cuenta…';
    });
})();
