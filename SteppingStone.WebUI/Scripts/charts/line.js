$(function () { })

$(document).on("click", "#btn-activity", function () {
    var button = $(this);
    var startDate = $("#activity-chart #StartDate").val();
    var endDate = $("#activity-chart #EndDate").val();
    var adminId = $("#activity-chart").data("adminId");

    var url = "/Charts/ActivityAsLine";

    $.ajax({
        type: "POST",
        url: url,
        data: { startDate: startDate, endDate: endDate, adminId: adminId }
    }).done(function (html) {
            $("#panel-activity").hide(300).html(html).show(500);
            loadChart();
    });
});
