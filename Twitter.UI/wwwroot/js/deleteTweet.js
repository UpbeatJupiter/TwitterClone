var deleteTweet = {

    deleteTweet: function (tweetid,userid) {
        var id = tweetid;


        $.post("/Home/DeleteTweet",
            {
                tweetid: id

            },
            function (response) {
                followTheme.getAllTweets(userid);
            }
        );
    }
}
