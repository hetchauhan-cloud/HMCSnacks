// Constants
const ERRORTYPE = "error";
const SUCESSTYPE = "success";
const INFORMATIONTYPE = "info";
const WARRNIGTYPE = "warning";
const INLINEALERTSHOWTIME = 3000; // milliseconds

function ShowAlertInline(message, messageType) {
    const alertBox = document.getElementById('alertMessage');
    if (!alertBox) return;

    alertBox.className = 'alert text-center'; // reset
    alertBox.innerText = message;
    alertBox.style.display = 'block';

    switch (messageType.toLowerCase()) {
        case "error":
            alertBox.classList.add("alert-danger");
            break;
        case "success":
            alertBox.classList.add("alert-success");
            break;
        case "info":
            alertBox.classList.add("alert-info");
            break;
        case "warning":
            alertBox.classList.add("alert-warning");
            break;
    }

    setTimeout(() => {
        alertBox.style.opacity = '0';
        alertBox.style.transition = 'opacity 0.5s ease-out';
    }, 3000);
}


function HideAlertInline() {
    $('#alertMessage').fadeOut();
}
