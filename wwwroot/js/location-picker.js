// ============ REUSABLE LOCATION PICKER ============
// Usage: var picker = initLocationPicker({ mapId, latInputId, lngInputId, addressInputId, coordsDisplayId });
//
// Options:
//   mapId           – the id of the map container div
//   latInputId      – hidden input for latitude
//   lngInputId      – hidden input for longitude
//   addressInputId  – (optional) text input to fill with reverse-geocoded address
//   coordsDisplayId – small text element showing "Marked: lat, lng"
//   initialLat      – default centre latitude  (default 33.6844 — Islamabad)
//   initialLng      – default centre longitude (default 73.0479)
//   initialZoom     – default zoom level (default 12)

function initLocationPicker(opts) {
    var mapId           = opts.mapId;
    var latInputId      = opts.latInputId;
    var lngInputId      = opts.lngInputId;
    var addressInputId  = opts.addressInputId || null;
    var coordsDisplayId = opts.coordsDisplayId || (mapId + '-coords');
    var initialLat      = opts.initialLat  || 33.6844;
    var initialLng      = opts.initialLng  || 73.0479;
    var initialZoom     = opts.initialZoom || 12;

    var map    = null;
    var marker = null;
    var _reverseTimer = null;

    // ── Core: create the Leaflet map ──────────────────────────────────
    function createMap() {
        if (map) return map;
        var el = document.getElementById(mapId);
        if (!el) return null;

        map = L.map(mapId).setView([initialLat, initialLng], initialZoom);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        // Restore existing marker from hidden inputs (e.g. post-back)
        var eLat = parseFloat(document.getElementById(latInputId).value);
        var eLng = parseFloat(document.getElementById(lngInputId).value);
        if (!isNaN(eLat) && !isNaN(eLng) && (eLat !== 0 || eLng !== 0)) {
            placeMarker(eLat, eLng, false);
            map.setView([eLat, eLng], initialZoom);
        }

        // Click anywhere → place / move marker
        map.on('click', function (e) {
            placeMarker(e.latlng.lat, e.latlng.lng, true);
        });

        // Wire up the geolocation button (rendered by the partial)
        var geoBtn = document.getElementById(mapId + '-geolocate');
        if (geoBtn) geoBtn.addEventListener('click', handleGeolocation);

        return map;
    }

    // ── Place / move marker ───────────────────────────────────────────
    function placeMarker(lat, lng, doReverse) {
        var latlng = L.latLng(lat, lng);

        document.getElementById(latInputId).value = lat.toFixed(6);
        document.getElementById(lngInputId).value = lng.toFixed(6);

        var coordsEl = document.getElementById(coordsDisplayId);
        if (coordsEl) coordsEl.textContent = 'Marked: ' + lat.toFixed(6) + ', ' + lng.toFixed(6);

        if (marker) {
            marker.setLatLng(latlng);
        } else {
            marker = L.marker(latlng, { draggable: true }).addTo(map);
            marker.on('dragend', function (ev) {
                var pos = ev.target.getLatLng();
                placeMarker(pos.lat, pos.lng, true);
            });
        }

        if (doReverse && addressInputId) {
            debouncedReverse(lat, lng);
        }
    }

    // ── Reverse geocoding (Nominatim, debounced 500 ms) ───────────────
    function debouncedReverse(lat, lng) {
        if (_reverseTimer) clearTimeout(_reverseTimer);
        _reverseTimer = setTimeout(function () { reverseGeocode(lat, lng); }, 500);
    }

    function reverseGeocode(lat, lng) {
        var input = addressInputId ? document.getElementById(addressInputId) : null;
        if (!input) return;

        var url = 'https://nominatim.openstreetmap.org/reverse?lat=' + lat +
                  '&lon=' + lng + '&format=json&addressdetails=1';

        fetch(url, { headers: { 'User-Agent': 'SentinelPulse/1.0' } })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data && data.display_name) {
                    input.value = data.display_name;
                }
            })
            .catch(function () { /* silently ignore geocode failures */ });
    }

    // ── Geolocation button handler ────────────────────────────────────
    function handleGeolocation() {
        var statusEl = document.getElementById(mapId + '-geo-status');

        if (!navigator.geolocation) {
            showGeoStatus(statusEl, "Geolocation not supported — tap the map to set location.");
            return;
        }

        if (statusEl) {
            statusEl.textContent = 'Getting location…';
            statusEl.style.color = 'var(--muted, #888)';
        }

        navigator.geolocation.getCurrentPosition(
            function (pos) {
                if (statusEl) statusEl.textContent = '';
                var lat = pos.coords.latitude;
                var lng = pos.coords.longitude;
                if (map) map.setView([lat, lng], 15);
                placeMarker(lat, lng, true);
            },
            function () {
                showGeoStatus(statusEl, "Couldn't get location — tap the map to set it manually.");
            },
            { enableHighAccuracy: true, timeout: 10000 }
        );
    }

    function showGeoStatus(el, msg) {
        if (!el) return;
        el.textContent = msg;
        el.style.color = 'var(--danger, #d6241a)';
    }

    // ── Visibility handling (wizard steps, tabs, etc.) ─────────────────
    function ensureMapVisible() {
        var el = document.getElementById(mapId);
        if (!el) return;

        // Already visible → init immediately
        if (el.offsetParent !== null || el.offsetWidth > 0) {
            if (!map) createMap();
            else setTimeout(function () { map.invalidateSize(); }, 150);
            return;
        }

        // Hidden → observe ancestors for class / style changes
        var observer = new MutationObserver(function () {
            if (el.offsetParent !== null || el.offsetWidth > 0) {
                if (!map) createMap();
                setTimeout(function () { if (map) map.invalidateSize(); }, 250);
                observer.disconnect();
            }
        });

        // Walk up and observe every ancestor
        var node = el.parentElement;
        while (node && node !== document.body) {
            observer.observe(node, { attributes: true, attributeFilter: ['class', 'style'] });
            node = node.parentElement;
        }
    }

    // Kick off
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', ensureMapVisible);
    } else {
        ensureMapVisible();
    }

    // ── Public API ────────────────────────────────────────────────────
    return {
        init: createMap,
        invalidateSize: function () { if (map) map.invalidateSize(); },
        getMap: function () { return map; }
    };
}
