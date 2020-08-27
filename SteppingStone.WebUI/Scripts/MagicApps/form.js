function parseJsonSelect(json, targetField, optionLabel) {
    $select = $(targetField);

    // Clear the previous options
    $($select).empty();

    $('<option>', {
        value: ""
    }).html("Select " + optionLabel + " [" + json.length + "]").appendTo($select);

    $.each(json, function (i, item) {
        $('<option>', {
            value: item.id
        }).html(item.name).appendTo($select);
    });
}

function toggleCheckbox(checkbox, panel, showOnTrue) {
    var which = $(checkbox).prop("checked");
    togglePanelByValue(panel, which, showOnTrue);

    $(checkbox).click(function () {
        var which = $(this).prop("checked");
        togglePanelByValue(panel, which, showOnTrue);
    });
}

function toggleDropdown(dropdown, panel, showValue) {
    var v = $(dropdown).val();
    togglePanelByValue(panel, v, showValue);

    $(dropdown).change(function () {
        var v = $(this).val();
        togglePanelByValue(panel, v, showValue);
    });
}

function toggleButtonGroup(radioCss, buttonCss, panel, showValue) {
    var radio = $(radioCss + ":checked");
    var v = radio.val();
    togglePanelByValue(panel, v, showValue);

    if (v != null) {
        radio.parent().addClass("active");
    }

    $(buttonCss).click(function () {
        var v = $(this).find(radioCss).val();
        togglePanelByValue(panel, v, showValue);
    });
}

function togglePanelByValue(panel, inputValue, showValue) {
    if (inputValue == showValue) {
        $(panel).show(700);
    }
    else {
        $(panel).hide(700);
    }
}

function togglePanel(panel, which) {
    if (which) {
        $(panel).show(700);
    }
    else {
        $(panel).hide(700);
    }
}