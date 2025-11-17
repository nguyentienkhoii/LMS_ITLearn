var menu1 = document.getElementById('menu');
var content = document.getElementById('content');
var toggleMenu = document.getElementById('toggle-menu');
var btndong = document.getElementById('btn-dong');

toggleMenu.addEventListener('click', function () {
    menu1.classList.toggle('active');
    btndong.classList.toggle('active');
    content.classList.toggle('active');
    toggleMenu.classList.toggle('active');
});
btndong.addEventListener('click', function () {
    menu1.classList.toggle('active');
    btndong.classList.toggle('active');
    content.classList.toggle('active');
    toggleMenu.classList.toggle('active');
});

