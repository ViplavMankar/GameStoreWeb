// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
window.getCookie = function (cookieName) {
    const match = document.cookie.match(new RegExp('(^| )' + cookieName + '=([^;]+)'));
    return match ? match[2] : null;
};
window.registerEndSession = function (sessionId) {
    console.log("sessionId " + sessionId);
    if (!sessionId) return;

    window.addEventListener("beforeunload", function () {
        // const ok = navigator.sendBeacon(
        //     "https://localhost:7054/api/sessions/end-session",
        //     new Blob([JSON.stringify({ SessionId: sessionId })], { type: "application/json" })
        // );
        // console.log("Beacon queued:", ok);
        fetch(`${apiBaseUrl}/api/sessions/end-session`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ SessionId: sessionId }),
            keepalive: true
          });
    });
};