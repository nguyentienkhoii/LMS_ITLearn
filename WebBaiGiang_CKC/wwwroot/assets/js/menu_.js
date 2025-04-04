
var menuTogglek = document.querySelector('.menu-toggle');
var menuk = document.querySelector('.thanh');

menuTogglek.addEventListener('click', function () {
    menuk.style.display = menuk.style.display === 'block' ? 'none' : 'block';
});

var menuToggle = document.querySelector('.close-btn');
var menu = document.querySelector('.thanh');

menuToggle.addEventListener('click', function () {
    menu.style.display = menu.style.display === 'block' ? 'none' : 'block';
});
