$("#txtSearch").keyup(function () {
    var txtSearch = $("#txtSearch");
    var panel = $("#cblPupils");
    var countLabel = $('#spnCount');

    if (txtSearch.val() != "") {
        //alert("Here");
        var count = 0;
        panel.children('.checkbox').each(function () {
            var match = false;
            $(this).children('label').each(function () {
                if ($(this).text().toUpperCase().indexOf(txtSearch.val().toUpperCase()) > -1)
                    match = true;
            });
            if (match) {
                $(this).show();
                count++;
            }
            else { $(this).hide(); }
        });
        countLabel.html((count) + ' match');
    }
    else {
        panel.children('.checkbox').each(function () {
            $(this).show();
        });
        countLabel.html('');
    }

   
});

$(document).ready(function () {

    var countLabel = $('#curCount');
    var currentCount = countLabel.data("count");

    $('#cblPupils input:checkbox').change(function () {
        if ($(this).is(':checked')) {
            currentCount += 1;
        } else {
            currentCount -= 1;
        }

        countLabel.html(currentCount + ' Pupil(s) Selected');
        countLabel.data("count", currentCount);
    });
});
