/// <reference path="jquery.d.ts"/>
/// <reference path="jquery-2.0.0-vsdoc.js"/>
/// <reference path="linq.js.d.ts"/>
/// <reference path="linq-vsdoc.js"/>
/// <reference path="linq.jquery.js"/>
/// <reference path="linq.jquery.d.ts"/>
/// <reference path="rx.min.js"/>
/// <reference path="rx.js.d.ts"/>

var getPath = () => location.pathname
    .replace(/.html$/, '');

var getFileName = () => getPath()
    .substr(1)
    .split('/')
    .slice(-1)[0]
    .replace(/.html$/, '');

var getLang = () => ((s: string) =>
    (s.indexOf('.') < 0
        ? ''
        : s.split('.').slice(-1)[0]
    ))(getFileName());

var allHeaders = Enumerable.from($('h2, h3, h4, h5, h6')
    .toEnumerable()
    .doAction((e, i) => e.attr('id', i + 1))
    .select((e, i) => ({
        'index': i + 1,
        'level': parseInt(e.get(0).tagName.substr(1)),
        'text': e.text(),
        'position': e.offset().top,
    }))
    .toArray()
);

$(() => {
    // navigation bar: floating control
    () => {
        var $navbar = $('#nav_wrap');
        var height = $navbar.offset().top;
        $(window).scroll(() => {
            if ($(window).scrollTop() >= height) {
                $navbar.css('position', 'fixed');
                $navbar.css('top', 0);
                $('#main_content_wrap').css('margin-top', '30px');
            } else {
                $navbar.css('position', 'static');
                $('#main_content_wrap').css('margin-top', '0');
            }
        });
    } ();

    // navigation bar: document navigation control
    () => {
        $('h1').eq(0).attr('id', 0);
        $(window).scroll(() => {
            var headers = Enumerable.from(allHeaders
                .takeWhile(e => e.position <= $(window).scrollTop())
                .reverse()
                .distinct(e => e.level)
                .orderBy(e => e.level)
                .aggregate(
                    [{
                        'index': 0,
                        'level': 1,
                        'text': page.title,
                        'position': 0
                    }],
                    (a, e) => e.position > a[a.length - 1].position
                        ? a.concat(e)
                        : a
                )
            );
            $('#nav_location')
                .empty()
                .append(headers
                    .select(e => $('<li/>')
                        .addClass('menu_item header')
                        .append($('<a/>')
                            .attr('href', '#' + e.index)
                            .text(e.text)
                        )
                    )
                    .alternate($('<li/>').text('/'))
                    .tojQuery()
                );
        });
    }();

    // navigation bar: language switcher
    () => {
        $.map($('.menu_item.lang'), e => $(e)
            .find('a')
            .attr('href', ((s: string) =>
                s.substr(0, s.length - getLang().length)
            )(getPath()) + $(e).text())
        );
    }();
});
