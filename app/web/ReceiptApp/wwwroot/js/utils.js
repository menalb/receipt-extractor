var ReceiptApp = ReceiptApp || {};

ReceiptApp.saveAsFile = function(filename, bytesBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
};

ReceiptApp.browserResize = {
    getInnerHeight: function () {
        return window.innerHeight;
    },
    getInnerWidth: function () {
        return window.innerWidth;
    },
    registerResizeCallback: function (dotNetObject) {
        window.addEventListener("resize", () => {
            ReceiptApp.browserResize.resized(dotNetObject);
        });
    },
    resized: function (obj) {
        obj.invokeMethodAsync('SetBrowserDimensions', window.innerWidth, window.innerHeight)
            .then(data => data)
            .catch(error => {
                console.log("Error during browser resize: " + error);
            });
    }
}