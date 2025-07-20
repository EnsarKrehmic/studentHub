$(document).ready(function () {
    function isMobile() { return $(window).width() <= 992; }

    function openSidebar() {
        $('#sidebar').addClass('active');
        $('.wrapper').addClass('active');
        if (isMobile()) {
            $('#sidebar-overlay').addClass('active');
            $('.navbar').hide(); // Sakrij header na mobile kad je sidebar otvoren
        } else {
            $('#sidebar-overlay').removeClass('active');
            $('.navbar').show(); // Prikaži header na desktopu
        }
    }

    function closeSidebar() {
        $('#sidebar').removeClass('active');
        $('.wrapper').removeClass('active');
        $('#sidebar-overlay').removeClass('active');
        $('.navbar').show(); // Uvijek prikaži header kad sidebar nije aktivan
    }

    function initSidebar() {
        if (isMobile()) {
            closeSidebar();
        } else {
            openSidebar();
        }
    }

    // Init on load & resize
    initSidebar();
    $(window).resize(initSidebar);

    // Toggle button
    $('#sidebarToggle').click(function (e) {
        e.stopPropagation();
        if ($('#sidebar').hasClass('active')) closeSidebar();
        else openSidebar();
    });

    // Click outside sidebar or on overlay closes (mobile)
    $('#sidebar-overlay').click(function () {
        closeSidebar();
    });

    $(document).click(function (e) {
        if (isMobile() && $('#sidebar').hasClass('active')) {
            if (!$(e.target).closest('#sidebar, #sidebarToggle').length) {
                closeSidebar();
            }
        }
    });

    // Prevent closing when clicking inside sidebar
    $('#sidebar').click(function (e) { e.stopPropagation(); });

    // Sidebar dropdown toggles
    $('.dropdown-toggle').click(function () {
        $(this).toggleClass('show');
    });
});
