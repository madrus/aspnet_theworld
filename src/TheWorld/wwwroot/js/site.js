// site.js
//$(document).ready(function () {
$(function () {
    // placing $ before the variable name is a reminder
    // that it is a jQuery object
    var $sidebarAndWrapper = $("#sidebar, #wrapper");
    $("#sidebarToggle").on("click", function () {
        $sidebarAndWrapper.toggleClass("hide-sidebar");
        if ($sidebarAndWrapper.hasClass("hide-sidebar")) {
            $(this).text("Show Sidebar");
        } else {
            $(this).text("Hide Sidebar");
        }
    })
});