$(document).ready(function () {
    var activeUrl = window.location.pathname;
    $('.navbar-nav li a[href="' + activeUrl + '"]').parent('li').addClass('active');

});