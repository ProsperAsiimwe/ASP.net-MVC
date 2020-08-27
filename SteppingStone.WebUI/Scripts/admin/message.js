$(function () {
    var panel = $('#schedule');
    var checkbox = $('#SendNow');
    toggleChekboxPanel(checkbox, panel);
});

function toggleChekboxPanel(checkbox, panel) {

    checkbox.change(function () {
        if (checkbox.is(':checked')) {            
            panel.hide(500);
            panel.addClass('hide');
        } else {
            panel.removeClass('hide');
            panel.show(1000);
        }
    });
}