$(document).ready(function () {
    $('#supportFab').on('click', function () {
        $('#supportPanel').removeClass('d-none');
        setTimeout(() => { $('#chatbotInput').focus(); }, 350);
    });
    $('#supportPanelClose').on('click', function () {
        $('#supportPanel').addClass('d-none');
    });
    $(document).on('keydown', function (e) {
        if (e.key === "Escape") {
            $('#supportPanel').addClass('d-none');
        }
    });
    // Klik izvan panela zatvara (opciono)
    $('#supportPanel').on('click', function (e) {
        if (e.target === this) $('#supportPanel').addClass('d-none');
    });
});
