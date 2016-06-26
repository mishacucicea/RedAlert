$(function ()
{
    // Highlight the current tab.
    var currentTab = $(".dropdown").has(".dropdown-menu a[href='" + location.pathname + "']");
    currentTab.addClass("selected");
});