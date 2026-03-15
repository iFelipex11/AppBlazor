window.homeMap = (() => {
    let map = null;
    let markers = [];
    let markerById = {};

    function clearMarkers() {
        if (!map) return;

        markers.forEach(m => map.removeLayer(m));
        markers = [];
        markerById = {};
    }

    function destroyMap() {
        if (map) {
            clearMarkers();
            map.remove();
            map = null;
        }
    }

    function initMap(elementId, estudios) {
        const mapDiv = document.getElementById(elementId);
        if (!mapDiv) return;

        destroyMap();

        map = L.map(elementId);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        const bounds = [];

        estudios.forEach(estudio => {
            const marker = L.marker([estudio.lat, estudio.lng])
                .addTo(map)
                .bindPopup(`<strong>${estudio.nombre}</strong><br>${estudio.descripcion ?? ''}`);

            markers.push(marker);
            markerById[estudio.id] = marker;
            bounds.push([estudio.lat, estudio.lng]);
        });

        if (bounds.length > 0) {
            map.fitBounds(bounds, { padding: [30, 30] });
        } else {
            map.setView([4.7110, -74.0721], 11);
        }

        setTimeout(() => {
            if (map) {
                map.invalidateSize();
            }
        }, 200);
    }

    function focusStudy(id, lat, lng) {
        if (!map) return;

        map.setView([lat, lng], 17, { animate: true });

        const marker = markerById[id];
        if (marker) {
            marker.openPopup();
        }
    }

    return {
        initMap,
        focusStudy,
        destroyMap
    };
})();