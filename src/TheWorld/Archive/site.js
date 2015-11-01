// site.js
$(document).ready(function () {
    //var ele = document.getElementById("username");
    //ele.innerHTML = "Andre Roussakoff";
    var ele = $("#username");
    ele.text = "Andre Roussakoff";

    //var main = document.getElementById("main");
    var main = $("#main");

    //main.onmouseenter = function () {
    //    main.style = "background-color: #888";
    //};
    main.on("mouseenter", function () {
        main.style = "background-color: #888";
    });

    //main.onmouseleave = function () {
    //    main.style = "";
    //};
    main.on("mouseleave", function () {
        main.style = "";
    });

    var menuItems = $("ul.menu li a");
    menuItems.on("click", function () {
        var me = $(this);
        alert(me.text());
    });
});