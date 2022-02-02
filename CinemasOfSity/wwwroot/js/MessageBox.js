let messageBox = document.getElementById('messageBox');
let messageBoxContent = document.getElementById('messageBox-content');

const MessageBoxOpen = (messageHtml) => {
    if (!messageBox.classList.contains('open')) {
        messageBoxContent = document.getElementById('messageBox-content');
        messageBoxContent.innerHTML = messageHtml;
        messageBox.classList.add('open');
    }
}

const MessageBoxClose = () => {
    messageBox.classList.remove('open');
}

const WriteErrors = (errorList) => {
    let errorListHtml = '<ul id="errorList">'
    for (let error of errorList) {
        errorListHtml += `<li class="error">${error}</li>`
    }
    errorListHtml += '</ul>';
    MessageBoxOpen(errorListHtml);
}
