(function () {
    'use strict';

    var selector = '.build-timer, .research-timer, .fleet-timer, [data-countdown], [data-completes]';
    var timers = Array.prototype.slice.call(document.querySelectorAll(selector));
    if (timers.length === 0) {
        return;
    }

    function pad(value) {
        return String(value).padStart(2, '0');
    }

    function render(element) {
        var rawDate = element.getAttribute('data-completes') || element.getAttribute('data-countdown');
        if (!rawDate) {
            return;
        }

        var target = new Date(rawDate);
        if (Number.isNaN(target.getTime())) {
            element.classList.add('timer-invalid');
            return;
        }

        var secondsLeft = Math.max(0, Math.floor((target.getTime() - Date.now()) / 1000));
        var hours = Math.floor(secondsLeft / 3600);
        var minutes = Math.floor((secondsLeft % 3600) / 60);
        var seconds = secondsLeft % 60;

        element.textContent = pad(hours) + ':' + pad(minutes) + ':' + pad(seconds);
        element.classList.toggle('timer-complete', secondsLeft === 0);
    }

    function tick() {
        timers.forEach(render);
    }

    tick();
    window.setInterval(tick, 1000);
}());
