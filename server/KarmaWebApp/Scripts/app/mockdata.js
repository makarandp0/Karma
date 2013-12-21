
$(document).ready(function () {
    $.mockjax({
        url: '/Api/getFriends',
        responseTime: 750,
        responseText: {
            errorcode: '',
            error: false,
            friends: [
                { name: "Mak", id: "johnid", karmapoints: 0 },
                { name: "Jill", id: "Jill", karmapoints: 0 },
                { name: "Boo", id: "Boo", karmapoints: 0 }
            ]
        }
    });
});