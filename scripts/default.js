/// <reference path="jquery-2.0.0-vsdoc.js"/>

$(function () {
    var path = location.pathname;
    if (path == '/') {
        path = '/index';
    }
    path = path.substring(path.lastIndexOf('/') + 1);
    var pagename = path.replace(/\.html?$/, '');
    var lang = (pagename.indexOf('.') >= 0) ? pagename.substring(pagename.lastIndexOf('.') + 1) : 'en';
    if (lang != 'en') {
        pagename = pagename.substring(0, pagename.lastIndexOf('.'));
    }

    $('.menu_item.lang').map(function () {
        var $this = $(this);
        if ($this.text() != lang) {
            $this.find('a').attr('href', pagename + ($this.text() == 'en' ? '' : ('.' + $this.text())));
        }
    })
});