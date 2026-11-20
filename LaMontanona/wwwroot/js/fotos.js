
// Sube la foto por HTTP (fuera de la conexión de Blazor)
// y avisa al componente con la ruta resultante.
window.fotos = {
    init(inputId, previewId, tipo, dotnet) {
        const input = document.getElementById(inputId);

        if (!input || input.dataset.listo) return;

        input.dataset.listo = '1';

        let previewUrl = null;

        const limpiarPreview = () => {
            const box = document.getElementById(previewId);

            if (previewUrl) {
                URL.revokeObjectURL(previewUrl);
                previewUrl = null;
            }

            if (box) box.innerHTML = '';
        };

        // Cuando se selecciona un archivo
        input.addEventListener('change', async () => {
            limpiarPreview();

            const f = input.files && input.files[0];

            try {
                if (!f) {
                    await dotnet.invokeMethodAsync(
                        'FotoCambio', null, null
                    );
                    return;
                }

                if (f.size > 5 * 1024 * 1024) {
                    input.value = '';

                    await dotnet.invokeMethodAsync(
                        'FotoCambio', null,
                        'La foto supera los 5 MB.'
                    );
                    return;
                }

                const box = document.getElementById(previewId);

                if (box) {
                    const img = document.createElement('img');
                    img.className = 'preview';
                    img.alt = 'Vista previa de la foto';

                    previewUrl = URL.createObjectURL(f);
                    img.src = previewUrl;

                    box.appendChild(img);
                }

                await dotnet.invokeMethodAsync('FotoSubiendo');

                const fd = new FormData();
                fd.append('foto', f);

                const r = await fetch('/api/fotos/' + tipo, {
                    method: 'POST',
                    body: fd,
                    credentials: 'same-origin'
                });

                const j = await r.json().catch(() => null);

                if (!r.ok || !j || !j.ruta) {
                    throw new Error(
                        (j && j.error) || 'No se pudo subir la foto.'
                    );
                }

                await dotnet.invokeMethodAsync(
                    'FotoCambio', j.ruta, null
                );

            } catch (e) {
                limpiarPreview();
                input.value = '';

                try {
                    await dotnet.invokeMethodAsync(
                        'FotoCambio',
                        null,
                        (e && e.message) || 'No se pudo subir la foto.'
                    );
                } catch (error) {
                    console.warn('Blazor no está disponible:', error);
                }
            }
        });

        // Cuando se cancela el selector de archivos
        input.addEventListener('cancel', async () => {
            limpiarPreview();

            input.value = '';

            try {
                await dotnet.invokeMethodAsync(
                    'FotoCambio', null, null
                );
            } catch (e) {
                console.warn(
                    'No se pudo actualizar el componente:', e
                );
            }
        });
    },

    // Limpiar el selector y la vista previa
    reset(inputId, previewId) {
        const i = document.getElementById(inputId);

        if (i) i.value = '';

        const b = document.getElementById(previewId);

        if (b) b.innerHTML = '';
    }
};
