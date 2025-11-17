var hasSubmenu = document.querySelectorAll('.has-submenu');

hasSubmenu.forEach(function (item) {
    var submenu = item.querySelector('.submenu');
    var link = item.querySelector('a');

    link.addEventListener('click', function (e) {
        e.preventDefault();
        submenu.style.display = submenu.style.display === 'block' ? 'none' : 'block';
    });
});


var hasSubmenu1 = document.querySelectorAll('.has-submenu1');

hasSubmenu1.forEach(function (item) {
    var submenu1 = item.querySelector('.submenu1');
    var link = item.querySelector('a');

    link.addEventListener('click', function (e) {
        e.preventDefault();
        submenu1.style.display = submenu1.style.display === 'block' ? 'none' : 'block';
    });
});

var menuToggle11 = document.querySelector('.menu-heder1');
var menu11 = document.querySelector('.mini');

menuToggle11.addEventListener('click', function () {
    menu11.style.display = menu11.style.display === 'block' ? 'none' : 'block';
});




function toggleContent() {
    var content = document.getElementById('mydiv');
    var label = document.querySelector('.lwptoc_toggle_label');

    if (content.style.display === 'none') {
        content.style.display = 'block';
        label.setAttribute('data-label', 'Ẩn');
        label.innerHTML = '[Ẩn]';
    } else {
        content.style.display = 'none';
        label.setAttribute('data-label', 'Hiện');
        label.innerHTML = '[Hiện]';
    }
}


///mau chu 

const menuItems = document.querySelectorAll('.menu-item a');
let selectedMenuItem = null;

menuItems.forEach(item => {
    item.addEventListener('click', () => {
        if (selectedMenuItem) {
            selectedMenuItem.classList.remove('active');
        }
        selectedMenuItem = item;
        item.classList.add('active');
        localStorage.setItem('selectedMenuItem', item.id);
    });
});

const selectedMenuItemId = localStorage.getItem('selectedMenuItem');
if (selectedMenuItemId) {
    const selectedMenuItem = document.getElementById(selectedMenuItemId);
    if (selectedMenuItem) {
        selectedMenuItem.classList.add('active');
    }
}


var menupro = document.getElementById('menu-pro');
var dropmemu = document.getElementById('container-dn');

if (dropmemu !== null) {
    dropmemu.addEventListener('click', function () {
        menupro.classList.toggle('active');
        dropmemu.classList.toggle('active');
    });
}

