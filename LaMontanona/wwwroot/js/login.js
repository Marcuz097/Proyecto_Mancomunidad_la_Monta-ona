(() => {
    const clave = document.getElementById('Input_Password');
    const ver = document.getElementById('verClave');
    const form = document.querySelector('.login-form');
    const btn = document.getElementById('btnIngresar');

    // Mostrar / ocultar contraseña
    ver?.addEventListener('click', () => {
        const mostrar = clave.type === 'password';
        clave.type = mostrar ? 'text' : 'password';
        ver.setAttribute('aria-pressed', mostrar);
        ver.setAttribute('aria-label', mostrar ? 'Ocultar contraseña' : 'Mostrar contraseña');
    });

    // Evita doble clic y muestra estado de carga
    form?.addEventListener('submit', () => {
        btn.disabled = true;
        btn.querySelector('span').textContent = 'Verificando…';
    });
})();
