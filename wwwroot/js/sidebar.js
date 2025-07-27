$(document).ready(function () {
    const SIDEBAR_WIDTH = 280;

    function isMobile() {
        return window.innerWidth <= 992;
    }

    function updateLayout() {
        if (isMobile()) {
            $('#sidebar').addClass('active');
            $('#sidebar-overlay').removeClass('active');
            $('#content').css('margin-left', '0');
        } else {
            $('#sidebar').removeClass('active');
            $('#sidebar-overlay').removeClass('active');
            $('#content').css('margin-left', SIDEBAR_WIDTH + 'px');
        }
    }

    updateLayout();

    $(window).on('resize', updateLayout);

    // Sidebar toggle button
    $('#sidebarToggle').click(function (e) {
        e.preventDefault();
        e.stopPropagation();
        if (isMobile()) {
            if ($('#sidebar').hasClass('active')) {
                $('#sidebar').removeClass('active');
                $('#sidebar-overlay').addClass('active');
                $('#content').css('margin-left', '0');
            } else {
                $('#sidebar').addClass('active');
                $('#sidebar-overlay').removeClass('active');
                $('#content').css('margin-left', '0');
            }
        } else {
            // Na desktopu sidebar je uvijek otvoren, toggle ga ne zatvara!
            // Ako želiš mogućnost zatvaranja na desktopu, otkomentariši ispod
            /*
            if ($('#sidebar').hasClass('active')) {
                $('#sidebar').removeClass('active');
                $('#content').css('margin-left', SIDEBAR_WIDTH + 'px');
            } else {
                $('#sidebar').addClass('active');
                $('#content').css('margin-left', '0');
            }
            */
        }
    });

    // Klik na overlay zatvara sidebar na mobile
    $('#sidebar-overlay').click(function () {
        $('#sidebar').addClass('active');
        $(this).removeClass('active');
        $('#content').css('margin-left', '0');
    });

    // Klik van sidebara (na document) zatvara sidebar (na mobile)
    $(document).click(function (e) {
        if (isMobile() && !$(e.target).closest('#sidebar, #sidebarToggle').length) {
            $('#sidebar').addClass('active');
            $('#sidebar-overlay').removeClass('active');
            $('#content').css('margin-left', '0');
        }
    });

    // Onemogući zatvaranje klikom u sidebar
    $('#sidebar').click(function (e) { e.stopPropagation(); });

    // Sidebar dropdown toggles (po potrebi)
    $('.dropdown-toggle').click(function (e) {
        $(this).toggleClass('show');
        // Spriječi zatvaranje dropdowna klikom na parent
        e.stopPropagation();
    });
});
