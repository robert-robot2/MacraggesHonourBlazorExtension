// Macragge's Honour - Menu
// popup.js - Toolbar dropdown menu logic

document.addEventListener('DOMContentLoaded', function () {     

    document.getElementById('blazor-btn').addEventListener('click', function () {
        chrome.tabs.create({ url: chrome.runtime.getURL('index.html') });
    });

});
