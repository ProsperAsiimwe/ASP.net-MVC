$(function () {
    
    $("#new-expense").click(function () {
        showModal(0);
    });

    $(".edit-expense").click(function () {
        showModal($(this).data("expenseid"));
    });   

});

function showModal(ExpenseId) {
    hideMainError();

    var url;
        
    if (ExpenseId > 0) {
        url = "/Expenses/" + ExpenseId + "/Edit";
    }
    else {
        url = "/Expenses/New";
    }

    $.get(url, function (html) {
        $("#action-modal-content").html(html);        
    });

    $("#action-modal .modal-title").text("Expense");
    $("#action-modal").modal("show");
}

function hideMainError() {
    var panel = $("#alert_main");

    if (panel.length > 0) {
        panel.hide(500);
    }
}