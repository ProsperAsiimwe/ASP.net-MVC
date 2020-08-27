//$("#CurrentLevelId").change(function () {
    
//    if ($('#CurrentLevelId option:selected').html().indexOf('Nursery') >= 0) {
//        toggleChekboxPanel(true, $('#half-day'));
//    } else {
        
//        toggleChekboxPanel(false, $('#half-day'));
//    }

    
    
//});

//document.ready(function () {

//    toggleRadio('#SchoolLevel_0');
//    toggleRadio('#SchoolLevel_1');
//    toggleRadio('#SchoolLevel_2');

//});
//function toggleRadio(radio) {

//    $(radio).change(function () {
//        var isChecked = $(radio).prop('checked');

//        if (isChecked) {
//            toggleChekboxPanel(true, $('#half-day'));
//        }
        
//        if(!checked && radio == '#SchoolLevel_0'){
//            toggleChekboxPanel(false, $('#half-day'));
//        }
//    });
//}
$('#SchoolLevel_0').change(function () {
    var isChecked = $('#SchoolLevel_0').prop('checked');

    if (isChecked) {
        toggleChekboxPanel(true, $('#half-day'));
    } else {
        toggleChekboxPanel(false, $('#half-day'));
    }
});
$('#SchoolLevel_1').change(function () {
    var isChecked = $('#SchoolLevel_1').prop('checked');

    if (isChecked) {
        toggleChekboxPanel(false, $('#half-day'));
    } 
});
$('#SchoolLevel_2').change(function () {
    var isChecked = $('#SchoolLevel_2').prop('checked');

    if (isChecked) {
        toggleChekboxPanel(false, $('#half-day'));
    } 
});

function toggleChekboxPanel(show, panel) {

    if (show == true) {
        panel.removeClass('hide');
        panel.show(1000);
    } else {
        panel.addClass('hide');
        panel.hide(500);
    }
}