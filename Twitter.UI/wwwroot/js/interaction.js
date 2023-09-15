var interaction = {
    like: function (userid, tweetid) {
        var id = tweetid;
        var uid = userid;


        $.post("/Home/LikeTweet",
            {
                userid: uid,
                tweetid: id

            },
            function (response) {
                followTheme.getAllTweets(userid);
            }
        );
    },

    unlike: function (userid, tweetid) {
        var id = tweetid;
        var uid = userid;


        $.post("/Home/UnlikeTweet",
            {
                userid: uid,
                tweetid: id

            },
            function (response) {
                followTheme.getAllTweets(userid);
            }
        );
    },

    retweet: function (userid, tweetid) {
        var id = tweetid;
        var uid = userid;

        $.post("/Home/RetweetTweet",
            {
                userid: uid,
                tweetid: id
            },
            function (response) {
                followTheme.getAllTweets(userid);
            }
        );
    },

    unretweet: function (userid, tweetid) {
        var id = tweetid;
        var uid = userid;

        $.post("/Home/UnRetweetTweet",
            {
                userid: uid,
                tweetid: id
            },
            function (response) {
                followTheme.getAllTweets(userid);
            }
        );
    }

}