
window.mapaIncidentes = (function () {
    "use strict";

    let mapa = null;
    let marcadores = [];
    let incidentes = [];
    let filtroEstado = "";
    let esAdmin = false;

    const estados = [
        "Recibido",
        "En revisión",
        "Atendido",
        "Cerrado"
    ];

    const colores = [
        "#d99a24",
        "#2785bd",
        "#299b62",
        "#78858b"
    ];

    function iniciar() {
        const contenedor = document.getElementById("mapa");

        if (!contenedor) {
            console.error("No se encontró el elemento #mapa");
            return;
        }

        if (typeof L === "undefined") {
            contenedor.innerHTML =
                "Error: Leaflet no se ha cargado. Comprueba tu conexión y App.razor.";
            console.error("Leaflet (L) no está disponible.");
            return;
        }

        if (mapa) {
            mapa.remove();
            mapa = null;
        }

        esAdmin = contenedor.dataset.admin === "true";

        contenedor.innerHTML = "";

        mapa = L.map(contenedor, {
            scrollWheelZoom: true
        }).setView([14.15, -89.10], 10);

        L.tileLayer(
            "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            {
                maxZoom: 19,
                attribution:
                    '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            }
        ).addTo(mapa);

        // Leaflet necesita conocer las dimensiones reales
        setTimeout(function () {
            if (mapa) {
                mapa.invalidateSize();
            }
        }, 300);

        configurarFiltros();
        cargarIncidentes();
    }

    function configurarFiltros() {
        const selector = document.getElementById("fEstado");

        if (selector) {
            selector.onchange = function () {
                filtroEstado = selector.value;
                actualizarVista();
            };
        }
    }

    async function cargarIncidentes() {
        const lista = document.getElementById("lista");

        if (lista) {
            lista.textContent = "Cargando incidentes...";
        }

        try {
            const respuesta = await fetch("/api/incidentes/", {
                method: "GET",
                credentials: "same-origin",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!respuesta.ok) {
                throw new Error(
                    "Error HTTP " + respuesta.status +
                    ". Comprueba que has iniciado sesión."
                );
            }

            const datos = await respuesta.json();

            incidentes = Array.isArray(datos) ? datos : [];

            actualizarVista();

        } catch (error) {
            console.error("Error al cargar incidentes:", error);

            if (lista) {
                lista.textContent =
                    "No se pudieron cargar los incidentes: " +
                    error.message;
            }

            const resumen = document.getElementById("resumen");

            if (resumen) {
                resumen.textContent = "Error al consultar incidentes";
            }
        }
    }

    function obtenerFiltrados() {
        if (filtroEstado === "") {
            return incidentes;
        }

        return incidentes.filter(function (i) {
            return String(i.estado) === filtroEstado;
        });
    }

    function actualizarVista() {
        if (!mapa) return;

        const datos = obtenerFiltrados();

        dibujarMarcadores(datos);
        dibujarLista(datos);
        dibujarFiltros();
        actualizarResumen(datos);
    }

    function dibujarMarcadores(datos) {
        marcadores.forEach(function (m) {
            mapa.removeLayer(m);
        });

        marcadores = [];

        datos.forEach(function (i) {
            if (i.latitud == null || i.longitud == null) {
                return;
            }

            const lat = Number(i.latitud);
            const lng = Number(i.longitud);

            if (!Number.isFinite(lat) ||
                !Number.isFinite(lng) ||
                lat < -90 || lat > 90 ||
                lng < -180 || lng > 180) {
                return;
            }

            const estado = Number(i.estado);
            const color = colores[estado] || colores[0];

            const marcador = L.circleMarker([lat, lng], {
                radius: 9,
                color: color,
                fillColor: color,
                fillOpacity: 0.85,
                weight: 2
            }).addTo(mapa);

            const popup = document.createElement("div");

            const titulo = document.createElement("strong");
            titulo.textContent = i.codigo || "Incidente";

            const tipo = document.createElement("p");
            tipo.textContent = i.tipo || "";

            const ubicacion = document.createElement("p");
            ubicacion.textContent = i.ubicacion || "";

            const estadoTexto = document.createElement("p");
            estadoTexto.textContent =
                "Estado: " + (i.estadoTexto || "Sin estado");

            popup.appendChild(titulo);
            popup.appendChild(tipo);
            popup.appendChild(ubicacion);
            popup.appendChild(estadoTexto);

            if (i.foto) {
                const imagen = document.createElement("img");

                imagen.src = i.foto;
                imagen.alt = "Fotografía del incidente";
                imagen.style.maxWidth = "180px";

                popup.appendChild(imagen);
            }

            marcador.bindPopup(popup);

            marcadores.push(marcador);
        });

        if (marcadores.length > 0) {
            const grupo = L.featureGroup(marcadores);
            mapa.fitBounds(grupo.getBounds(), {
                padding: [30, 30],
                maxZoom: 15
            });
        }
    }

    function dibujarLista(datos) {
        const lista = document.getElementById("lista");

        if (!lista) return;

        lista.replaceChildren();

        if (datos.length === 0) {
            const mensaje = document.createElement("p");
            mensaje.textContent =
                "No hay incidentes registrados para este filtro.";

            lista.appendChild(mensaje);
            return;
        }

        datos.forEach(function (i) {
            const tarjeta = document.createElement("article");
            tarjeta.className = "mi";

            const codigo = document.createElement("strong");
            codigo.textContent = i.codigo || "Incidente";

            const tipo = document.createElement("p");
            tipo.textContent = i.tipo || "";

            const ubicacion = document.createElement("p");
            ubicacion.textContent = i.ubicacion || "";

            const estado = document.createElement("span");
            estado.className = "be";
            estado.textContent = i.estadoTexto || "Sin estado";

            estado.style.backgroundColor =
                colores[Number(i.estado)] || colores[0];

            tarjeta.appendChild(codigo);
            tarjeta.appendChild(estado);
            tarjeta.appendChild(tipo);
            tarjeta.appendChild(ubicacion);

            if (i.descripcion) {
                const descripcion = document.createElement("p");
                descripcion.textContent = i.descripcion;
                tarjeta.appendChild(descripcion);
            }

            if (i.latitud != null && i.longitud != null) {
                tarjeta.style.cursor = "pointer";

                tarjeta.addEventListener("click", function () {
                    mapa.setView(
                        [Number(i.latitud), Number(i.longitud)],
                        16
                    );
                });
            }

            if (esAdmin) {
                const selector = document.createElement("select");

                selector.setAttribute(
                    "aria-label",
                    "Cambiar estado del incidente"
                );

                estados.forEach(function (nombre, indice) {
                    const opcion = document.createElement("option");
                    opcion.value = String(indice);
                    opcion.textContent = nombre;
                    opcion.selected = Number(i.estado) === indice;

                    selector.appendChild(opcion);
                });

                selector.addEventListener("change", function () {
                    cambiarEstado(i.id, Number(selector.value), selector);
                });

                tarjeta.appendChild(selector);
            }

            lista.appendChild(tarjeta);
        });
    }

    function dibujarFiltros() {
        const contenedor = document.getElementById("chips");

        if (!contenedor) return;

        contenedor.replaceChildren();

        const opciones = [
            ["", "Todos"],
            ["0", "Recibidos"],
            ["1", "En revisión"],
            ["2", "Atendidos"],
            ["3", "Cerrados"]
        ];

        opciones.forEach(function (opcion) {
            const boton = document.createElement("button");

            boton.type = "button";
            boton.textContent = opcion[1];
            boton.className =
                filtroEstado === opcion[0] ? "on" : "";

            boton.addEventListener("click", function () {
                filtroEstado = opcion[0];

                const selector = document.getElementById("fEstado");

                if (selector) {
                    selector.value = filtroEstado;
                }

                actualizarVista();
            });

            contenedor.appendChild(boton);
        });
    }

    function actualizarResumen(datos) {
        const resumen = document.getElementById("resumen");

        if (!resumen) return;

        const ubicados = datos.filter(function (i) {
            return i.latitud != null &&
                i.longitud != null &&
                Number.isFinite(Number(i.latitud)) &&
                Number.isFinite(Number(i.longitud));
        }).length;

        resumen.textContent =
            datos.length + " incidentes · " +
            ubicados + " con coordenadas";
    }

    async function cambiarEstado(id, estado, selector) {
        selector.disabled = true;

        try {
            const respuesta = await fetch(
                "/api/incidentes/" + id + "/estado",
                {
                    method: "PUT",
                    credentials: "same-origin",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept": "application/json"
                    },
                    body: JSON.stringify({
                        estado: estado
                    })
                }
            );

            if (!respuesta.ok) {
                throw new Error("No se pudo actualizar el incidente.");
            }

            const incidente = incidentes.find(function (i) {
                return i.id === id;
            });

            if (incidente) {
                incidente.estado = estado;
                incidente.estadoTexto = estados[estado];
            }

            actualizarVista();

        } catch (error) {
            console.error(error);

            alert(error.message);
            selector.value = String(
                incidentes.find(function (i) {
                    return i.id === id;
                }).estado
            );

        } finally {
            selector.disabled = false;
        }
    }

    return {
        iniciar: iniciar,
        recargar: cargarIncidentes
    };

})();
