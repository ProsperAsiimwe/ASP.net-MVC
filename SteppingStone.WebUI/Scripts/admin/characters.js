$("#Description").keyup(function () {
    //alert("Hey");
    CheckCharCounter($("#Description"), 160);
});

function CheckCharCounter(textBox, maxLength) {
   
    var charCount = maxLength - textBox.val().length;
    //alert("Hey");
    // alert("Hey " + charCount);
    $("#lblCharCount").html((charCount) +' Characters left');

    if (charCount >= 0) {        
        document.getElementById("lblCharCount").style.color = "black";        
    }
    else {
        //negative chars - display in red
        document.getElementById("lblCharCount").style.color = "red";
    }

    
}