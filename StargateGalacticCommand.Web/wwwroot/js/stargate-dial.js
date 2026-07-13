(function () {
    'use strict';

    var root = document.getElementById('stargateAnimation');
    if (!root) {
        return;
    }

    var chevrons = Array.prototype.slice.call(root.querySelectorAll('.chevron'));
    var horizon = root.querySelector('.event-horizon');
    var ring = root.querySelector('.gate-ring');
    if (chevrons.length === 0 || !horizon || !ring) {
        return;
    }

    if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        chevrons.forEach(function (c) { c.classList.add('lit'); });
        ring.classList.add('locked');
        horizon.classList.add('active');
        return;
    }

    var LOCK_INTERVAL = 420;
    var HOLD_DURATION = 6000;
    var UNLOCK_INTERVAL = 220;
    var PAUSE = 1800;

    function dial(index) {
        if (index >= chevrons.length) {
            ring.classList.add('locked');
            window.setTimeout(function () { horizon.classList.add('burst'); }, 250);
            window.setTimeout(function () { horizon.classList.add('active'); }, 950);
            window.setTimeout(unlock, 950 + HOLD_DURATION);
            return;
        }
        chevrons[index].classList.add('lit');
        window.setTimeout(function () { dial(index + 1); }, LOCK_INTERVAL);
    }

    function unlock(index) {
        index = index || 0;
        if (index === 0) {
            horizon.classList.remove('active', 'burst');
            ring.classList.remove('locked');
        }
        if (index >= chevrons.length) {
            window.setTimeout(startCycle, PAUSE);
            return;
        }
        chevrons[chevrons.length - 1 - index].classList.remove('lit');
        window.setTimeout(function () { unlock(index + 1); }, UNLOCK_INTERVAL);
    }

    function startCycle() {
        window.setTimeout(function () { dial(0); }, 400);
    }

    startCycle();
}());
