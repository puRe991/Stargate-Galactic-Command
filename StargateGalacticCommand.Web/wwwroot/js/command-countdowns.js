(function () {
    'use strict';

    var selector = '.build-timer, .research-timer, .fleet-timer, .mission-timer, .cooldown-timer, [data-countdown], [data-completes]';
    var timers = Array.prototype.slice.call(document.querySelectorAll(selector));
    if (timers.length === 0) {
        return;
    }

    function pad(value) {
        return String(value).padStart(2, '0');
    }

    function format(secondsLeft) {
        var days = Math.floor(secondsLeft / 86400);
        var hours = Math.floor((secondsLeft % 86400) / 3600);
        var minutes = Math.floor((secondsLeft % 3600) / 60);
        var seconds = secondsLeft % 60;
        var clock = pad(hours) + ':' + pad(minutes) + ':' + pad(seconds);
        return days > 0 ? days + 'T ' + clock : clock;
    }

    function enableTarget(element) {
        var enableSelector = element.getAttribute('data-enable-selector');
        if (!enableSelector) {
            return;
        }
        var target = document.querySelector(enableSelector);
        if (target) {
            target.disabled = false;
        }
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
        if (secondsLeft === 0) {
            element.textContent = element.getAttribute('data-ready-text') || '00:00:00';
            enableTarget(element);
        } else {
            element.textContent = format(secondsLeft);
        }
        element.classList.toggle('timer-complete', secondsLeft === 0);
    }

    function tick() {
        timers.forEach(render);
    }

    tick();
    window.setInterval(tick, 1000);
}());

(function () {
    'use strict';

    var values = Array.prototype.slice.call(document.querySelectorAll('.res-value[data-rate]'));
    if (values.length === 0) {
        return;
    }

    var start = Date.now();
    var bases = values.map(function (el) { return parseFloat(el.getAttribute('data-base')) || 0; });
    var rates = values.map(function (el) { return parseFloat(el.getAttribute('data-rate')) || 0; });

    function tick() {
        var elapsedSeconds = (Date.now() - start) / 1000;
        values.forEach(function (el, i) {
            var value = Math.max(0, Math.floor(bases[i] + rates[i] / 3600 * elapsedSeconds));
            el.textContent = value.toLocaleString('de-DE');
        });
    }

    tick();
    window.setInterval(tick, 1000);
}());
