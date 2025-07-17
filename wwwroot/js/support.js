$(document).ready(function () {
    $('#supportFab').on('click', function () {
        $('#supportPanel').removeClass('d-none');
    });
    $('#supportPanelClose').on('click', function () {
        $('#supportPanel').addClass('d-none');
    });
    // ESC za zatvaranje panela
    $(document).on('keydown', function (e) {
        if (e.key === "Escape") {
            $('#supportPanel').addClass('d-none');
        }
    });
});
