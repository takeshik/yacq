var getPath = function () {
    return location.pathname.replace(/.html$/, '');
};
var getFileName = function () {
    return getPath().substr(1).split('/').slice(-1)[0].replace(/.html$/, '');
};
var getLang = function () {
    return (function (s) {
        return (s.indexOf('.') < 0 ? '' : s.split('.').slice(-1)[0]);
    })(getFileName());
};
var allHeaders = Enumerable.from($('h2, h3, h4, h5, h6').toEnumerable().doAction(function (e, i) {
    return e.attr('id', i + 1);
}).select(function (e, i) {
    return ({
        'index': i + 1,
        'level': parseInt(e.get(0).tagName.substr(1)),
        'text': e.text(),
        'position': e.offset().top
    });
}).toArray());
$(function () {
    (function () {
        var $navbar = $('#nav_wrap');
        var height = $navbar.offset().top;
        $(window).scroll(function () {
            if($(window).scrollTop() >= height) {
                $navbar.css('position', 'fixed');
                $navbar.css('top', 0);
                $('#main_content_wrap').css('margin-top', '60px');
            } else {
                $navbar.css('position', 'static');
                $('#main_content_wrap').css('margin-top', '0');
            }
        });
    })();
    (function () {
        $('h1').eq(0).attr('id', 0);
        $(window).scroll(function () {
            var headers = Enumerable.from(allHeaders.takeWhile(function (e) {
                return e.position <= $(window).scrollTop();
            }).reverse().distinct(function (e) {
                return e.level;
            }).orderBy(function (e) {
                return e.level;
            }).aggregate([
                {
                    'index': 0,
                    'level': 1,
                    'text': page.title,
                    'position': 0
                }
            ], function (a, e) {
                return e.position > a[a.length - 1].position ? a.concat(e) : a;
            }));
            $('#nav_location').empty().append(headers.select(function (e) {
                return $('<li/>').addClass('menu_item header').append($('<a/>').attr('href', '#' + e.index).text(e.text));
            }).alternate($('<li/>').text('/')).tojQuery());
        });
    })();
    (function () {
        var _this = this;
        $('.menu_item.lang').map(function () {
            return $(_this).find('a').attr('href', '.' + (function (s) {
                return s.substr(0, s.length - getLang().length);
            })(getPath()) + $(_this).text());
        });
    })();
});
