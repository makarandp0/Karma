
$(document).ready(function () {
    $.mockjax({
        url: '/Api/getFriends',
        responseTime: 750,
        responseText: {
            errorcode: '',
            error: false,
            friends: [
                { name: "Guru Dutt", id: "gurudutt", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                { name: "Lata Mangeshkar", id: "latamangeshkar", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                { name: "Kishor Kumar", id: "kishorkumar", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                { name: "AR Reheman", id: "arreheman", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                { name: "Hridaynath Mangeshkar", id: "hridaynath", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                { name: "Rahat Fateh Ali Khan", id: "RahatFatehAliKhan", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" }
            ]
        }
    });

    // inbox
    // requestStatus = opened = 1, closed = 0
    // yourStatus = nostatus = 0, ignored = 1, offered = 2
    $.mockjax({
        url: '/Api/getInbox',
        responseTime: 750,
        responseText: {
            errorcode: '',
            error: false,
            inbox: [
                { 
                    from: { name: "Guru Dutt", id: "gurudutt", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    request: { name: "Needs some more time", id:"1", requestStatus:0, yourStatus: 0}
                },
                {
                    from: { name: "Rahat Fateh Ali Khan", id: "RahatFatehAliKhan", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    request: { name: "Needs some money management lessons", id: "2", requestStatus: 0, yourStatus: 0 }
                },
                {
                    from: { name: "Hridaynath Mangeshkar", id: "hridaynath", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    request: { name: "Needs a break", id: "3", requestStatus: 0, yourStatus: 0 }
                }
            ]
        }
    });

    // outbox
    // helpOffered :
    // helpIgnored :
    // helpAccepted :
    $.mockjax({
        url: '/Api/getOutBox',
        responseTime: 750,
        responseText: {
            errorcode: '',
            error: false,
            outbox: [
                {
                    name: "Need a Ride to Airport",
                    id: "1",
                    helpOffered: [
                        { name: "Guru Dutt", id: "gurudutt", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                        { name: "Lata Mangeshkar", id: "latamangeshkar", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    ],
                    helpIgnored: [
                        { name: "Lata Mangeshkar", id: "latamangeshkar", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    ],
                    helpAccepted: [
                        { name: "Guru Dutt", id: "gurudutt", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    ]
                },
                {
                    name: "Help feeding my cat",
                    id: "2",
                    helpOffered: [
                        { name: "Guru Dutt", id: "gurudutt", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                        { name: "Lata Mangeshkar", id: "latamangeshkar", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    ],
                    helpIgnored: [
                        { name: "Lata Mangeshkar", id: "latamangeshkar", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    ],
                    helpAccepted: [
                        { name: "Guru Dutt", id: "gurudutt", karmapoints: 0, pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg" },
                    ]
                }
            ]
        }
    });
});