/// <reference path="jquery-2.0.0-vsdoc.js"/>

$(function () {
    var path = location.pathname;
    path = path.substring(0, path.lastIndexOf('/'));
    var pagename = path.replace(/(\.html)?(#.+)?$/, '');
    var lang = (pagename.indexOf('.') >= 0) ? pagename.substring(pagename.lastIndexOf('.')) : 'en';

    $('.menu_item.lang').map(function () {
        var $this = $(this);
        if ($this.text() != lang) {
            $this.attr('href', pagename + ($this.text() == 'en' ? '' : '.' + lang));
        }
    })
});