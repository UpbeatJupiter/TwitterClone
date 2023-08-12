var postTweet = {

    tweetCont: function (userid) {
        let input = $('#tweetContentInput').val();
        let date = Date.now;

        var tweetdata = {
            TweetContent: input,
            UserId: userid,
            TweetDate: date
        };

        $.post("/Home/PostTweets",
            {
                tweetdto: tweetdata
            },
            function (response) {
                followTheme.getAllTweets(userid);
            }
        );
    }
}
