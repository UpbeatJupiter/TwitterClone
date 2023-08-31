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
    }
}