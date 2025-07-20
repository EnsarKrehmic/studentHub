document.addEventListener("DOMContentLoaded", function () {
    if (document.getElementById('map')) {
        // Inicijalizacija mape
        $(document).ready(function () {
            var map = L.map('map').setView([44.199135, 17.903445], 17);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);
            L.marker([44.199135, 17.903445]).addTo(map)
                .bindPopup('<b>Mašinski fakultet Zenica</b><br>Univerzitet u Zenici')
                .openPopup();

            // Inicijalizacija tooltipova
            $('[data-toggle="tooltip"]').tooltip({
                trigger: 'hover',
                placement: 'auto'
            });
        });
    }
});