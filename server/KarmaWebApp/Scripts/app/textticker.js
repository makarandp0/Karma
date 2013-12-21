// CopyRights Makarand Patwardhan!
// adds a news ticker style suggestions to input or textarea
// by changing its placeholder values.
// 1st parameter is string representing the id of the element
// 2nd parameter is the array of string to be used as tickers.
// Need jquery.
function applyTicker(targetid, tickerMessages) {
    var tickerelement = $("#" + targetid);
    var timerId = 0;
    var row = 0;
    var col = 1;

    function timerfunction() {
        tickerelement.attr('placeholder', tickerMessages[row].substring(0, col));
        if (++col == tickerMessages[row].length) {
            col = 1;
            row = ++row % tickerMessages.length;
        }
    }

    // on gaining focus clear the timer.
    tickerelement.focus(function () {
        var input = $(this);
        input.attr('placeholder', tickerMessages[row]);
        if (timerId) clearInterval(timerId);
    });

    // or loosing focus start timer if the placeholder==text.
    tickerelement.blur(function () {
        var input = $(this);
        if (timerId) clearInterval(timerId);

        if (input.val() == '' || input.val() == input.attr('placeholder')) {
            timerId = setInterval(timerfunction, 100);
        }
    });
    timerId = setInterval(timerfunction, 100);
}
