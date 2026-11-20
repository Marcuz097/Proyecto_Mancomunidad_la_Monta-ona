// Sube la foto por HTTP (fuera de la conexión de Blazor) y avisa al componente con la ruta resultante.
window.fotos = {
  init(inputId, previewId, tipo, dotnet) {
    const input = document.getElementById(inputId);
    if (!input || input.dataset.listo) return;
    input.dataset.listo = '1';
    input.addEventListener('change', async () => {
      const box = document.getElementById(previewId);
      if (box) box.innerHTML = '';
      const f = input.files && input.files[0];
      try {
        if (!f) { await dotnet.invokeMethodAsync('FotoCambio', null, null); return; }   // canceló el selector
        if (f.size > 5 * 1024 * 1024) { input.value = ''; await dotnet.invokeMethodAsync('FotoCambio', null, 'La foto supera los 5 MB.'); return; }
        if (box) {
          const img = document.createElement('img');
          img.className = 'preview'; img.alt = 'Vista previa de la foto'; img.src = URL.createObjectURL(f);
          box.appendChild(img);
        }
        await dotnet.invokeMethodAsync('FotoSubiendo');
        const fd = new FormData(); fd.append('foto', f);
        const r = await fetch('/api/fotos/' + tipo, { method: 'POST', body: fd, credentials: 'same-origin' });
        const j = await r.json().catch(() => null);
        if (!r.ok || !j || !j.ruta) throw new Error((j && j.error) || 'No se pudo subir la foto.');
        await dotnet.invokeMethodAsync('FotoCambio', j.ruta, null);
      } catch (e) {
        if (box) box.innerHTML = '';
        input.value = '';
        try { await dotnet.invokeMethodAsync('FotoCambio', null, (e && e.message) || 'No se pudo subir la foto.'); } catch {}
      }
    });
  },
  reset(inputId, previewId) {
    const i = document.getElementById(inputId); if (i) i.value = '';
    const b = document.getElementById(previewId); if (b) b.innerHTML = '';
  }
};
