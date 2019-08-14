window.snakeFunctions = {
    startInputHandler: function (razorPage) {
        console.log("Starting input handler...");
        document.addEventListener('keydown', function (event) {
            console.log("Detected keydown", event);
            /* event.key is used by edge */
            razorPage.invokeMethodAsync("HandleInput", event.code || event.key)
                .then(response => console.log(response));
        });
    }
};